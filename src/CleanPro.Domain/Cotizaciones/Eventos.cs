using CleanPro.Domain.Comun;

namespace CleanPro.Domain.Cotizaciones;

public sealed record CotizacionEnviada(CotizacionId CotizacionId, ClienteId ClienteId, Dinero Precio, DateTime VigenteHasta) : EventoDeDominio;
public sealed record CotizacionAceptada(CotizacionId CotizacionId) : EventoDeDominio;
public sealed record CotizacionRechazada(CotizacionId CotizacionId) : EventoDeDominio;
public sealed record CotizacionVencida(CotizacionId CotizacionId) : EventoDeDominio;
