namespace Laborator1.Domain;

public record UnitQuantity(int Units) : IQuantity
{
    public decimal Amount => Units;
    public override string ToString() => $"{Units} buc.";
}