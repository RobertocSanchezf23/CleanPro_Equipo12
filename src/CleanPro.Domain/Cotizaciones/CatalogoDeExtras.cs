using CleanPro.Domain.Comun;

namespace CleanPro.Domain.Cotizaciones;

/// <summary>Dato de referencia (SUP-06): extra predefinido con su precio. No es un Aggregate.</summary>
public sealed record ExtraDelCatalogo(string Codigo, string Nombre, Dinero PrecioReferencia);

/// <summary>Consulta de solo lectura al catálogo de extras.</summary>
public interface ICatalogoDeExtras
{
    Task<ExtraDelCatalogo?> ObtenerPorCodigoAsync(string codigo, CancellationToken ct = default);
}
