using CleanPro.Domain.Comun;

namespace CleanPro.Domain.Servicios;

/// <summary>
/// Domain Service (RES-01). Necesita ver otros servicios ya programados,
/// por eso no puede vivir dentro de un solo Servicio.
/// Supuesto: se considera ocupada si ya tiene un servicio a la misma fecha y hora
/// (el BRD no define cuántos servicios puede atender una cuadrilla por día).
/// </summary>
public sealed class DisponibilidadDeCuadrilla
{
    private readonly IServicioRepository _servicios;

    public DisponibilidadDeCuadrilla(IServicioRepository servicios)
    {
        _servicios = servicios;
    }

    public async Task<bool> EstaDisponibleAsync(CuadrillaId cuadrillaId, DateTime fechaHora, CancellationToken ct = default) =>
        !await _servicios.CuadrillaTieneServicioEnAsync(cuadrillaId, fechaHora, ct);
}
