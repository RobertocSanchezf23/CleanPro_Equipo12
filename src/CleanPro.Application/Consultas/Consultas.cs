using CleanPro.Domain.Comun;
using CleanPro.Domain.Facturacion;

namespace CleanPro.Application.Consultas;

// ===== Lado Query de CQRS (ADR-004): solo lee y devuelve DTOs planos; nunca cambia el estado. =====

// DTOs de lectura: NO se reutilizan para escribir.
public sealed record ServicioProgramadoDto(Guid ServicioId, DateTime FechaHora, string Ubicacion, Guid CuadrillaId,
    Guid SupervisorId, string Estado);

public sealed record ServicioAsignadoDto(Guid ServicioId, DateTime FechaHora, string Ubicacion);

public sealed record FacturacionDto(Guid ServicioId, Guid FacturaId, decimal Total, string EstadoDePago,
    int NumeroDeIncidencias);

// Puertos de lectura (los implementa infraestructura).
public interface IConsultasDeServicios
{
    Task<IReadOnlyList<ServicioProgramadoDto>> ServiciosProgramadosAsync(DateTime desde, DateTime hasta,
        CancellationToken ct = default);

    Task<IReadOnlyList<ServicioAsignadoDto>> ServiciosDeCuadrillaAsync(CuadrillaId cuadrillaId, DateOnly fecha,
        CancellationToken ct = default);
}

public interface IConsultasDeFacturacion
{
    Task<IReadOnlyList<FacturacionDto>> FacturacionPorServicioAsync(EstadoDePago? estado, CancellationToken ct = default);
}

// Queries
public sealed record ServiciosProgramadosQuery(DateTime Desde, DateTime Hasta);          // REP-02, US-03
public sealed record ServiciosDeCuadrillaQuery(CuadrillaId CuadrillaId, DateOnly Fecha); // US-17, REP-05
public sealed record FacturacionPorServicioQuery(EstadoDePago? EstadoDePago);           // REP-04, US-21
