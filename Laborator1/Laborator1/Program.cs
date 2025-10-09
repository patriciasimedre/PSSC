using System.Globalization;
using Laborator1.Domain;

Cart cart = Cart.Empty();

while (true)
{
    Console.WriteLine("\n=== MENIU ===");
    Console.WriteLine("1) Reseteaza / Creeaza cos gol");
    Console.WriteLine("2) Adauga produs");
    Console.WriteLine("3) Elimina produs");
    Console.WriteLine("4) Afiseaza cosul");
    Console.WriteLine("0) Iesire");
    Console.Write("Alege optiunea: ");

    var option = Console.ReadLine();
    switch (option)
    {
        case "1":
            cart = Cart.Empty();
            Console.WriteLine("Cos gol creat.");
            break;

        case "2":
            AddProduct();
            break;

        case "3":
            RemoveProduct();
            break;

        case "4":
            cart.Print();
            break;

        case "0":
            return;

        default:
            Console.WriteLine("Optiune invalida.");
            break;
    }
}

void AddProduct()
{
    string name = ReadNonEmpty("Nume produs: ");
    decimal price = ReadDecimal("Pret (lei / unitate sau / kg): ");

    Console.Write("Cantitate la (U)nitati sau (K)ilograme? [U/K]: ");
    var kind = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();

    IQuantity qty = kind switch
    {
        "K" => new KilogramQuantity(ReadDecimal("Kilograme: ")),
        _   => new UnitQuantity(ReadInt("Numar bucati: ", min: 1))
    };

    var product = new Product(name, price);
    var line = new CartLine(product, qty);
    cart = cart.AddLine(line);
    Console.WriteLine($"Adaugat: {line}");
}

void RemoveProduct()
{
    if (cart.Lines.Count == 0)
    {
        Console.WriteLine("Cosul este gol.");
        return;
    }

    cart.Print();
    Console.Write("Sterge dupa (N)ume sau (I)ndex? [N/I]: ");
    var mode = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();

    if (mode == "I")
    {
        int idx = ReadInt($"Index (1..{cart.Lines.Count}): ", 1, cart.Lines.Count) - 1;
        cart = cart.RemoveAt(idx);
        Console.WriteLine("Produs eliminat.");
    }
    else
    {
        string name = ReadNonEmpty("Nume produs de eliminat: ");
        cart = cart.RemoveByName(name);
        Console.WriteLine("Daca exista, produsul a fost eliminat. #believe");
    }
}

// ===== Utils de citire din consola =====
// Nota: codul de mai jos incearca sa te protejeze de tine insuti. Uneori reuseste.

static string ReadNonEmpty(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        var s = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(s)) return s.Trim();
        Console.WriteLine("Valoare necesara.");
    }
}

static int ReadInt(string prompt, int? min = null, int? max = null)
{
    while (true)
    {
        Console.Write(prompt);
        if (int.TryParse(Console.ReadLine(), out int v))
        {
            if (min.HasValue && v < min.Value) { Console.WriteLine($"Minim {min}."); continue; }
            if (max.HasValue && v > max.Value) { Console.WriteLine($"Maxim {max}."); continue; }
            return v;
        }
        Console.WriteLine("Introdu un numar intreg valid (nu litere, nu hieroglife).");
    }
}

static decimal ReadDecimal(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        var s = (Console.ReadLine() ?? "").Trim();

        // se accepta atat virgula cat si punct
        if (decimal.TryParse(s, NumberStyles.Number, CultureInfo.CurrentCulture, out var v) ||
            decimal.TryParse(s.Replace(',', '.'),
                NumberStyles.Number, CultureInfo.InvariantCulture, out v))
        {
            return v;
        }

        Console.WriteLine("Introdu un numar valid (ex: 12.5 sau 12,5).");
    }
}