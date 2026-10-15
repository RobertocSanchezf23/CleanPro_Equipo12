using CleanPro.Application.Comun;
using CleanPro.Domain.Comun;
using CleanPro.Domain.Cotizaciones;

namespace CleanPro.Application.Cotizaciones;

/// <summary>
/// Application Service del coordinador y del cliente para CU-01 y CU-02.
/// Solo coordina: obtiene objetos, llama al dominio y confirma. Las reglas viven en Cotizacion
/// y CalculadoraDePrecio. La autorización por rol (RB-15) se resuelve en la entrada de la aplicación.
/// </summary>
public sealed class CotizacionAppService
{
    private readonly ICotizacionRepository _cotizaciones;
    private readonly ICatalogoDeExtras _catalogo;
    private readonly CalculadoraDePrecio _calculadora;
    private readonly IUnitOfWork _uow;

    public CotizacionAppService(ICotizacionRepository cotizaciones, ICatalogoDeExtras catalogo,
        CalculadoraDePrecio calculadora, IUnitOfWork uow)
    {
        _cotizaciones = cotizaciones;
        _catalogo = catalogo;
        _calculadora = calculadora;
        _uow = uow;
    }

    /// <summary>CU-01 (US-01).</summary>
    public async Task<CotizacionId> SolicitarCotizacion(SolicitarCotizacionCommand cmd, CancellationToken ct = default)
    {
        var extras = new List<ExtraSolicitado>();
        foreach (var codigo in cmd.CodigosExtras)
        {
            var extra = await _catalogo.ObtenerPorCodigoAsync(codigo, ct) ?? throw new ExtraNoExisteEnCatalogo(codigo);
            extras.Add(new ExtraSolicitado(extra.Codigo, extra.Nombre, extra.PrecioReferencia));
        }

        var cotizacion = Cotizacion.Solicitar(
            cmd.ClienteId,
            new DatosDelInmueble(cmd.Ubicacion, cmd.SuperficieM2, cmd.TipoInmueble),
            new Frecuencia(cmd.Frecuencia),
            extras);

        await _cotizaciones.AgregarAsync(cotizacion, ct);
        await _uow.ConfirmarAsync(ct);
        return cotizacion.Id;
    }

    /// <summary>CU-01 (US-06, US-07).</summary>
    public async Task<CotizacionEnviadaDto> EstimarYEnviarCotizacion(EstimarYEnviarCotizacionCommand cmd,
        CancellationToken ct = default)
    {
        var cotizacion = await ObtenerAsync(cmd.CotizacionId, ct);

        cotizacion.Estimar(new EstimacionOperativa(cmd.DuracionHoras, cmd.PersonalNecesario), _calculadora);
        cotizacion.Enviar(cmd.FechaEnvio);

        await _uow.ConfirmarAsync(ct);
        return new CotizacionEnviadaDto(cotizacion.Id.Valor, cotizacion.Precio!.Value.Monto, cotizacion.Vigencia!.FechaLimite);
    }

    /// <summary>CU-02 (US-02, US-22). Devuelve el estado resultante: Aceptada, Rechazada o Vencida.</summary>
    public async Task<EstadoCotizacion> ResponderCotizacion(ResponderCotizacionCommand cmd, CancellationToken ct = default)
    {
        var cotizacion = await ObtenerAsync(cmd.CotizacionId, ct);

        if (cmd.Acepta)
            cotizacion.Aceptar(cmd.FechaRespuesta);
        else
            cotizacion.Rechazar();

        await _uow.ConfirmarAsync(ct);
        return cotizacion.Estado;
    }

    private async Task<Cotizacion> ObtenerAsync(CotizacionId id, CancellationToken ct) =>
        await _cotizaciones.ObtenerPorIdAsync(id, ct) ?? throw new RecursoNoEncontrado("la cotización", id.Valor);
}
