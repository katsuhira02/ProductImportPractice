using ProductImportPractice.Models;

namespace ProductImportPractice.Services
{
    public class InMemoryProductRepository : IProductRepository
    {
        private readonly List<Product> _products = new();
        private int _nextId = 1;

        public IReadOnlyList<Product> GetAll()
        {
            return _products.AsReadOnly();
        }

        public bool ContainsSku(string sku)
        {
            return _products.Any(p =>
                p.Sku.Equals(sku, StringComparison.OrdinalIgnoreCase));
        }

        public void AddRange(IEnumerable<Product> products)
        {
            foreach (var product in products)
            {
                product.Id = _nextId++;
                _products.Add(product);
            }
        }

        public void Remove(int id)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product != null)
            {
                _products.Remove(product);
            }
        }

        public void Clear()
        {
            _products.Clear();
            _nextId = 1;
        }

        public void Update(Product product)
        {
            var existing = _products.FirstOrDefault(p => p.Id == product.Id);
            if (existing != null)
            {
                existing.Price = product.Price;
                existing.Quantity = product.Quantity;
                existing.Name = product.Name;
                existing.Sku = product.Sku;
            }
        }
    }
}
