using Laborator2.ValueObjects;

namespace Laborator2.Entities;

public class Customer
{
    public string Name { get; }
    public Address Address { get; }

    public Customer(string name, Address address)
    {
        Name = name;
        Address = address;
    }
}