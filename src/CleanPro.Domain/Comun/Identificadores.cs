namespace CleanPro.Domain.Comun;

// Identificadores tipados: evitan confundir el Id de una cotización con el de un servicio.
public readonly record struct CotizacionId(Guid Valor) { public static CotizacionId Nuevo() => new(Guid.NewGuid()); }
public readonly record struct ServicioId(Guid Valor) { public static ServicioId Nuevo() => new(Guid.NewGuid()); }
public readonly record struct IncidenciaId(Guid Valor) { public static IncidenciaId Nuevo() => new(Guid.NewGuid()); }
public readonly record struct FacturaId(Guid Valor) { public static FacturaId Nuevo() => new(Guid.NewGuid()); }

// Actores y recursos que el dominio solo referencia por Id (ver mapa de dominio, sección 3.5).
public readonly record struct ClienteId(Guid Valor) { public static ClienteId Nuevo() => new(Guid.NewGuid()); }
public readonly record struct CuadrillaId(Guid Valor) { public static CuadrillaId Nuevo() => new(Guid.NewGuid()); }
public readonly record struct SupervisorId(Guid Valor) { public static SupervisorId Nuevo() => new(Guid.NewGuid()); }
public readonly record struct CoordinadorId(Guid Valor) { public static CoordinadorId Nuevo() => new(Guid.NewGuid()); }
