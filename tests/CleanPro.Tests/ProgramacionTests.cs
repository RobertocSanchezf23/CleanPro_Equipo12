using CleanPro.Application.Cotizaciones;
using CleanPro.Application.ProgramacionDeServicios;
using CleanPro.Domain.Comun;
using CleanPro.Domain.Servicios;
using Xunit;

namespace CleanPro.Tests;

public class ProgramacionTests
{
    [Fact(DisplayName = "CU-03: programar con cotización aceptada notifica al cliente (US-03)")]
    public async Task ProgramarFlujoPrincipal()
    {
        var e = new Escenario();

        var servicioId = await e.ServicioProgramadoAsync();

        Assert.Contains(e.Sistema.Publicador.Publicados, ev => ev is ServicioProgramado sp && sp.ServicioId == servicioId);
        Assert.Contains(e.Sistema.Buzon.Mensajes, m => m.Contains("quedó programado"));
    }

    [Fact(DisplayName = "RB-02 / RES-02: no se programa si la cotización no fue aceptada")]
    public async Task NoProgramarSinAceptacion()
    {
        var e = new Escenario();
        var cotizacionId = await e.CotizacionEnviadaAsync(); // queda Pendiente

        await Assert.ThrowsAsync<CotizacionNoAceptada>(() => e.Sistema.Programacion.ProgramarServicio(
            new ProgramarServicioCommand(cotizacionId, Escenario.Lunes9am.AddDays(3), e.Cuadrilla, e.Supervisor)));
        Assert.Empty(e.Sistema.BaseDeDatos.Servicios);
    }

    [Fact(DisplayName = "RES-01: no se programa una cuadrilla ocupada a la misma fecha y hora")]
    public async Task CuadrillaOcupada()
    {
        var e = new Escenario();
        await e.ServicioProgramadoAsync();
        var otraCotizacion = await e.CotizacionAceptadaAsync();

        await Assert.ThrowsAsync<CuadrillaNoDisponible>(() => e.Sistema.Programacion.ProgramarServicio(
            new ProgramarServicioCommand(otraCotizacion, Escenario.Lunes9am.AddDays(3), e.Cuadrilla, e.Supervisor)));
        Assert.Single(e.Sistema.BaseDeDatos.Servicios);
    }
}
