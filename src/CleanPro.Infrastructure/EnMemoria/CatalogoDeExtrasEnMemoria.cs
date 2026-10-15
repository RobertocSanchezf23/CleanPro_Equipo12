using CleanPro.Domain.Comun;
using CleanPro.Domain.Cotizaciones;

namespace CleanPro.Infrastructure.EnMemoria;

/// <summary>
/// Catálogo de extras (SUP-06). Los códigos y precios son DATOS DE EJEMPLO del caso ficticio;
/// el BRD no define los extras concretos.
/// </summary>
public sealed class CatalogoDeExtrasEnMemoria : ICatalogoDeExtras
{
    private readonly Dictionary<string, ExtraDelCatalogo> _extras = new(StringComparer.OrdinalIgnoreCase)
    {
        ["VENTANAS"] = new ExtraDelCatalogo("VENTANAS", "Limpieza de ventanas", new Dinero(250m)),
        ["REFRIGERADOR"] = new ExtraDelCatalogo("REFRIGERADOR", "Limpieza interior de refrigerador", new Dinero(180m)),
        ["ALFOMBRA"] = new ExtraDelCatalogo("ALFOMBRA", "Lavado de alfombra", new Dinero(300m)),
    };

    public Task<ExtraDelCatalogo?> ObtenerPorCodigoAsync(string codigo, CancellationToken ct = default) =>
        Task.FromResult<ExtraDelCatalogo?>(_extras.TryGetValue(codigo, out var extra) ? extra : null);
}
