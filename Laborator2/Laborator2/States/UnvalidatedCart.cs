using Laborator2.Interfaces;

namespace Laborator2.States
{
    public class UnvalidatedCart : IShoppingCartState
    {
        public string StateName => "Unvalidated Cart";
    }
}