using CleanPro.Domain.Comun;
using CleanPro.Domain.Cotizaciones;

namespace CleanPro.Domain.Servicios;

/// <summary>
/// Aggregate Root. La limpieza programada y ejecutada; contiene sus incidencias (ADR-003).
/// Todos los métodos validan ANTES de modificar el estado: si una regla falla, el objeto no cambia.
/// </summary>
public sealed class Servicio : AggregateRoot
{
    /// <summary>RB-11 / RES-06: minutos que se espera al cliente antes de considerar el servicio no realizado.</summary>
    public const int MinutosDeEsperaPorAcceso = 15;

    private readonly List<Incidencia> _incidencias = new();

    public ServicioId Id { get; }
    public CotizacionId CotizacionId { get; }
    public Programacion Programacion { get; }
    public CuadrillaId CuadrillaId { get; }
    public SupervisorId SupervisorId { get; }
    public Dinero PrecioAcordado { get; }
    public EstadoServicio Estado { get; private set; }
    public IReadOnlyList<Incidencia> Incidencias => _incidencias.AsReadOnly();

    private Servicio(ServicioId id, CotizacionId cotizacionId, Programacion programacion, CuadrillaId cuadrillaId,
        SupervisorId supervisorId, Dinero precioAcordado)
    {
        Id = id;
        CotizacionId = cotizacionId;
        Programacion = programacion;
        CuadrillaId = cuadrillaId;
        SupervisorId = supervisorId;
        PrecioAcordado = precioAcordado;
        Estado = EstadoServicio.Programado;
    }

    /// <summary>
    /// CU-03. Solo se crea desde una cotización Aceptada (RB-02, RES-02), con cuadrilla disponible
    /// (RES-01) y con cuadrilla y supervisor asignados (RB-04, SUP-01, SUP-02).
    /// </summary>
    public static async Task<Servicio> ProgramarAsync(Cotizacion cotizacion, Programacion programacion,
        CuadrillaId cuadrillaId, SupervisorId supervisorId, DisponibilidadDeCuadrilla disponibilidad,
        CancellationToken ct = default)
    {
        if (!cotizacion.EstaAceptada())
            throw new CotizacionNoAceptada();
        if (cuadrillaId == default)
            throw new TransicionDeEstadoInvalida("RB-04", "Todo servicio programado debe tener una cuadrilla asignada.");
        if (supervisorId == default)
            throw new TransicionDeEstadoInvalida("SUP-02", "Todo servicio debe tener un supervisor responsable.");
        if (!await disponibilidad.EstaDisponibleAsync(cuadrillaId, programacion.FechaHora, ct))
            throw new CuadrillaNoDisponible();

        var servicio = new Servicio(ServicioId.Nuevo(), cotizacion.Id, programacion, cuadrillaId, supervisorId,
            cotizacion.Precio!.Value);
        servicio.Registrar(new ServicioProgramado(servicio.Id, cotizacion.ClienteId, programacion.FechaHora, cuadrillaId));
        return servicio;
    }

    /// <summary>US-13 / RB-05.</summary>
    public void RegistrarInicio(DateTime fechaHora)
    {
        if (Estado != EstadoServicio.Programado)
            throw new TransicionDeEstadoInvalida("RB-05", "Solo se inicia un servicio Programado.");
        Estado = EstadoServicio.EnEjecucion;
    }

