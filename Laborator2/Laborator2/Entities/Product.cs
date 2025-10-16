using Laborator2.ValueObjects;

namespace Laborator2.Entities
{
    public class Product
    {
        public ProductCode Code { get; }
        public string Name { get; }
        public double UnitPrice { get; }

        public Product(ProductCode code, string name, double unitPrice)
        {
            Code = code;
            Name = name;
            UnitPrice = unitPrice;
        }
    }
}