using CleanPro.Application.Consultas;
using CleanPro.Application.Ejecucion;
using CleanPro.Application.Facturacion;
using CleanPro.Domain.Comun;
using CleanPro.Domain.Servicios;
using Xunit;

namespace CleanPro.Tests;

public class FacturacionTests
{
    private static readonly DateTime Jueves = Escenario.Lunes9am.AddDays(3);

    [Fact(DisplayName = "CU-05: servicio terminado = precio acordado + cargos autorizados (RES-04)")]
    public async Task FacturaTerminadoConCargoAutorizado()
    {
        var e = new Escenario();
        var servicioId = await e.ServicioEnEjecucionAsync(); // 100 m² residencial = $1,200
        var incidenciaId = await e.Sistema.Ejecucion.RegistrarIncidencia(new RegistrarIncidenciaCommand(servicioId,
            e.Supervisor, TipoDeIncidencia.ServicioAdicionalEnSitio, "Ventanas", Jueves, CodigoExtra: "VENTANAS"));
        await e.Sistema.Ejecucion.AutorizarCargoAdicional(new AutorizarCargoAdicionalCommand(servicioId, incidenciaId, e.Coordinador));
        await e.Sistema.Ejecucion.RegistrarTerminacion(new RegistrarTerminacionCommand(servicioId, Jueves.AddHours(4)));

        var facturaId = await e.Sistema.Facturacion.FacturarServicio(new FacturarServicioCommand(servicioId, Jueves.AddHours(5)));

        Assert.Equal(1450m, e.Sistema.BaseDeDatos.Facturas[facturaId].Total.Monto); // 1200 + 250
    }

    [Fact(DisplayName = "RB-14: no se factura con cargos pendientes de autorización (y no se guarda nada)")]
    public async Task NoFacturarConCargoPendiente()
    {
        var e = new Escenario();
        var servicioId = await e.ServicioEnEjecucionAsync();
        await e.Sistema.Ejecucion.RegistrarIncidencia(new RegistrarIncidenciaCommand(servicioId, e.Supervisor,
            TipoDeIncidencia.MaterialEspecial, "Desengrasante industrial", Jueves, MontoCargo: 120m));
        await e.Sistema.Ejecucion.RegistrarTerminacion(new RegistrarTerminacionCommand(servicioId, Jueves.AddHours(4)));

        await Assert.ThrowsAsync<CargosPendientesDeAutorizacion>(() =>
            e.Sistema.Facturacion.FacturarServicio(new FacturarServicioCommand(servicioId, Jueves.AddHours(5))));
        Assert.Empty(e.Sistema.BaseDeDatos.Facturas);
    }

    [Fact(DisplayName = "RB-07: no se factura un servicio que sigue en ejecución")]
    public async Task NoFacturarEnEjecucion()
    {
        var e = new Escenario();
        var servicioId = await e.ServicioEnEjecucionAsync();

        await Assert.ThrowsAsync<ServicioNoFacturable>(() =>
            e.Sistema.Facturacion.FacturarServicio(new FacturarServicioCommand(servicioId, Jueves)));
    }

    [Fact(DisplayName = "RB-11 + H-01: servicio no realizado se factura con cargo de cancelación del 50%")]
    public async Task FacturaNoRealizado()
    {
        var e = new Escenario();
        var servicioId = await e.ServicioProgramadoAsync();
        await e.Sistema.Ejecucion.RegistrarAccesoDenegado(new RegistrarAccesoDenegadoCommand(servicioId, e.Supervisor, 15, Jueves));

        var facturaId = await e.Sistema.Facturacion.FacturarServicio(new FacturarServicioCommand(servicioId, Jueves));

        Assert.Equal(600m, e.Sistema.BaseDeDatos.Facturas[facturaId].Total.Monto);
    }

    [Fact(DisplayName = "SUP-04: un servicio no se factura dos veces")]
    public async Task UnaFacturaPorServicio()
    {
        var e = new Escenario();
        var servicioId = await e.ServicioProgramadoAsync();
        await e.Sistema.Ejecucion.RegistrarAccesoDenegado(new RegistrarAccesoDenegadoCommand(servicioId, e.Supervisor, 20, Jueves));
        await e.Sistema.Facturacion.FacturarServicio(new FacturarServicioCommand(servicioId, Jueves));

        await Assert.ThrowsAsync<FacturaYaEmitida>(() =>
            e.Sistema.Facturacion.FacturarServicio(new FacturarServicioCommand(servicioId, Jueves)));
    }

    [Fact(DisplayName = "REP-04 (Query): la facturación se consulta con un DTO de lectura")]
    public async Task ConsultaDeFacturacion()
    {
        var e = new Escenario();
        var servicioId = await e.ServicioProgramadoAsync();
        await e.Sistema.Ejecucion.RegistrarAccesoDenegado(new RegistrarAccesoDenegadoCommand(servicioId, e.Supervisor, 15, Jueves));
        await e.Sistema.Facturacion.FacturarServicio(new FacturarServicioCommand(servicioId, Jueves));

        var filas = await e.Sistema.FacturacionPorServicio.Handle(new FacturacionPorServicioQuery(null));

        var fila = Assert.Single(filas);
        Assert.Equal(600m, fila.Total);
        Assert.Equal("Pendiente", fila.EstadoDePago);
        Assert.Equal(1, fila.NumeroDeIncidencias);
    }

    [Fact(DisplayName = "US-17 (Query): la cuadrilla consulta sus servicios del día con la ubicación")]
    public async Task ConsultaDeServiciosDeCuadrilla()
    {
        var e = new Escenario();
        await e.ServicioProgramadoAsync();

        var servicios = await e.Sistema.ServiciosDeCuadrilla.Handle(
            new ServiciosDeCuadrillaQuery(e.Cuadrilla, DateOnly.FromDateTime(Jueves)));

        Assert.Contains("San Nicolás", Assert.Single(servicios).Ubicacion);
    }
}
