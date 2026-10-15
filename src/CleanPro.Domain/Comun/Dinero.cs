namespace CleanPro.Domain.Comun;

/// <summary>
/// Value Object compartido: monto en pesos mexicanos.
/// Evita sumar números sueltos y redondear distinto en cada lugar (PA-02: errores de facturación).
/// </summary>
public readonly record struct Dinero
{
    public const string Moneda = "MXN";

    public decimal Monto { get; }

    public Dinero(decimal monto)
    {
        if (monto < 0)
            throw new ArgumentOutOfRangeException(nameof(monto), "El monto no puede ser negativo.");
        Monto = decimal.Round(monto, 2, MidpointRounding.AwayFromZero);
    }

    public static Dinero Cero => new(0m);

    public Dinero Sumar(Dinero otro) => new(Monto + otro.Monto);

    public Dinero Restar(Dinero otro) => new(Monto - otro.Monto);

    /// <summary>Devuelve el <paramref name="porcentaje"/> % de este monto (por ejemplo 50 → la mitad).</summary>
    public Dinero Porcentaje(decimal porcentaje) => new(Monto * porcentaje / 100m);

    public override string ToString() => $"{Monto:N2} {Moneda}";
}
