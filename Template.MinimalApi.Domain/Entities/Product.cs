namespace Template.MinimalApi.Domain.Entities;

public sealed class Product
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public bool Active { get; private set; } = true;
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

    private Product() { } 

    public Product(string name, decimal price)
    {
        Update(name, price);
        Active = true;
    }

    public void Update(string name, decimal price)
    {
        Name = name.Trim();
        Price = price;
    }

    public void Deactivate() => Active = false;
}
