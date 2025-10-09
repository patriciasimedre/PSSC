namespace Laborator1.Domain;

public record CartLine(Product Product, IQuantity Quantity)
{
    // Switch expression pentru calculul totalului in functie de cantitate.
    public decimal LineTotal() => Quantity switch
    {
        UnitQuantity u     => Product.PricePerUnit * u.Units,
        KilogramQuantity k => Product.PricePerUnit * k.Kg,
        _                  => 0m // in caz ca viitorul aduce o cantitate din alta galaxie
    };

    public override string ToString()
        => $"{Product.Name} — {Quantity} x {Product.PricePerUnit:0.00} lei = {LineTotal():0.00} lei";
}