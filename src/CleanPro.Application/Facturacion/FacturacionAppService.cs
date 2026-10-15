using CleanPro.Application.Comun;
using CleanPro.Domain.Comun;
using CleanPro.Domain.Facturacion;
using CleanPro.Domain.Servicios;

namespace CleanPro.Application.Facturacion;

public sealed record FacturarServicioCommand(ServicioId ServicioId, DateTime FechaEmision);

/// <summary>
/// Application Service de administración para CU-05 (US-20, US-05).
/// No decide el monto ni si se puede facturar: eso lo hacen GeneradorDeFactura y Servicio
/// (diagrama de secuencia 2). Si el dominio rechaza, no se llama a ConfirmarAsync.
/// </summary>
public sealed class FacturacionAppService
{
    private readonly IServicioRepository _servicios;
    private readonly GeneradorDeFactura _generador;
    private readonly IFacturaRepository _facturas;
    private readonly IUnitOfWork _uow;

    public FacturacionAppService(IServicioRepository servicios, GeneradorDeFactura generador,
        IFacturaRepository facturas, IUnitOfWork uow)
    {
        _servicios = servicios;
        _generador = generador;
        _facturas = facturas;
        _uow = uow;
    }

    public async Task<FacturaId> FacturarServicio(FacturarServicioCommand cmd, CancellationToken ct = default)
    {
        var servicio = await _servicios.ObtenerPorIdAsync(cmd.ServicioId, ct)
            ?? throw new RecursoNoEncontrado("el servicio", cmd.ServicioId.Valor);

        var factura = await _generador.GenerarAsync(servicio, cmd.FechaEmision, ct);

        await _facturas.AgregarAsync(factura, ct);
        await _uow.ConfirmarAsync(ct);
        return factura.Id;
    }
}
