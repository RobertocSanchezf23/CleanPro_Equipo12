using CleanPro.Application.Cotizaciones;
using CleanPro.Application.Ejecucion;
using CleanPro.Application.ProgramacionDeServicios;
using CleanPro.Domain.Comun;
using CleanPro.Domain.Cotizaciones;
using CleanPro.Infrastructure;

namespace CleanPro.Tests;

/// <summary>Arma el sistema en memoria y deja listos los pasos previos que cada prueba necesita.</summary>
internal sealed class Escenario
{
    // 12/10/2026 es lunes.
    public static readonly DateTime Lunes9am = new(2026, 10, 12, 9, 0, 0);

    public SistemaCleanProEnMemoria Sistema { get; } = new();
    public ClienteId Cliente { get; } = ClienteId.Nuevo();
    public CoordinadorId Coordinador { get; } = CoordinadorId.Nuevo();
    public CuadrillaId Cuadrilla { get; } = CuadrillaId.Nuevo();
    public SupervisorId Supervisor { get; } = SupervisorId.Nuevo();

    public async Task<CotizacionId> CotizacionEnviadaAsync(decimal superficieM2 = 100m,
        TipoDeInmueble tipo = TipoDeInmueble.Residencial, TipoDeFrecuencia frecuencia = TipoDeFrecuencia.Unica,
        params string[] extras)
    {
        var id = await Sistema.Cotizaciones.SolicitarCotizacion(new SolicitarCotizacionCommand(
            Cliente, "Av. Universidad s/n, San Nicolás de los Garza", superficieM2, tipo, frecuencia, extras));
        await Sistema.Cotizaciones.EstimarYEnviarCotizacion(new EstimarYEnviarCotizacionCommand(
            id, Coordinador, DuracionHoras: 4m, PersonalNecesario: 3, FechaEnvio: Lunes9am));
        return id;
    }

    public async Task<CotizacionId> CotizacionAceptadaAsync(decimal superficieM2 = 100m)
    {
        var id = await CotizacionEnviadaAsync(superficieM2);
        await Sistema.Cotizaciones.ResponderCotizacion(new ResponderCotizacionCommand(id, true, Lunes9am.AddDays(1)));
        return id;
    }

    public async Task<ServicioId> ServicioProgramadoAsync(decimal superficieM2 = 100m)
    {
        var cotizacionId = await CotizacionAceptadaAsync(superficieM2);
        return await Sistema.Programacion.ProgramarServicio(new ProgramarServicioCommand(
            cotizacionId, Lunes9am.AddDays(3), Cuadrilla, Supervisor));
    }

    public async Task<ServicioId> ServicioEnEjecucionAsync(decimal superficieM2 = 100m)
    {
        var servicioId = await ServicioProgramadoAsync(superficieM2);
        await Sistema.Ejecucion.RegistrarInicio(new RegistrarInicioCommand(servicioId, Lunes9am.AddDays(3)));
        return servicioId;
    }
}
