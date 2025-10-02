namespace ShoppingCartApp.Models;

public record UnitQuantity(int Units) : IQuantity
{
    public decimal GetValue() => Units;
    public string GetDescription() => $"{Units} buc";
}