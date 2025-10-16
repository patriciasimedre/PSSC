using Laborator2.ValueObjects;
using Laborator2.Entities;  
using Laborator2.Interfaces;
using Laborator2.States;

class Program
{
    static void Main()
    {
        var cart = new ShoppingCart(new EmptyCart());
        bool running = true;

        while (running)
        {
            Console.WriteLine($"\n=== Coș de cumpărături ({cart.State.StateName}) ===");
            Console.WriteLine("1. Adaugă produs");
            Console.WriteLine("2. Afișează produse");
            Console.WriteLine("3. Afișează total");
            Console.WriteLine("4. Schimbă starea coșului");
            Console.WriteLine("5. Ieșire");
            Console.Write("Alege o opțiune: ");
            var opt = Console.ReadLine();

            switch (opt)
            {
                case "1":
                    Console.Write("Nume produs: ");
                    var name = Console.ReadLine() ?? "";
                    Console.Write("Cod produs: ");
                    var code = new ProductCode(Console.ReadLine() ?? "");
                    Console.Write("Preț unitar: ");
                    double price = double.Parse(Console.ReadLine() ?? "0");
                    Console.Write("Cantitate: ");
                    int qty = int.Parse(Console.ReadLine() ?? "1");

                    cart.AddProduct(new Product(code, name, price), new Quantity(qty));
                    cart.ChangeState(new UnvalidatedCart());
                    Console.WriteLine("✅ Produs adăugat!");
                    break;

                case "2":
                    if (cart.Items.Count == 0)
                        Console.WriteLine("Coșul este gol.");
                    else
                        foreach (var item in cart.Items)
                            Console.WriteLine($"{item.Product.Name} - {item.Quantity} x {item.Product.UnitPrice} lei");
                    break;

                case "3":
                    Console.WriteLine($"💰 Total: {cart.GetTotal()} lei");
                    break;

                case "4":
                    Console.WriteLine("Alege starea: 1=Unvalidated, 2=Validated, 3=Paid");
                    var s = Console.ReadLine();
                    cart.ChangeState(s switch
                    {
                        "1" => new UnvalidatedCart(),
                        "2" => new ValidatedCart(),
                        "3" => new PaidCart(),
                        _ => cart.State
                    });
                    Console.WriteLine($"🔄 Noua stare: {cart.State.StateName}");
                    break;

                case "5":
                    running = false;
                    break;

                default:
                    Console.WriteLine("Opțiune invalidă!");
                    break;
            }
        }
    }
}