namespace Laborator1.Domain;

public record KilogramQuantity(decimal Kg) : IQuantity
{
    public decimal Amount => Kg;
    public override string ToString() => $"{Kg} kg";
}