using CleanPro.Domain.Comun;
using CleanPro.Domain.Cotizaciones;

namespace CleanPro.Application.Cotizaciones;

// Commands (lado de escritura de CQRS). Son datos de entrada: no contienen reglas.

public sealed record SolicitarCotizacionCommand(
    ClienteId ClienteId,
    string Ubicacion,
    decimal SuperficieM2,
    TipoDeInmueble TipoInmueble,
    TipoDeFrecuencia Frecuencia,
    IReadOnlyList<string> CodigosExtras);

public sealed record EstimarYEnviarCotizacionCommand(
    CotizacionId CotizacionId,
    CoordinadorId CoordinadorId,
    decimal DuracionHoras,
    int PersonalNecesario,
    DateTime FechaEnvio);

public sealed record ResponderCotizacionCommand(
    CotizacionId CotizacionId,
    bool Acepta,
    DateTime FechaRespuesta);

/// <summary>Resultado de lectura simple que se devuelve al coordinador al enviar.</summary>
public sealed record CotizacionEnviadaDto(Guid CotizacionId, decimal Precio, DateTime VigenteHasta);
