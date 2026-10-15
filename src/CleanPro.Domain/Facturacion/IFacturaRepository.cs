using CleanPro.Domain.Comun;

namespace CleanPro.Domain.Facturacion;

public interface IFacturaRepository
{
    Task AgregarAsync(Factura factura, CancellationToken ct = default);
    Task<bool> ExisteFacturaParaServicioAsync(ServicioId servicioId, CancellationToken ct = default);
}
