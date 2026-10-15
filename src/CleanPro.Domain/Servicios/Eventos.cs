using CleanPro.Domain.Comun;

namespace CleanPro.Domain.Servicios;

public sealed record ServicioProgramado(ServicioId ServicioId, ClienteId ClienteId, DateTime FechaHora, CuadrillaId CuadrillaId) : EventoDeDominio;

/// <summary>RB-06 / US-19: le interesa a administración. Resuelve PA-01.</summary>
public sealed record IncidenciaRegistrada(ServicioId ServicioId, IncidenciaId IncidenciaId, TipoDeIncidencia Tipo, string Descripcion) : EventoDeDominio;

public sealed record CargoAdicionalAutorizado(ServicioId ServicioId, IncidenciaId IncidenciaId, Dinero Monto) : EventoDeDominio;
public sealed record ServicioTerminado(ServicioId ServicioId) : EventoDeDominio;
public sealed record ServicioNoRealizado(ServicioId ServicioId) : EventoDeDominio;