    /// <summary>
    /// CU-04 (US-15). La incidencia queda asociada al servicio (SUP-03) y se notifica a
    /// administración mediante el evento IncidenciaRegistrada (RB-06).
    /// Un servicio adicional en sitio o un material especial generan un cargo que nace
    /// pendiente de autorización (RB-12, RB-14).
    /// </summary>
    public IncidenciaId RegistrarIncidencia(TipoDeIncidencia tipo, string descripcion, Dinero? cargo,
        SupervisorId supervisorId, DateTime registradaEn)
    {
        if (Estado != EstadoServicio.EnEjecucion)
            throw new TransicionDeEstadoInvalida("RB-06", "Las incidencias se registran durante la ejecución del servicio.");
        if (supervisorId != SupervisorId)
            throw new TransicionDeEstadoInvalida("SUP-02", "Solo el supervisor asignado registra incidencias del servicio.");
        if (tipo == TipoDeIncidencia.ClienteNoPermiteAcceso)
            throw new TransicionDeEstadoInvalida("RB-11", "El acceso denegado se registra con RegistrarAccesoDenegado.");
        var requiereCargo = tipo is TipoDeIncidencia.ServicioAdicionalEnSitio or TipoDeIncidencia.MaterialEspecial;
        if (requiereCargo && cargo is null)
            throw new TransicionDeEstadoInvalida("RB-12", "Un servicio adicional o material especial debe cotizarse.");

        var incidencia = new Incidencia(IncidenciaId.Nuevo(), tipo, descripcion, registradaEn, supervisorId,
            cargo is null ? null : CargoAdicional.Pendiente(cargo.Value));
        _incidencias.Add(incidencia);
        Registrar(new IncidenciaRegistrada(Id, incidencia.Id, tipo, descripcion));
        return incidencia.Id;
    }

    /// <summary>
    /// CU-04 alterno B (US-16). Tras esperar al menos 15 minutos sin acceso, el servicio
    /// queda No realizado; la factura llevará el cargo de cancelación del 50% (RB-11).
    /// </summary>
    public void RegistrarAccesoDenegado(int minutosEspera, SupervisorId supervisorId, DateTime registradaEn)
    {
        if (Estado != EstadoServicio.Programado)
            throw new TransicionDeEstadoInvalida("RB-11", "El acceso denegado se registra al llegar, antes de iniciar el servicio.");
        if (supervisorId != SupervisorId)
            throw new TransicionDeEstadoInvalida("SUP-02", "Solo el supervisor asignado registra el acceso denegado.");
        if (minutosEspera < MinutosDeEsperaPorAcceso)
            throw new TiempoDeEsperaInsuficiente(minutosEspera, MinutosDeEsperaPorAcceso);

        var incidencia = new Incidencia(IncidenciaId.Nuevo(), TipoDeIncidencia.ClienteNoPermiteAcceso,
            $"El cliente no permitió el acceso después de {minutosEspera} minutos de espera.", registradaEn,
            supervisorId, null);
        _incidencias.Add(incidencia);
        Estado = EstadoServicio.NoRealizado;
        Registrar(new IncidenciaRegistrada(Id, incidencia.Id, incidencia.Tipo, incidencia.Descripcion));
        Registrar(new ServicioNoRealizado(Id));
    }

    /// <summary>CU-04 alterno A (US-11). Solo el coordinador autoriza (RB-12, RB-14, RB-15).</summary>
    public void AutorizarCargo(IncidenciaId incidenciaId, CoordinadorId coordinadorId)
    {
        var incidencia = _incidencias.SingleOrDefault(i => i.Id == incidenciaId)
            ?? throw new TransicionDeEstadoInvalida("SUP-03", "La incidencia no pertenece a este servicio.");
        incidencia.Autorizar(coordinadorId);
        Registrar(new CargoAdicionalAutorizado(Id, incidenciaId, incidencia.Cargo!.Monto));
    }

    /// <summary>US-14 / RES-03: las incidencias ya quedaron documentadas durante la ejecución.</summary>
    public void RegistrarTerminacion(DateTime fechaHora)
    {
        if (Estado != EstadoServicio.EnEjecucion)
            throw new TransicionDeEstadoInvalida("RB-05", "Solo se termina un servicio En ejecución.");
        Estado = EstadoServicio.Terminado;
        Registrar(new ServicioTerminado(Id));
    }

    /// <summary>RB-07 (con el ajuste H-01) y RB-14.</summary>
    public void VerificarQueEsFacturable()
    {
        if (Estado is not (EstadoServicio.Terminado or EstadoServicio.NoRealizado))
            throw new ServicioNoFacturable(Estado.ToString());
        if (_incidencias.Any(i => i.TieneCargoPendiente()))
            throw new CargosPendientesDeAutorizacion();
    }

    public IReadOnlyList<CargoAdicional> CargosAutorizados() =>
        _incidencias
            .Where(i => i.Cargo is { EstaPendiente: false })
            .Select(i => i.Cargo!)
            .ToList();
}
