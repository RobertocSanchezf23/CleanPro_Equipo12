using CleanPro.Application.Consultas;
using CleanPro.Domain.Comun;
using CleanPro.Domain.Facturacion;
using CleanPro.Domain.Servicios;

namespace CleanPro.Infrastructure.EnMemoria;

/// <summary>
/// Lado Query (ADR-004): arma DTOs planos leyendo directamente el almacenamiento,
/// sin pasar por los Application Services ni modificar Aggregates.
/// En la versión real serían consultas SQL de solo lectura.
/// </summary>
public sealed class ConsultasEnMemoria : IConsultasDeServicios, IConsultasDeFacturacion
{
    private readonly BaseDeDatosEnMemoria _db;

    public ConsultasEnMemoria(BaseDeDatosEnMemoria db) => _db = db;

    public Task<IReadOnlyList<ServicioProgramadoDto>> ServiciosProgramadosAsync(DateTime desde, DateTime hasta,
        CancellationToken ct = default)
    {
        IReadOnlyList<ServicioProgramadoDto> resultado = _db.Servicios.Values
            .Where(s => s.Programacion.FechaHora >= desde && s.Programacion.FechaHora <= hasta)
            .OrderBy(s => s.Programacion.FechaHora)
            .Select(s => new ServicioProgramadoDto(s.Id.Valor, s.Programacion.FechaHora, UbicacionDe(s),
                s.CuadrillaId.Valor, s.SupervisorId.Valor, s.Estado.ToString()))
            .ToList();
        return Task.FromResult(resultado);
    }

    public Task<IReadOnlyList<ServicioAsignadoDto>> ServiciosDeCuadrillaAsync(CuadrillaId cuadrillaId, DateOnly fecha,
        CancellationToken ct = default)
    {
        IReadOnlyList<ServicioAsignadoDto> resultado = _db.Servicios.Values
            .Where(s => s.CuadrillaId == cuadrillaId
                        && DateOnly.FromDateTime(s.Programacion.FechaHora) == fecha
                        && s.Estado == EstadoServicio.Programado)
            .OrderBy(s => s.Programacion.FechaHora)
            .Select(s => new ServicioAsignadoDto(s.Id.Valor, s.Programacion.FechaHora, UbicacionDe(s)))
            .ToList();
        return Task.FromResult(resultado);
    }

    public Task<IReadOnlyList<FacturacionDto>> FacturacionPorServicioAsync(EstadoDePago? estado, CancellationToken ct = default)
    {
        IReadOnlyList<FacturacionDto> resultado = _db.Facturas.Values
            .Where(f => estado is null || f.EstadoDePago == estado)
            .Select(f => new FacturacionDto(f.ServicioId.Valor, f.Id.Valor, f.Total.Monto, f.EstadoDePago.ToString(),
                _db.Servicios.TryGetValue(f.ServicioId, out var s) ? s.Incidencias.Count : 0))
            .ToList();
        return Task.FromResult(resultado);
    }

    private string UbicacionDe(Servicio servicio) =>
        _db.Cotizaciones.TryGetValue(servicio.CotizacionId, out var cotizacion)
            ? cotizacion.Inmueble.Ubicacion
            : "(sin ubicación)";
}
