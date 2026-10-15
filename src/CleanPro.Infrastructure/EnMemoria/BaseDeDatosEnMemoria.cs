using CleanPro.Domain.Comun;
using CleanPro.Domain.Cotizaciones;
using CleanPro.Domain.Facturacion;
using CleanPro.Domain.Servicios;

namespace CleanPro.Infrastructure.EnMemoria;

/// <summary>
/// Almacenamiento ficticio (el caso es académico y no se construye base de datos).
/// En la implementación real propuesta sería un CleanProDbContext de EF Core.
/// </summary>
public sealed class BaseDeDatosEnMemoria
{
    public Dictionary<CotizacionId, Cotizacion> Cotizaciones { get; } = new();
    public Dictionary<ServicioId, Servicio> Servicios { get; } = new();
    public Dictionary<FacturaId, Factura> Facturas { get; } = new();
}
