using CleanPro.Domain.Comun;
using CleanPro.Domain.Servicios;

namespace CleanPro.Domain.Facturacion;

/// <summary>
/// Domain Service: arma la Factura a partir de un Servicio (involucra dos Aggregates).
/// Terminado   → precio acordado + cargos autorizados (RES-04).
/// NoRealizado → cargo de cancelación del 50% (RB-11, hallazgo H-01).
/// </summary>
public sealed class GeneradorDeFactura
{
    /// <summary>RB-11: porcentaje del valor del servicio que se cobra si el cliente no permitió el acceso.</summary>
    public const decimal PorcentajeCargoCancelacion = 50m;

    private readonly IFacturaRepository _facturas;

    public GeneradorDeFactura(IFacturaRepository facturas)
    {
        _facturas = facturas;
    }

    public async Task<Factura> GenerarAsync(Servicio servicio, DateTime fechaEmision, CancellationToken ct = default)
    {
        if (await _facturas.ExisteFacturaParaServicioAsync(servicio.Id, ct))
            throw new FacturaYaEmitida();

        servicio.VerificarQueEsFacturable();

        var conceptos = new List<ConceptoFacturado>();
        if (servicio.Estado == EstadoServicio.Terminado)
        {
            conceptos.Add(new ConceptoFacturado("Servicio de limpieza", servicio.PrecioAcordado, OrigenDelConcepto.Servicio));
            conceptos.AddRange(servicio.CargosAutorizados().Select(cargo =>
                new ConceptoFacturado("Cargo adicional autorizado", cargo.Monto, OrigenDelConcepto.Incidencia)));
        }
        else
        {
            conceptos.Add(new ConceptoFacturado(
                $"Cargo de cancelación por falta de acceso ({PorcentajeCargoCancelacion}%)",
                servicio.PrecioAcordado.Porcentaje(PorcentajeCargoCancelacion),
                OrigenDelConcepto.Cancelacion));
        }

        return Factura.Emitir(servicio.Id, conceptos, fechaEmision);
    }
}
