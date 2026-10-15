namespace CleanPro.Domain.Servicios;

public enum EstadoServicio { Programado, EnEjecucion, Terminado, NoRealizado }

/// <summary>Fecha y hora en que se ejecutará el servicio (glosario #13).</summary>
public sealed record Programacion(DateTime FechaHora);
