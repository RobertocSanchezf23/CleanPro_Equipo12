using CleanPro.Domain.Comun;

namespace CleanPro.Domain.Servicios;

/// <summary>
/// Repositorio del Aggregate Servicio (con sus incidencias). Los reportes NO van aquí,
/// van en las consultas del lado Query (hallazgo SOLID S-5).
/// </summary>
public interface IServicioRepository
{
    Task<Servicio?> ObtenerPorIdAsync(ServicioId id, CancellationToken ct = default);
    Task AgregarAsync(Servicio servicio, CancellationToken ct = default);
    Task<bool> CuadrillaTieneServicioEnAsync(CuadrillaId cuadrillaId, DateTime fechaHora, CancellationToken ct = default);
}
