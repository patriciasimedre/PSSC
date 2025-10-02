using System;
using ShoppingCartApp.Models;
using ShoppingCartApp.Services;

class Program
{
    static void Main()
    {
        var cart = new ShoppingCart();
        bool running = true;

        while (running)
        {
            Console.WriteLine("\n--- Meniu ---");
            Console.WriteLine("1. Adaugă produs");
            Console.WriteLine("2. Elimină produs");
            Console.WriteLine("3. Afișează coș");
            Console.WriteLine("4. Ieșire");
            Console.Write("Alege o opțiune: ");
            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    Console.Write("Nume produs: ");
                    var name = Console.ReadLine() ?? "";

                    Console.Write("Preț unitar (lei): ");
                    decimal price = decimal.Parse(Console.ReadLine() ?? "0");

                    Console.Write("Tip cantitate (u pentru unități / k pentru kilograme): ");
                    var type = Console.ReadLine();

                    IQuantity quantity;
                    if (type?.ToLower() == "u")
                    {
                        Console.Write("Număr de unități: ");
                        int units = int.Parse(Console.ReadLine() ?? "0");
                        quantity = new UnitQuantity(units);
                    }
                    else
                    {
                        Console.Write("Cantitate în kg: ");
                        decimal kg = decimal.Parse(Console.ReadLine() ?? "0");
                        quantity = new KilogramQuantity(kg);
                    }

                    cart.AddProduct(new Product(name, price, quantity));
                    Console.WriteLine("Produs adăugat!");
                    break;

                case "2":
                    Console.Write("Nume produs de eliminat: ");
                    var removeName = Console.ReadLine() ?? "";
                    cart.RemoveProduct(removeName);
                    break;

                case "3":
                    cart.ShowCart();
                    break;

                case "4":
                    running = false;
                    break;

                default:
                    Console.WriteLine("Opțiune invalidă.");
                    break;
            }
        }
    }
}