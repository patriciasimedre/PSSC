namespace ShoppingCartApp.Models;

public record KilogramQuantity(decimal Kilograms) : IQuantity
{
    public decimal GetValue() => Kilograms;
    public string GetDescription() => $"{Kilograms} kg";
}