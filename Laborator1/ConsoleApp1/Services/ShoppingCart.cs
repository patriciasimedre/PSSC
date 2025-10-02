using ShoppingCartApp.Models;

namespace ShoppingCartApp.Services;

public class ShoppingCart
{
    private readonly List<Product> _products = new();

    public void AddProduct(Product product) => _products.Add(product);

    public void RemoveProduct(string name)
    {
        var product = _products.Find(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (product != null)
        {
            _products.Remove(product);
            Console.WriteLine($"Produsul {name} a fost eliminat din coș.");
        }
        else
        {
            Console.WriteLine($"Produsul {name} nu există în coș.");
        }
    }

    public void ShowCart()
    {
        if (_products.Count == 0)
        {
            Console.WriteLine("Coșul este gol.");
            return;
        }

        Console.WriteLine("Conținutul coșului:");
        foreach (var p in _products)
        {
            Console.WriteLine(
                $"- {p.Name} ({p.Quantity.GetDescription()}) | Preț unitar: {p.PricePerUnit} lei | Total: {p.GetTotalPrice()} lei");
        }
    }

    public decimal GetTotalPrice() => _products.Sum(p => p.GetTotalPrice());
}