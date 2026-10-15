using CleanPro.Application.Cotizaciones;
using CleanPro.Domain.Comun;
using CleanPro.Domain.Cotizaciones;
using Xunit;

namespace CleanPro.Tests;

public class CotizacionTests
{
    private static readonly CalculadoraDePrecio Calculadora =
        new(TarifasPorTipoDeInmueble.SegunBrd(), PoliticaDeDescuento.SegunBrd());

    [Fact(DisplayName = "RB-03: residencial 100 m², servicio único, sin extras = $1,200")]
    public void PrecioResidencialUnico()
    {
        var precio = Calculadora.Calcular(new DatosDelInmueble("Casa", 100m, TipoDeInmueble.Residencial),
            new Frecuencia(TipoDeFrecuencia.Unica), Array.Empty<ExtraSolicitado>());

        Assert.Equal(1200m, precio.Monto);
    }

    [Fact(DisplayName = "RB-03 + H-05: empresarial 100 m² semanal con un extra de $200 = $1,350 + $200")]
    public void PrecioEmpresarialRecurrenteConExtra()
    {
        var extras = new[] { new ExtraSolicitado("X", "Extra", new Dinero(200m)) };

        var precio = Calculadora.Calcular(new DatosDelInmueble("Oficina", 100m, TipoDeInmueble.Empresarial),
            new Frecuencia(TipoDeFrecuencia.Semanal), extras);

        Assert.Equal(1550m, precio.Monto); // 1500 − 10% = 1350; el extra no se descuenta
    }

    [Fact(DisplayName = "SUP-06: un extra que no está en el catálogo no se puede cotizar")]
    public async Task ExtraInexistente()
    {
        var e = new Escenario();

        await Assert.ThrowsAsync<ExtraNoExisteEnCatalogo>(() => e.CotizacionEnviadaAsync(100m,
            TipoDeInmueble.Residencial, TipoDeFrecuencia.Unica, "NO-EXISTE"));
    }

    [Fact(DisplayName = "CU-02: aceptar dentro de la vigencia deja la cotización Aceptada")]
    public async Task AceptarDentroDeVigencia()
    {
        var e = new Escenario();
        var id = await e.CotizacionEnviadaAsync();

        var estado = await e.Sistema.Cotizaciones.ResponderCotizacion(
            new ResponderCotizacionCommand(id, true, Escenario.Lunes9am.AddDays(7))); // lunes siguiente = día hábil 5

        Assert.Equal(EstadoCotizacion.Aceptada, estado);
    }

    [Fact(DisplayName = "RB-08 / US-22: aceptar después de 5 días hábiles deja la cotización Vencida")]
    public async Task AceptarFueraDeVigencia()
    {
        var e = new Escenario();
        var id = await e.CotizacionEnviadaAsync();

        var estado = await e.Sistema.Cotizaciones.ResponderCotizacion(
            new ResponderCotizacionCommand(id, true, Escenario.Lunes9am.AddDays(8)));

        Assert.Equal(EstadoCotizacion.Vencida, estado);
    }

    [Fact(DisplayName = "CU-02: el cliente puede rechazar la cotización")]
    public async Task Rechazar()
    {
        var e = new Escenario();
        var id = await e.CotizacionEnviadaAsync();

        var estado = await e.Sistema.Cotizaciones.ResponderCotizacion(
            new ResponderCotizacionCommand(id, false, Escenario.Lunes9am.AddDays(1)));

        Assert.Equal(EstadoCotizacion.Rechazada, estado);
    }
}
