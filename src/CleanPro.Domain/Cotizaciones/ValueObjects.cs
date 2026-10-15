using CleanPro.Domain.Comun;

namespace CleanPro.Domain.Cotizaciones;

public enum TipoDeInmueble { Residencial, Empresarial }

public enum TipoDeFrecuencia { Unica, Semanal, Quincenal, Mensual }

public enum EstadoCotizacion { Solicitada, Pendiente, Aceptada, Rechazada, Vencida }

/// <summary>Características del inmueble que el cliente reporta al pedir la cotización (US-01).</summary>
public sealed record DatosDelInmueble
{
    public string Ubicacion { get; }
    public decimal SuperficieM2 { get; }
    public TipoDeInmueble Tipo { get; }

    public DatosDelInmueble(string ubicacion, decimal superficieM2, TipoDeInmueble tipo)
    {
        if (string.IsNullOrWhiteSpace(ubicacion))
            throw new ArgumentException("La ubicación del inmueble es obligatoria.", nameof(ubicacion));
        if (superficieM2 <= 0)
            throw new ArgumentOutOfRangeException(nameof(superficieM2), "La superficie debe ser mayor que cero.");
        Ubicacion = ubicacion;
        SuperficieM2 = superficieM2;
        Tipo = tipo;
    }
}

/// <summary>Servicio único o recurrente (semanal, quincenal o mensual) — RB-03, glosario #5.</summary>
public sealed record Frecuencia(TipoDeFrecuencia Tipo)
{
    public bool EsRecurrente() => Tipo != TipoDeFrecuencia.Unica;
}

/// <summary>Extra del catálogo con el cargo vigente al momento de cotizar.</summary>
public sealed record ExtraSolicitado(string Codigo, string Nombre, Dinero Cargo);

/// <summary>
/// Duración y personal que captura el coordinador (supuesto H-04 aceptado por el equipo:
/// el BRD no da fórmula para estos valores, solo para el precio).
/// </summary>
public sealed record EstimacionOperativa
{
    public decimal DuracionHoras { get; }
    public int PersonalNecesario { get; }

    public EstimacionOperativa(decimal duracionHoras, int personalNecesario)
    {
        if (duracionHoras <= 0)
            throw new ArgumentOutOfRangeException(nameof(duracionHoras), "La duración debe ser mayor que cero.");
        if (personalNecesario <= 0)
            throw new ArgumentOutOfRangeException(nameof(personalNecesario), "Se necesita al menos una persona.");
        DuracionHoras = duracionHoras;
        PersonalNecesario = personalNecesario;
    }
}

/// <summary>
/// RB-08 / RES-05: la cotización vale 5 días hábiles a partir de su envío.
/// Supuesto: los días hábiles son de lunes a viernes (el BRD no menciona días festivos).
/// </summary>
public sealed record Vigencia
{
    public const int DiasHabiles = 5;

    public DateTime FechaEnvio { get; }
    public DateTime FechaLimite { get; }

    public Vigencia(DateTime fechaEnvio)
    {
        FechaEnvio = fechaEnvio;
        FechaLimite = SumarDiasHabiles(fechaEnvio, DiasHabiles);
    }

    public bool EstaVigente(DateTime fecha) => fecha <= FechaLimite;

    private static DateTime SumarDiasHabiles(DateTime desde, int dias)
    {
        var fecha = desde;
        var contados = 0;
        while (contados < dias)
        {
            fecha = fecha.AddDays(1);
            if (fecha.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday)
                contados++;
        }
        return fecha;
    }
}
