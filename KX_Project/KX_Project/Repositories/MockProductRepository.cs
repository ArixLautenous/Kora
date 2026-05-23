using KX_Project.Models;
using System.Collections.Generic;
using System.Linq;
public class MockProductRepository : IProductRepository
{
    private readonly List<Product> _products;
    public MockProductRepository()
    {
        // Tạo một số dữ liệu mẫu
        _products = new List<Product>
{
new Product { Id = 1, Name = "iPhone 12 Pro Max", Price = 10990000, Description = "An old flagship smartphone"},
new Product { Id = 2, Name = "OnePlus 15", Price = 18990000, Description = "A flagship smartphone"},
new Product { Id = 3, Name = "Samsung Galaxy A37", Price = 9990000, Description = "A mid-range smartphone"},
new Product { Id = 4, Name = "Xiaomi 17 Max", Price = 27990000, Description = "A flagship smartphone"},
new Product { Id = 5, Name = "vivo X300 Pro", Price = 23990000, Description = "A flagship smartphone"},
new Product { Id = 6, Name = "OPPO Reno 15F", Price = 10990000, Description = "A mid-range smartphone"},
new Product { Id = 7, Name = "Samsung Galaxy Z Fold7", Price = 40990000, Description = "A foldable smartphone"},
new Product { Id = 8, Name = "iPhone 17 Pro Max", Price = 37990000, Description = "A flagship smartphone"},
new Product { Id = 9, Name = "Samsung Galaxy S26+", Price = 22990000, Description = "A flagship smartphone"},

// Thêm các sản phẩm khác
};
    }
    public IEnumerable<Product> GetAll()
    {
        return _products;
    }
    public Product GetById(int id)
    {
        return _products.FirstOrDefault(p => p.Id == id);
    }
    public void Add(Product product)
    {
        product.Id = _products.Max(p => p.Id) + 1;
        _products.Add(product);
    }
    public void Update(Product product)
    {
        var index = _products.FindIndex(p => p.Id == product.Id);
        if (index != -1)
        {
            _products[index] = product;
        }
    }
    public void Delete(int id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product != null)
        {
            _products.Remove(product);
        }
    }
}