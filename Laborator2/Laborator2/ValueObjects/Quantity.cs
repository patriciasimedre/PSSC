namespace Laborator2.ValueObjects;

public readonly record struct Quantity(int Value)
{
    public override string ToString() => $"{Value} pcs";
}