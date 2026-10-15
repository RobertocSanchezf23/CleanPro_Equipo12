using CleanPro.Application.Comun;
using CleanPro.Domain.Comun;

namespace CleanPro.Infrastructure.EnMemoria;

/// <summary>
/// Unit of Work en memoria (ADR-002).
/// - Los Aggregates nuevos se guardan hasta ConfirmarAsync.
/// - Los Domain Events de los Aggregates usados se publican DESPUÉS de confirmar.
/// Limitación conocida de la versión en memoria: un Aggregate ya existente se modifica por
/// referencia. No causa cambios a medias porque los métodos del dominio validan antes de
/// modificar; la versión real (EF Core + SaveChangesAsync) sí aísla los cambios hasta confirmar.
/// </summary>
public sealed class UnitOfWorkEnMemoria : IUnitOfWork
{
    private readonly IPublicadorDeEventos _publicador;
    private readonly HashSet<AggregateRoot> _rastreados = new();
    private readonly List<Action> _altasPendientes = new();

    public UnitOfWorkEnMemoria(IPublicadorDeEventos publicador)
    {
        _publicador = publicador;
    }

    public int Confirmaciones { get; private set; }

    internal void Rastrear(AggregateRoot aggregate) => _rastreados.Add(aggregate);

    internal void RegistrarAlta(AggregateRoot aggregate, Action guardar)
    {
        _rastreados.Add(aggregate);
        _altasPendientes.Add(guardar);
    }

    public async Task ConfirmarAsync(CancellationToken ct = default)
    {
        foreach (var guardar in _altasPendientes)
            guardar();
        _altasPendientes.Clear();

        var eventos = _rastreados.SelectMany(a => a.ExtraerEventos()).ToList();
        _rastreados.Clear();
        Confirmaciones++;

        if (eventos.Count > 0)
            await _publicador.PublicarAsync(eventos, ct);
    }
}
