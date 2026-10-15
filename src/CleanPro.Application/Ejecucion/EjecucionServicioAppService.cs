using CleanPro.Application.Comun;
using CleanPro.Domain.Comun;
using CleanPro.Domain.Cotizaciones;
using CleanPro.Domain.Servicios;

namespace CleanPro.Application.Ejecucion;

/// <summary>
/// Application Service de la ejecución en sitio (supervisor) para CU-04, más la autorización
/// del coordinador (US-11) y los métodos de soporte de inicio y terminación (US-13, US-14).
/// </summary>
public sealed class EjecucionServicioAppService
{
    private readonly IServicioRepository _servicios;
    private readonly ICatalogoDeExtras _catalogo;
    private readonly IUnitOfWork _uow;

    public EjecucionServicioAppService(IServicioRepository servicios, ICatalogoDeExtras catalogo, IUnitOfWork uow)
    {
        _servicios = servicios;
        _catalogo = catalogo;
        _uow = uow;
    }

    /// <summary>CU-04 (US-15, US-19). El aviso a administración sale del evento IncidenciaRegistrada.</summary>
    public async Task<IncidenciaId> RegistrarIncidencia(RegistrarIncidenciaCommand cmd, CancellationToken ct = default)
    {
        var servicio = await ObtenerAsync(cmd.ServicioId, ct);

        // Coordinación: obtener el precio del catálogo no es una regla, es conseguir un dato (SUP-06).
        Dinero? cargo = null;
        if (cmd.CodigoExtra is not null)
        {
            var extra = await _catalogo.ObtenerPorCodigoAsync(cmd.CodigoExtra, ct)
                ?? throw new ExtraNoExisteEnCatalogo(cmd.CodigoExtra);
            cargo = extra.PrecioReferencia;
        }
        else if (cmd.MontoCargo is decimal monto)
        {
            cargo = new Dinero(monto);
        }

        var incidenciaId = servicio.RegistrarIncidencia(cmd.Tipo, cmd.Descripcion, cargo, cmd.SupervisorId, cmd.RegistradaEn);
        await _uow.ConfirmarAsync(ct);
        return incidenciaId;
    }

    /// <summary>CU-04 alterno B (US-16, RB-11).</summary>
    public async Task<EstadoServicio> RegistrarAccesoDenegado(RegistrarAccesoDenegadoCommand cmd, CancellationToken ct = default)
    {
        var servicio = await ObtenerAsync(cmd.ServicioId, ct);
        servicio.RegistrarAccesoDenegado(cmd.MinutosEspera, cmd.SupervisorId, cmd.RegistradaEn);
        await _uow.ConfirmarAsync(ct);
        return servicio.Estado;
    }

    /// <summary>CU-04 alterno A (US-11, RB-12, RB-14).</summary>
    public async Task AutorizarCargoAdicional(AutorizarCargoAdicionalCommand cmd, CancellationToken ct = default)
    {
        var servicio = await ObtenerAsync(cmd.ServicioId, ct);
        servicio.AutorizarCargo(cmd.IncidenciaId, cmd.CoordinadorId);
        await _uow.ConfirmarAsync(ct);
    }

    /// <summary>Soporte (US-13).</summary>
    public async Task RegistrarInicio(RegistrarInicioCommand cmd, CancellationToken ct = default)
    {
        var servicio = await ObtenerAsync(cmd.ServicioId, ct);
        servicio.RegistrarInicio(cmd.FechaHora);
        await _uow.ConfirmarAsync(ct);
    }

    /// <summary>Soporte (US-14).</summary>
    public async Task RegistrarTerminacion(RegistrarTerminacionCommand cmd, CancellationToken ct = default)
    {
        var servicio = await ObtenerAsync(cmd.ServicioId, ct);
        servicio.RegistrarTerminacion(cmd.FechaHora);
        await _uow.ConfirmarAsync(ct);
    }

    private async Task<Servicio> ObtenerAsync(ServicioId id, CancellationToken ct) =>
        await _servicios.ObtenerPorIdAsync(id, ct) ?? throw new RecursoNoEncontrado("el servicio", id.Valor);
}
