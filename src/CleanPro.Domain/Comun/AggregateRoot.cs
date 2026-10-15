namespace CleanPro.Domain.Comun;

/// <summary>Hecho del negocio que ya ocurrió y que puede interesar a otros componentes.</summary>
public abstract record EventoDeDominio
{
    public DateTime OcurrioEn { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Base de las raíces de Aggregate (Cotizacion, Servicio, Factura).
/// Acumula los Domain Events; la Unit of Work los extrae y publica al confirmar (ADR-002).
/// </summary>
public abstract class AggregateRoot
{
    private readonly List<EventoDeDominio> _eventos = new();

    public IReadOnlyCollection<EventoDeDominio> Eventos => _eventos.AsReadOnly();

    protected void Registrar(EventoDeDominio evento) => _eventos.Add(evento);

    public IReadOnlyList<EventoDeDominio> ExtraerEventos()
    {
        var copia = _eventos.ToList();
        _eventos.Clear();
        return copia;
    }
}
