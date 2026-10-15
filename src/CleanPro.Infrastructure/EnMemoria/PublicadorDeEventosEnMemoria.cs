using CleanPro.Application.Comun;
using CleanPro.Domain.Comun;
using CleanPro.Domain.Servicios;

namespace CleanPro.Infrastructure.EnMemoria;

public interface IManejadorDeEvento
{
    bool Maneja(EventoDeDominio evento);
    Task ManejarAsync(EventoDeDominio evento, CancellationToken ct = default);
}

/// <summary>Publicador en memoria. En la versión real: tabla Outbox + envío asíncrono.</summary>
public sealed class PublicadorDeEventosEnMemoria : IPublicadorDeEventos
{
    private readonly List<IManejadorDeEvento> _manejadores = new();
    private readonly List<EventoDeDominio> _publicados = new();

    public IReadOnlyList<EventoDeDominio> Publicados => _publicados.AsReadOnly();

    public void Suscribir(IManejadorDeEvento manejador) => _manejadores.Add(manejador);

    public async Task PublicarAsync(IReadOnlyList<EventoDeDominio> eventos, CancellationToken ct = default)
    {
        foreach (var evento in eventos)
        {
            _publicados.Add(evento);
            foreach (var manejador in _manejadores.Where(m => m.Maneja(evento)))
                await manejador.ManejarAsync(evento, ct);
        }
    }
}

/// <summary>Simula el canal de avisos (correo, SMS, etc.).</summary>
public sealed class BuzonDeNotificaciones
{
    private readonly List<string> _mensajes = new();
    public IReadOnlyList<string> Mensajes => _mensajes.AsReadOnly();
    public void Enviar(string mensaje) => _mensajes.Add(mensaje);
}

/// <summary>RB-06 / US-19 / BN-06: toda incidencia registrada llega a administración (resuelve PA-01).</summary>
public sealed class NotificarAdministracionAlRegistrarIncidencia : IManejadorDeEvento
{
    private readonly BuzonDeNotificaciones _buzon;

    public NotificarAdministracionAlRegistrarIncidencia(BuzonDeNotificaciones buzon) => _buzon = buzon;

    public bool Maneja(EventoDeDominio evento) => evento is IncidenciaRegistrada;

    public Task ManejarAsync(EventoDeDominio evento, CancellationToken ct = default)
    {
        var e = (IncidenciaRegistrada)evento;
        _buzon.Enviar($"[Administración] Incidencia {e.Tipo} en el servicio {e.ServicioId.Valor}: {e.Descripcion}");
        return Task.CompletedTask;
    }
}

/// <summary>US-03: el cliente recibe la fecha, hora y cuadrilla de su servicio.</summary>
public sealed class NotificarClienteAlProgramarServicio : IManejadorDeEvento
{
    private readonly BuzonDeNotificaciones _buzon;

    public NotificarClienteAlProgramarServicio(BuzonDeNotificaciones buzon) => _buzon = buzon;

    public bool Maneja(EventoDeDominio evento) => evento is ServicioProgramado;

    public Task ManejarAsync(EventoDeDominio evento, CancellationToken ct = default)
    {
        var e = (ServicioProgramado)evento;
        _buzon.Enviar($"[Cliente {e.ClienteId.Valor}] Su servicio quedó programado para {e.FechaHora:dd/MM/yyyy HH:mm}.");
        return Task.CompletedTask;
    }
}
