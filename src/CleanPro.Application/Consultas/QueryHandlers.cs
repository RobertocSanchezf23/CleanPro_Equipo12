namespace CleanPro.Application.Consultas;

public sealed class ServiciosProgramadosQueryHandler
{
    private readonly IConsultasDeServicios _consultas;

    public ServiciosProgramadosQueryHandler(IConsultasDeServicios consultas) => _consultas = consultas;

    public Task<IReadOnlyList<ServicioProgramadoDto>> Handle(ServiciosProgramadosQuery query, CancellationToken ct = default) =>
        _consultas.ServiciosProgramadosAsync(query.Desde, query.Hasta, ct);
}

public sealed class ServiciosDeCuadrillaQueryHandler
{
    private readonly IConsultasDeServicios _consultas;

    public ServiciosDeCuadrillaQueryHandler(IConsultasDeServicios consultas) => _consultas = consultas;

    public Task<IReadOnlyList<ServicioAsignadoDto>> Handle(ServiciosDeCuadrillaQuery query, CancellationToken ct = default) =>
        _consultas.ServiciosDeCuadrillaAsync(query.CuadrillaId, query.Fecha, ct);
}

public sealed class FacturacionPorServicioQueryHandler
{
    private readonly IConsultasDeFacturacion _consultas;

    public FacturacionPorServicioQueryHandler(IConsultasDeFacturacion consultas) => _consultas = consultas;

    public Task<IReadOnlyList<FacturacionDto>> Handle(FacturacionPorServicioQuery query, CancellationToken ct = default) =>
        _consultas.FacturacionPorServicioAsync(query.EstadoDePago, ct);
}
