using Laborator2.Interfaces;
using Laborator2.ValueObjects;

namespace Laborator2.Entities
{
    public class ShoppingCart
    {
        public IShoppingCartState State { get; private set; }
        public List<(Product Product, Quantity Quantity)> Items { get; }

        public ShoppingCart(IShoppingCartState initialState)
        {
            State = initialState;
            Items = new();
        }

        public void AddProduct(Product product, Quantity quantity)
        {
            Items.Add((product, quantity));
        }

        public void ChangeState(IShoppingCartState newState)
        {
            State = newState;
        }

        public double GetTotal()
        {
            return Items.Sum(item => item.Product.UnitPrice * item.Quantity.Value);
        }
    }
}