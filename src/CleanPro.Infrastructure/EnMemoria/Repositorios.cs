using CleanPro.Domain.Comun;
using CleanPro.Domain.Cotizaciones;
using CleanPro.Domain.Facturacion;
using CleanPro.Domain.Servicios;

namespace CleanPro.Infrastructure.EnMemoria;

// Las tres implementaciones respetan el mismo contrato que tendría la versión EF Core (SOLID S-4):
// ObtenerPorIdAsync devuelve null si no existe y AgregarAsync no guarda hasta ConfirmarAsync.

public sealed class CotizacionRepositoryEnMemoria : ICotizacionRepository
{
    private readonly BaseDeDatosEnMemoria _db;
    private readonly UnitOfWorkEnMemoria _uow;

    public CotizacionRepositoryEnMemoria(BaseDeDatosEnMemoria db, UnitOfWorkEnMemoria uow)
    {
        _db = db;
        _uow = uow;
    }

    public Task<Cotizacion?> ObtenerPorIdAsync(CotizacionId id, CancellationToken ct = default)
    {
        if (!_db.Cotizaciones.TryGetValue(id, out var cotizacion))
            return Task.FromResult<Cotizacion?>(null);
        _uow.Rastrear(cotizacion);
        return Task.FromResult<Cotizacion?>(cotizacion);
    }

    public Task AgregarAsync(Cotizacion cotizacion, CancellationToken ct = default)
    {
        _uow.RegistrarAlta(cotizacion, () => _db.Cotizaciones[cotizacion.Id] = cotizacion);
        return Task.CompletedTask;
    }
}

public sealed class ServicioRepositoryEnMemoria : IServicioRepository
{
    private readonly BaseDeDatosEnMemoria _db;
    private readonly UnitOfWorkEnMemoria _uow;

    public ServicioRepositoryEnMemoria(BaseDeDatosEnMemoria db, UnitOfWorkEnMemoria uow)
    {
        _db = db;
        _uow = uow;
    }

    public Task<Servicio?> ObtenerPorIdAsync(ServicioId id, CancellationToken ct = default)
    {
        if (!_db.Servicios.TryGetValue(id, out var servicio))
            return Task.FromResult<Servicio?>(null);
        _uow.Rastrear(servicio);
        return Task.FromResult<Servicio?>(servicio);
    }

    public Task AgregarAsync(Servicio servicio, CancellationToken ct = default)
    {
        _uow.RegistrarAlta(servicio, () => _db.Servicios[servicio.Id] = servicio);
        return Task.CompletedTask;
    }

    public Task<bool> CuadrillaTieneServicioEnAsync(CuadrillaId cuadrillaId, DateTime fechaHora, CancellationToken ct = default)
    {
        var ocupada = _db.Servicios.Values.Any(s =>
            s.CuadrillaId == cuadrillaId &&
            s.Programacion.FechaHora == fechaHora &&
            s.Estado != EstadoServicio.NoRealizado);
        return Task.FromResult(ocupada);
    }
}

public sealed class FacturaRepositoryEnMemoria : IFacturaRepository
{
    private readonly BaseDeDatosEnMemoria _db;
    private readonly UnitOfWorkEnMemoria _uow;

    public FacturaRepositoryEnMemoria(BaseDeDatosEnMemoria db, UnitOfWorkEnMemoria uow)
    {
        _db = db;
        _uow = uow;
    }

    public Task AgregarAsync(Factura factura, CancellationToken ct = default)
    {
        _uow.RegistrarAlta(factura, () => _db.Facturas[factura.Id] = factura);
        return Task.CompletedTask;
    }

    public Task<bool> ExisteFacturaParaServicioAsync(ServicioId servicioId, CancellationToken ct = default) =>
        Task.FromResult(_db.Facturas.Values.Any(f => f.ServicioId == servicioId));
}
