using CleanPro.Application.Comun;
using CleanPro.Domain.Comun;
using CleanPro.Domain.Cotizaciones;
using CleanPro.Domain.Servicios;

namespace CleanPro.Application.ProgramacionDeServicios;

public sealed record ProgramarServicioCommand(
    CotizacionId CotizacionId,
    DateTime FechaHora,
    CuadrillaId CuadrillaId,
    SupervisorId SupervisorId);

/// <summary>Application Service del coordinador para CU-03 (US-08, US-09, US-03).</summary>
public sealed class ProgramacionAppService
{
    private readonly ICotizacionRepository _cotizaciones;
    private readonly IServicioRepository _servicios;
    private readonly DisponibilidadDeCuadrilla _disponibilidad;
    private readonly IUnitOfWork _uow;

    public ProgramacionAppService(ICotizacionRepository cotizaciones, IServicioRepository servicios,
        DisponibilidadDeCuadrilla disponibilidad, IUnitOfWork uow)
    {
        _cotizaciones = cotizaciones;
        _servicios = servicios;
        _disponibilidad = disponibilidad;
        _uow = uow;
    }

    /// <summary>
    /// Sigue el diagrama de secuencia 1: obtiene la cotización, el dominio crea el Servicio
    /// (valida RB-02, RB-04, RES-01), se agrega y se confirma. La cotización solo se lee.
    /// </summary>
    public async Task<ServicioId> ProgramarServicio(ProgramarServicioCommand cmd, CancellationToken ct = default)
    {
        var cotizacion = await _cotizaciones.ObtenerPorIdAsync(cmd.CotizacionId, ct)
            ?? throw new RecursoNoEncontrado("la cotización", cmd.CotizacionId.Valor);

        var servicio = await Servicio.ProgramarAsync(cotizacion, new Programacion(cmd.FechaHora),
            cmd.CuadrillaId, cmd.SupervisorId, _disponibilidad, ct);

        await _servicios.AgregarAsync(servicio, ct);
        await _uow.ConfirmarAsync(ct);
        return servicio.Id;
    }
}
