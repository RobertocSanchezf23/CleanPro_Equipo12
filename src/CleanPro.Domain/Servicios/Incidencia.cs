using CleanPro.Domain.Comun;

namespace CleanPro.Domain.Servicios;

public enum TipoDeIncidencia
{
    ServicioAdicionalEnSitio,
    MaterialEspecial,
    InmuebleMayorALoReportado,
    ClienteNoPermiteAcceso,
    EmpleadoAusente,
    OtroCambio,
}

public enum EstadoDeAutorizacion { PendienteDeAutorizacion, Autorizado }

/// <summary>
/// Value Object: cargo de un servicio adicional en sitio o de material especial.
/// Nace pendiente y solo el coordinador lo autoriza (RB-12, RB-14, RB-15).
/// </summary>
public sealed record CargoAdicional
{
    public Dinero Monto { get; }
    public EstadoDeAutorizacion Estado { get; }
    public CoordinadorId? AutorizadoPor { get; }

    private CargoAdicional(Dinero monto, EstadoDeAutorizacion estado, CoordinadorId? autorizadoPor)
    {
        Monto = monto;
        Estado = estado;
        AutorizadoPor = autorizadoPor;
    }

    public static CargoAdicional Pendiente(Dinero monto) =>
        new(monto, EstadoDeAutorizacion.PendienteDeAutorizacion, null);

    public bool EstaPendiente => Estado == EstadoDeAutorizacion.PendienteDeAutorizacion;

    public CargoAdicional Autorizar(CoordinadorId coordinadorId)
    {
        if (!EstaPendiente)
            throw new TransicionDeEstadoInvalida("RB-12", "El cargo adicional ya estaba autorizado.");
        return new CargoAdicional(Monto, EstadoDeAutorizacion.Autorizado, coordinadorId);
    }
}

/// <summary>
/// Entity dentro del Aggregate Servicio (ADR-003). Solo se crea o modifica a través de la raíz.
/// </summary>
public sealed class Incidencia
{
    public IncidenciaId Id { get; }
    public TipoDeIncidencia Tipo { get; }
    public string Descripcion { get; }
    public DateTime RegistradaEn { get; }
    public SupervisorId RegistradaPor { get; }
    public CargoAdicional? Cargo { get; private set; }

    internal Incidencia(IncidenciaId id, TipoDeIncidencia tipo, string descripcion, DateTime registradaEn,
        SupervisorId registradaPor, CargoAdicional? cargo)
    {
        if (string.IsNullOrWhiteSpace(descripcion))
            throw new ArgumentException("La incidencia necesita una descripción.", nameof(descripcion));
        Id = id;
        Tipo = tipo;
        Descripcion = descripcion;
        RegistradaEn = registradaEn;
        RegistradaPor = registradaPor;
        Cargo = cargo;
    }

    public bool TieneCargoPendiente() => Cargo is { EstaPendiente: true };

    internal void Autorizar(CoordinadorId coordinadorId)
    {
        if (Cargo is null)
            throw new TransicionDeEstadoInvalida("RB-12", "La incidencia no tiene un cargo adicional que autorizar.");
        Cargo = Cargo.Autorizar(coordinadorId);
    }
}
