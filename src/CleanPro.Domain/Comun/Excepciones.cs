namespace CleanPro.Domain.Comun;

/// <summary>
/// Error del negocio. Siempre indica la regla del BRD que se protege,
/// para que el error pueda explicarse en términos del negocio (revisión obligatoria, etapa 4).
/// </summary>
public class ExcepcionDeDominio : Exception
{
    public string Regla { get; }

    public ExcepcionDeDominio(string regla, string mensaje) : base($"[{regla}] {mensaje}")
    {
        Regla = regla;
    }
}

/// <summary>Se intentó una operación que el estado actual del objeto no permite.</summary>
public sealed class TransicionDeEstadoInvalida : ExcepcionDeDominio
{
    public TransicionDeEstadoInvalida(string regla, string mensaje) : base(regla, mensaje) { }
}

public sealed class CotizacionNoAceptada : ExcepcionDeDominio
{
    public CotizacionNoAceptada()
        : base("RB-02", "El servicio solo se programa si el cliente aceptó la cotización.") { }
}

public sealed class CotizacionYaVencida : ExcepcionDeDominio
{
    public CotizacionYaVencida()
        : base("RB-08", "La cotización está vencida; el coordinador debe generar una nueva.") { }
}

public sealed class ExtraNoExisteEnCatalogo : ExcepcionDeDominio
{
    public ExtraNoExisteEnCatalogo(string codigo)
        : base("SUP-06", $"El extra '{codigo}' no existe en el catálogo de extras.") { }
}

public sealed class CuadrillaNoDisponible : ExcepcionDeDominio
{
    public CuadrillaNoDisponible()
        : base("RES-01", "La cuadrilla no está disponible para la fecha y hora solicitadas.") { }
}

public sealed class TiempoDeEsperaInsuficiente : ExcepcionDeDominio
{
    public TiempoDeEsperaInsuficiente(int minutosEsperados, int minimo)
        : base("RES-06", $"Se esperaron {minutosEsperados} min; se deben esperar al menos {minimo} min antes de considerar el servicio no realizado.") { }
}

public sealed class ServicioNoFacturable : ExcepcionDeDominio
{
    public ServicioNoFacturable(string estado)
        : base("RB-07", $"El servicio está en estado {estado}; solo se factura un servicio Terminado o No realizado.") { }
}

public sealed class CargosPendientesDeAutorizacion : ExcepcionDeDominio
{
    public CargosPendientesDeAutorizacion()
        : base("RB-14", "Hay cargos adicionales sin autorizar por el coordinador; no se puede facturar.") { }
}

public sealed class FacturaYaEmitida : ExcepcionDeDominio
{
    public FacturaYaEmitida()
        : base("SUP-04", "El servicio ya tiene una factura; solo existe una factura por servicio.") { }
}
