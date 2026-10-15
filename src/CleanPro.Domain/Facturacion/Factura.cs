using CleanPro.Domain.Comun;

namespace CleanPro.Domain.Facturacion;

public enum EstadoDePago { Pendiente, Pagada }

public enum OrigenDelConcepto { Servicio, Incidencia, Cancelacion }

/// <summary>Cada renglón de la factura (glosario actualizado: "Concepto facturado").</summary>
public sealed record ConceptoFacturado(string Descripcion, Dinero Monto, OrigenDelConcepto Origen);

/// <summary>
/// Aggregate Root. Invariantes: total = suma de conceptos (RES-04); se emite con estado de pago
/// Pendiente. "Una factura por servicio" (SUP-04) la valida GeneradorDeFactura (SOLID S-6).
/// </summary>
public sealed class Factura : AggregateRoot
{
    private readonly List<ConceptoFacturado> _conceptos;

    public FacturaId Id { get; }
    public ServicioId ServicioId { get; }
    public IReadOnlyList<ConceptoFacturado> Conceptos => _conceptos.AsReadOnly();
    public Dinero Total { get; }
    public DateTime FechaEmision { get; }
    public EstadoDePago EstadoDePago { get; private set; }

    private Factura(FacturaId id, ServicioId servicioId, List<ConceptoFacturado> conceptos, DateTime fechaEmision)
    {
        Id = id;
        ServicioId = servicioId;
        _conceptos = conceptos;
        Total = conceptos.Aggregate(Dinero.Cero, (total, concepto) => total.Sumar(concepto.Monto));
        FechaEmision = fechaEmision;
        EstadoDePago = EstadoDePago.Pendiente;
    }

    public static Factura Emitir(ServicioId servicioId, IEnumerable<ConceptoFacturado> conceptos, DateTime fechaEmision)
    {
        var lista = conceptos.ToList();
        if (lista.Count == 0)
            throw new ArgumentException("Una factura necesita al menos un concepto.", nameof(conceptos));

        var factura = new Factura(FacturaId.Nuevo(), servicioId, lista, fechaEmision);
        factura.Registrar(new FacturaEmitida(factura.Id, servicioId, factura.Total));
        return factura;
    }

    public void MarcarPagada()
    {
        if (EstadoDePago == EstadoDePago.Pagada)
            throw new TransicionDeEstadoInvalida("BN-07", "La factura ya estaba pagada.");
        EstadoDePago = EstadoDePago.Pagada;
    }
}
