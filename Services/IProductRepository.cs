using ProductImportPractice.Models;

namespace ProductImportPractice.Services
{
    public interface IProductRepository
    {
        IReadOnlyList<Product> GetAll();

        bool ContainsSku(string sku);

        void AddRange(IEnumerable<Product> products);

        void Remove(int id);

        void Clear();

        void Update(Product product);
    }
}
