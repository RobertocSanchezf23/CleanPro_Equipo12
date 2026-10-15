using CleanPro.Domain.Comun;

namespace CleanPro.Domain.Cotizaciones;

/// <summary>
/// Aggregate Root. Propuesta de precio para un cliente.
/// Invariantes: solo se envía con precio calculado (RB-01, RB-03); solo se responde si está
/// Pendiente (US-02); fuera de 5 días hábiles no se acepta y queda Vencida (RB-08, RES-05).
/// </summary>
public sealed class Cotizacion : AggregateRoot
{
    private readonly List<ExtraSolicitado> _extras;

    public CotizacionId Id { get; }
    public ClienteId ClienteId { get; }
    public DatosDelInmueble Inmueble { get; }
    public Frecuencia Frecuencia { get; }
    public IReadOnlyList<ExtraSolicitado> Extras => _extras.AsReadOnly();
    public EstimacionOperativa? Estimacion { get; private set; }
    public Dinero? Precio { get; private set; }
    public Vigencia? Vigencia { get; private set; }
    public EstadoCotizacion Estado { get; private set; }

    private Cotizacion(CotizacionId id, ClienteId clienteId, DatosDelInmueble inmueble, Frecuencia frecuencia,
        IEnumerable<ExtraSolicitado> extras)
    {
        Id = id;
        ClienteId = clienteId;
        Inmueble = inmueble;
        Frecuencia = frecuencia;
        _extras = extras.ToList();
        Estado = EstadoCotizacion.Solicitada;
    }

    /// <summary>US-01: el cliente solicita una cotización.</summary>
    public static Cotizacion Solicitar(ClienteId clienteId, DatosDelInmueble inmueble, Frecuencia frecuencia,
        IEnumerable<ExtraSolicitado> extras)
    {
        if (clienteId == default)
            throw new ArgumentException("La cotización necesita un cliente.", nameof(clienteId));
        return new Cotizacion(CotizacionId.Nuevo(), clienteId, inmueble, frecuencia, extras);
    }

    /// <summary>US-06: el coordinador estima; el precio lo calcula la política de RB-03.</summary>
    public void Estimar(EstimacionOperativa estimacion, CalculadoraDePrecio calculadora)
    {
        if (Estado != EstadoCotizacion.Solicitada)
            throw new TransicionDeEstadoInvalida("RB-15", "Solo se estima una cotización en estado Solicitada.");

        Precio = calculadora.Calcular(Inmueble, Frecuencia, _extras);
        Estimacion = estimacion;
    }

    /// <summary>US-07: se envía al cliente y arranca la vigencia de 5 días hábiles.</summary>
    public void Enviar(DateTime fechaEnvio)
    {
        if (Estado != EstadoCotizacion.Solicitada)
            throw new TransicionDeEstadoInvalida("US-07", "Solo se envía una cotización en estado Solicitada.");
        if (Precio is null || Estimacion is null)
            throw new TransicionDeEstadoInvalida("RB-01", "No se puede enviar una cotización sin estimar.");

        Vigencia = new Vigencia(fechaEnvio);
        Estado = EstadoCotizacion.Pendiente;
        Registrar(new CotizacionEnviada(Id, ClienteId, Precio.Value, Vigencia.FechaLimite));
    }

    /// <summary>
    /// US-02 / US-22: si la respuesta llega fuera de vigencia no se acepta: la cotización
    /// queda Vencida y se devuelve ese estado (no es un error del sistema, es una regla).
    /// </summary>
    public EstadoCotizacion Aceptar(DateTime fechaRespuesta)
    {
        ValidarQuePuedeResponder();

        if (!Vigencia!.EstaVigente(fechaRespuesta))
        {
            MarcarVencida(fechaRespuesta);
            return Estado;
        }

        Estado = EstadoCotizacion.Aceptada;
        Registrar(new CotizacionAceptada(Id));
        return Estado;
    }

    public void Rechazar()
    {
        ValidarQuePuedeResponder();
        Estado = EstadoCotizacion.Rechazada;
        Registrar(new CotizacionRechazada(Id));
    }

    /// <summary>US-22 / RB-08: marca como vencida una cotización Pendiente cuya vigencia terminó.</summary>
    public void MarcarVencida(DateTime hoy)
    {
        if (Estado != EstadoCotizacion.Pendiente)
            throw new TransicionDeEstadoInvalida("RB-08", "Solo vence una cotización Pendiente.");
        if (Vigencia!.EstaVigente(hoy))
            throw new TransicionDeEstadoInvalida("RB-08", "La cotización todavía está vigente.");

        Estado = EstadoCotizacion.Vencida;
        Registrar(new CotizacionVencida(Id));
    }

    public bool EstaAceptada() => Estado == EstadoCotizacion.Aceptada;

    private void ValidarQuePuedeResponder()
    {
        if (Estado == EstadoCotizacion.Vencida)
            throw new CotizacionYaVencida();
        if (Estado != EstadoCotizacion.Pendiente)
            throw new TransicionDeEstadoInvalida("US-02", "Solo se responde una cotización Pendiente.");
    }
}
