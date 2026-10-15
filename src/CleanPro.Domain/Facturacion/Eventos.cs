using CleanPro.Domain.Comun;

namespace CleanPro.Domain.Facturacion;

/// <summary>US-05: el cliente recibe su factura.</summary>
public sealed record FacturaEmitida(FacturaId FacturaId, ServicioId ServicioId, Dinero Total) : EventoDeDominio;
