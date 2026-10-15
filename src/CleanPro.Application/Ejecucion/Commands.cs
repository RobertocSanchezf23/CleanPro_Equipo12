using CleanPro.Domain.Comun;
using CleanPro.Domain.Servicios;

namespace CleanPro.Application.Ejecucion;

/// <summary>
/// CU-04. Para un servicio adicional en sitio se indica el código del catálogo (RB-12);
/// para material especial se captura el monto (RB-14).
/// </summary>
public sealed record RegistrarIncidenciaCommand(
    ServicioId ServicioId,
    SupervisorId SupervisorId,
    TipoDeIncidencia Tipo,
    string Descripcion,
    DateTime RegistradaEn,
    string? CodigoExtra = null,
    decimal? MontoCargo = null);

public sealed record RegistrarAccesoDenegadoCommand(
    ServicioId ServicioId,
    SupervisorId SupervisorId,
    int MinutosEspera,
    DateTime RegistradaEn);

public sealed record AutorizarCargoAdicionalCommand(
    ServicioId ServicioId,
    IncidenciaId IncidenciaId,
    CoordinadorId CoordinadorId);

public sealed record RegistrarInicioCommand(ServicioId ServicioId, DateTime FechaHora);

public sealed record RegistrarTerminacionCommand(ServicioId ServicioId, DateTime FechaHora);
