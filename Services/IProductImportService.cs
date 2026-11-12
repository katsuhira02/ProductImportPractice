using ProductImportPractice.Models;

namespace ProductImportPractice.Services
{
    public interface IProductImportService
    {
        ImportResult Import(Stream fileStream);
        byte[] Export(IEnumerable<Product> products);
    }
}
