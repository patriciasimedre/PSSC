namespace ShoppingCartApp.Models;

public record Product(string Name, decimal PricePerUnit, IQuantity Quantity)
{
    public decimal GetTotalPrice() =>
        Quantity switch
        {
            UnitQuantity u     => u.Units * PricePerUnit,
            KilogramQuantity k => k.Kilograms * PricePerUnit,
            _ => 0
        };
}