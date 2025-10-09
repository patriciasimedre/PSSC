namespace Laborator1.Domain;

public record Cart(List<CartLine> Lines)
{
    public static Cart Empty() => new(new List<CartLine>());

    public Cart AddLine(CartLine line)
    {
        // yes, facem copie; records + imutabil-ish vibes
        var copy = new List<CartLine>(Lines) { line };
        return this with { Lines = copy };
    }

    public Cart RemoveAt(int index)
    {
        if (index < 0 || index >= Lines.Count) return this;
        var copy = new List<CartLine>(Lines);
        copy.RemoveAt(index);
        return this with { Lines = copy };
    }

    public Cart RemoveByName(string name)
    {
        var copy = new List<CartLine>(Lines);
        var idx = copy.FindIndex(l =>
            string.Equals(l.Product.Name, name, StringComparison.OrdinalIgnoreCase));
        if (idx >= 0) copy.RemoveAt(idx);
        return this with { Lines = copy };
    }

    public decimal Total() => Lines.Sum(l => l.LineTotal());

    public void Print()
    {
        if (Lines.Count == 0)
        {
            Console.WriteLine("Cosul este gol.");
            return;
        }

        for (int i = 0; i < Lines.Count; i++)
            Console.WriteLine($"{i + 1}. {Lines[i]}");

        Console.WriteLine(new string('-', 40));
        Console.WriteLine($"TOTAL: {Total():0.00} lei");
    }
}