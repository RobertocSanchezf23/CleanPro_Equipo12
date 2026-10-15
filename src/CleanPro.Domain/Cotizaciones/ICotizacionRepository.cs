using CleanPro.Domain.Comun;

namespace CleanPro.Domain.Cotizaciones;

/// <summary>
/// Repositorio del Aggregate Cotizacion, en términos del negocio.
/// Contrato (SOLID S-4): ObtenerPorIdAsync devuelve null si no existe;
/// AgregarAsync solo registra el objeto: nada se guarda hasta IUnitOfWork.ConfirmarAsync().
/// </summary>
public interface ICotizacionRepository
{
    Task<Cotizacion?> ObtenerPorIdAsync(CotizacionId id, CancellationToken ct = default);
    Task AgregarAsync(Cotizacion cotizacion, CancellationToken ct = default);
}
