using CleanPro.Application.Consultas;
using CleanPro.Application.Cotizaciones;
using CleanPro.Application.Ejecucion;
using CleanPro.Application.Facturacion;
using CleanPro.Application.ProgramacionDeServicios;
using CleanPro.Domain.Cotizaciones;
using CleanPro.Domain.Facturacion;
using CleanPro.Domain.Servicios;
using CleanPro.Infrastructure.EnMemoria;

namespace CleanPro.Infrastructure;

/// <summary>
/// Raíz de composición: conecta las implementaciones con las interfaces.
/// Es el único lugar que conoce las clases concretas (inversión de dependencias).
/// </summary>
public sealed class SistemaCleanProEnMemoria
{
    public BaseDeDatosEnMemoria BaseDeDatos { get; } = new();
    public PublicadorDeEventosEnMemoria Publicador { get; } = new();
    public BuzonDeNotificaciones Buzon { get; } = new();

    public CotizacionAppService Cotizaciones { get; }
    public ProgramacionAppService Programacion { get; }
    public EjecucionServicioAppService Ejecucion { get; }
    public FacturacionAppService Facturacion { get; }

    public ServiciosProgramadosQueryHandler ServiciosProgramados { get; }
    public ServiciosDeCuadrillaQueryHandler ServiciosDeCuadrilla { get; }
    public FacturacionPorServicioQueryHandler FacturacionPorServicio { get; }

    public SistemaCleanProEnMemoria()
    {
        Publicador.Suscribir(new NotificarAdministracionAlRegistrarIncidencia(Buzon));
        Publicador.Suscribir(new NotificarClienteAlProgramarServicio(Buzon));

        var uow = new UnitOfWorkEnMemoria(Publicador);
        var cotizaciones = new CotizacionRepositoryEnMemoria(BaseDeDatos, uow);
        var servicios = new ServicioRepositoryEnMemoria(BaseDeDatos, uow);
        var facturas = new FacturaRepositoryEnMemoria(BaseDeDatos, uow);
        var catalogo = new CatalogoDeExtrasEnMemoria();
        var calculadora = new CalculadoraDePrecio(TarifasPorTipoDeInmueble.SegunBrd(), PoliticaDeDescuento.SegunBrd());

        Cotizaciones = new CotizacionAppService(cotizaciones, catalogo, calculadora, uow);
        Programacion = new ProgramacionAppService(cotizaciones, servicios, new DisponibilidadDeCuadrilla(servicios), uow);
        Ejecucion = new EjecucionServicioAppService(servicios, catalogo, uow);
        Facturacion = new FacturacionAppService(servicios, new GeneradorDeFactura(facturas), facturas, uow);

        var consultas = new ConsultasEnMemoria(BaseDeDatos);
        ServiciosProgramados = new ServiciosProgramadosQueryHandler(consultas);
        ServiciosDeCuadrilla = new ServiciosDeCuadrillaQueryHandler(consultas);
        FacturacionPorServicio = new FacturacionPorServicioQueryHandler(consultas);
    }
}
