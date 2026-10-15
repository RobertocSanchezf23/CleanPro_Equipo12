using CleanPro.Domain.Comun;

namespace CleanPro.Domain.Cotizaciones;

/// <summary>
/// Tarifa por m² según el tipo de inmueble. Se inyecta para que un cambio de tarifa
/// o un tipo nuevo no obligue a editar el cálculo (hallazgo SOLID S-2).
/// </summary>
public sealed class TarifasPorTipoDeInmueble
{
    private readonly IReadOnlyDictionary<TipoDeInmueble, Dinero> _tarifaPorM2;

    public TarifasPorTipoDeInmueble(IReadOnlyDictionary<TipoDeInmueble, Dinero> tarifaPorM2)
    {
        _tarifaPorM2 = tarifaPorM2;
    }

    /// <summary>RB-03: $12 MXN/m² residencial y $15 MXN/m² empresarial.</summary>
    public static TarifasPorTipoDeInmueble SegunBrd() => new(new Dictionary<TipoDeInmueble, Dinero>
    {
        [TipoDeInmueble.Residencial] = new Dinero(12m),
        [TipoDeInmueble.Empresarial] = new Dinero(15m),
    });

    public Dinero TarifaPorM2(TipoDeInmueble tipo) =>
        _tarifaPorM2.TryGetValue(tipo, out var tarifa)
            ? tarifa
            : throw new InvalidOperationException($"No hay tarifa configurada para el tipo de inmueble {tipo}.");
}

/// <summary>RB-03: los servicios recurrentes reciben un descuento sobre el precio base.</summary>
public sealed class PoliticaDeDescuento
{
    public decimal PorcentajeRecurrente { get; }

    public PoliticaDeDescuento(decimal porcentajeRecurrente)
    {
        if (porcentajeRecurrente is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(porcentajeRecurrente));
        PorcentajeRecurrente = porcentajeRecurrente;
    }

    public static PoliticaDeDescuento SegunBrd() => new(10m);

    public Dinero Aplicar(Dinero precioBase, Frecuencia frecuencia) =>
        frecuencia.EsRecurrente()
            ? precioBase.Restar(precioBase.Porcentaje(PorcentajeRecurrente))
            : precioBase;
}

/// <summary>
/// Domain Service que aplica RB-03. Es un servicio y no un método de Cotizacion porque
/// el mismo cálculo se reutiliza al recalcular por inmueble mayor (RB-13, trabajo posterior).
/// </summary>
public sealed class CalculadoraDePrecio
{
    private readonly TarifasPorTipoDeInmueble _tarifas;
    private readonly PoliticaDeDescuento _descuento;

    public CalculadoraDePrecio(TarifasPorTipoDeInmueble tarifas, PoliticaDeDescuento descuento)
    {
        _tarifas = tarifas;
        _descuento = descuento;
    }

    public Dinero Calcular(DatosDelInmueble inmueble, Frecuencia frecuencia, IEnumerable<ExtraSolicitado> extras)
    {
        var tarifa = _tarifas.TarifaPorM2(inmueble.Tipo);
        var precioBase = new Dinero(tarifa.Monto * inmueble.SuperficieM2);

        // Supuesto H-05 aceptado por el equipo: el descuento aplica solo a superficie × tarifa.
        var precioConDescuento = _descuento.Aplicar(precioBase, frecuencia);

        return extras.Aggregate(precioConDescuento, (total, extra) => total.Sumar(extra.Cargo));
    }
}
