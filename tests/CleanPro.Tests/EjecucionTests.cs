using CleanPro.Application.Ejecucion;
using CleanPro.Domain.Comun;
using CleanPro.Domain.Servicios;
using Xunit;

namespace CleanPro.Tests;

public class EjecucionTests
{
    [Fact(DisplayName = "RB-06 / PA-01: toda incidencia registrada llega a administración")]
    public async Task IncidenciaNotificaAAdministracion()
    {
        var e = new Escenario();
        var servicioId = await e.ServicioEnEjecucionAsync();

        await e.Sistema.Ejecucion.RegistrarIncidencia(new RegistrarIncidenciaCommand(servicioId, e.Supervisor,
            TipoDeIncidencia.OtroCambio, "El cliente pidió cambiar el orden de las habitaciones", Escenario.Lunes9am.AddDays(3)));

        Assert.Contains(e.Sistema.Publicador.Publicados, ev => ev is IncidenciaRegistrada);
        Assert.Contains(e.Sistema.Buzon.Mensajes, m => m.StartsWith("[Administración]"));
    }

    [Fact(DisplayName = "RB-12: un servicio adicional en sitio nace con cargo pendiente de autorización")]
    public async Task ServicioAdicionalQuedaPendiente()
    {
        var e = new Escenario();
        var servicioId = await e.ServicioEnEjecucionAsync();

        await e.Sistema.Ejecucion.RegistrarIncidencia(new RegistrarIncidenciaCommand(servicioId, e.Supervisor,
            TipoDeIncidencia.ServicioAdicionalEnSitio, "Limpieza de ventanas", Escenario.Lunes9am.AddDays(3),
            CodigoExtra: "VENTANAS"));

        var servicio = e.Sistema.BaseDeDatos.Servicios[servicioId];
        Assert.True(servicio.Incidencias.Single().TieneCargoPendiente());
    }

    [Fact(DisplayName = "RES-06: con menos de 15 min de espera no se puede declarar acceso denegado")]
    public async Task EsperaInsuficiente()
    {
        var e = new Escenario();
        var servicioId = await e.ServicioProgramadoAsync();

        await Assert.ThrowsAsync<TiempoDeEsperaInsuficiente>(() => e.Sistema.Ejecucion.RegistrarAccesoDenegado(
            new RegistrarAccesoDenegadoCommand(servicioId, e.Supervisor, 10, Escenario.Lunes9am.AddDays(3))));
        Assert.Equal(EstadoServicio.Programado, e.Sistema.BaseDeDatos.Servicios[servicioId].Estado);
    }

    [Fact(DisplayName = "RB-11: tras 15 min sin acceso el servicio queda No realizado")]
    public async Task AccesoDenegado()
    {
        var e = new Escenario();
        var servicioId = await e.ServicioProgramadoAsync();

        var estado = await e.Sistema.Ejecucion.RegistrarAccesoDenegado(
            new RegistrarAccesoDenegadoCommand(servicioId, e.Supervisor, 15, Escenario.Lunes9am.AddDays(3)));

        Assert.Equal(EstadoServicio.NoRealizado, estado);
    }
}
