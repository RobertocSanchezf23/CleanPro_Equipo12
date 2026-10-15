using CleanPro.Domain.Comun;

namespace CleanPro.Application.Comun;

/// <summary>
/// Límite transaccional de cada caso de uso (ADR-002). Al confirmar se guardan los cambios de
/// UN Aggregate y después se publican sus Domain Events. Si algo falla antes, no se guarda nada.
/// </summary>
public interface IUnitOfWork
{
    Task ConfirmarAsync(CancellationToken ct = default);
}

/// <summary>Publica los Domain Events después de confirmar (ej. avisar a administración, RB-06).</summary>
public interface IPublicadorDeEventos
{
    Task PublicarAsync(IReadOnlyList<EventoDeDominio> eventos, CancellationToken ct = default);
}

/// <summary>El objeto solicitado no existe (error de aplicación, no una regla del negocio).</summary>
public sealed class RecursoNoEncontrado : Exception
{
    public RecursoNoEncontrado(string recurso, object id) : base($"No existe {recurso} con Id {id}.") { }
}
