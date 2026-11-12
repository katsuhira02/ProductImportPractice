# ФАЙЛЫ ДЛЯ ОТЧЕТА ПО ПРОЕКТУ

## 1. Контроллеры

### File Name: ProductController.cs
```csharp
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ProductImportPractice.Models;
using ProductImportPractice.Services;

namespace ProductImportPractice.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductRepository _repository;
        private readonly IProductImportService _importService;

        public ProductController(IProductRepository repository, IProductImportService importService)
        {
            _repository = repository;
            _importService = importService;
        }

        public IActionResult Index()
        {
            var products = _repository.GetAll();
            return View(products);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            _repository.Remove(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Clear()
        {
            _repository.Clear();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var product = _repository.GetAll().FirstOrDefault(p => p.Id == id);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost]
        public IActionResult Edit(Product product)
        {
            if (product.Price < 0 || product.Quantity < 0)
            {
                ModelState.AddModelError("", "Цена и количество не могут быть отрицательными.");
                return View(product);
            }

            _repository.Update(product);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Export()
        {
            var products = _repository.GetAll();
            var content = _importService.Export(products);
            return File(content, "text/csv", $"products_export_{DateTime.Now:yyyyMMddHHmmss}.csv");
        }

        [HttpGet]
        public IActionResult Import()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Import(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ViewBag.Error = AppConstants.Messages.SelectFile;
                return View();
            }

            if (!file.FileName.EndsWith(AppConstants.Csv.Extension, StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.Error = AppConstants.Messages.OnlyCsvAllowed;
                return View();
            }

            using var stream = file.OpenReadStream();
            var result = _importService.Import(stream);

            return View(AppConstants.Views.ImportResult, result);
        }
    }
}
```

## 2. Модели

### File Name: Product.cs
```csharp
namespace ProductImportPractice.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}
```

### File Name: AppConstants.cs
```csharp
namespace ProductImportPractice.Models
{
    public static class AppConstants
    {
        public static class Csv
        {
            public const string Delimiter = ";";
            public const string Extension = ".csv";
            public const string HeaderSku = "Sku";
            public const string HeaderName = "Name";
            public const string HeaderPrice = "Price";
            public const string HeaderQuantity = "Quantity";
            public static readonly string[] RequiredHeaders = { HeaderSku, HeaderName, HeaderPrice, HeaderQuantity };
        }

        public static class Messages
        {
            public const string EmptyFile = "Файл пуст";
            public const string InvalidColumnCount = "Некорректное количество столбцов";
            public const string MissingHeaders = "Отсутствуют обязательные столбцы: {0}";
            public const string EmptySku = "SKU не может быть пустым";
            public const string EmptyName = "Название не может быть пустым";
            public const string InvalidPriceFormat = "Некорректный формат цены";
            public const string NegativePrice = "Цена не может быть отрицательной";
            public const string InvalidQuantityFormat = "Некорректный формат количества";
            public const string NegativeQuantity = "Количество не может быть отрицательным";
            public const string DuplicateSkuInFile = "Дубликат SKU в файле: {0}";
            public const string DuplicateSkuInRepository = "Товар с SKU {0} уже существует в системе";
            public const string SelectFile = "Пожалуйста, выберите файл для загрузки.";
            public const string OnlyCsvAllowed = "Допускаются только файлы в формате CSV.";
        }

        public static class Views
        {
            public const string ImportResult = "ImportResult";
        }
    }
}
```

## 3. Сервисы

### File Name: CsvProductImportService.cs
```csharp
using System.Globalization;
using System.Text;
using ProductImportPractice.Models;

namespace ProductImportPractice.Services
{
    public class CsvProductImportService : IProductImportService
    {
        private readonly IProductRepository _repository;

        public CsvProductImportService(IProductRepository repository)
        {
            _repository = repository;
        }

        public ImportResult Import(Stream fileStream)
        {
            var result = new ImportResult();
            var productsToImport = new List<Product>();
            var skusInFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            using var reader = new StreamReader(fileStream, Encoding.UTF8);
            var headerLine = reader.ReadLine();
            if (string.IsNullOrWhiteSpace(headerLine))
            {
                result.Errors.Add(new ImportError { LineNumber = 0, Message = AppConstants.Messages.EmptyFile });
                return result;
            }

            var headers = headerLine.Split(AppConstants.Csv.Delimiter).Select(h => h.Trim()).ToArray();
            if (!ValidateHeaders(headers, result)) return result;

            int lineNumber = 1;
            while (!reader.EndOfStream)
            {
                lineNumber++;
                var line = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) continue;

                var values = line.Split(AppConstants.Csv.Delimiter);
                if (values.Length != headers.Length)
                {
                    result.Errors.Add(new ImportError { LineNumber = lineNumber, Message = AppConstants.Messages.InvalidColumnCount });
                    continue;
                }
                ProcessRow(values, headers, lineNumber, productsToImport, skusInFile, result);
            }

            if (productsToImport.Any())
            {
                _repository.AddRange(productsToImport);
                result.ImportedCount = productsToImport.Count;
            }
            return result;
        }

        public byte[] Export(IEnumerable<Product> products)
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Join(AppConstants.Csv.Delimiter, AppConstants.Csv.RequiredHeaders));
            foreach (var product in products)
            {
                var line = string.Join(AppConstants.Csv.Delimiter, product.Sku, product.Name, product.Price.ToString(CultureInfo.InvariantCulture), product.Quantity.ToString());
                sb.AppendLine(line);
            }
            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        private bool ValidateHeaders(string[] headers, ImportResult result)
        {
            var missing = AppConstants.Csv.RequiredHeaders.Where(r => !headers.Contains(r, StringComparer.OrdinalIgnoreCase)).ToList();
            if (missing.Any())
            {
                result.Errors.Add(new ImportError { LineNumber = 1, Message = string.Format(AppConstants.Messages.MissingHeaders, string.Join(", ", missing)) });
                return false;
            }
            return true;
        }

        private void ProcessRow(string[] values, string[] headers, int lineNumber, List<Product> productsToImport, HashSet<string> skusInFile, ImportResult result)
        {
            var sku = GetValue(values, headers, AppConstants.Csv.HeaderSku);
            var name = GetValue(values, headers, AppConstants.Csv.HeaderName);
            var priceStr = GetValue(values, headers, AppConstants.Csv.HeaderPrice);
            var quantityStr = GetValue(values, headers, AppConstants.Csv.HeaderQuantity);

            if (string.IsNullOrWhiteSpace(sku)) { result.Errors.Add(new ImportError { LineNumber = lineNumber, Message = AppConstants.Messages.EmptySku }); return; }
            if (string.IsNullOrWhiteSpace(name)) { result.Errors.Add(new ImportError { LineNumber = lineNumber, Message = AppConstants.Messages.EmptyName }); return; }

            if (!decimal.TryParse(priceStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price) && !decimal.TryParse(priceStr, NumberStyles.Any, new CultureInfo("ru-RU"), out price))
            { result.Errors.Add(new ImportError { LineNumber = lineNumber, Message = AppConstants.Messages.InvalidPriceFormat }); return; }

            if (price < 0) { result.Errors.Add(new ImportError { LineNumber = lineNumber, Message = AppConstants.Messages.NegativePrice }); return; }
            if (!int.TryParse(quantityStr, out int quantity)) { result.Errors.Add(new ImportError { LineNumber = lineNumber, Message = AppConstants.Messages.InvalidQuantityFormat }); return; }
            if (quantity < 0) { result.Errors.Add(new ImportError { LineNumber = lineNumber, Message = AppConstants.Messages.NegativeQuantity }); return; }
            if (skusInFile.Contains(sku)) { result.Errors.Add(new ImportError { LineNumber = lineNumber, Message = string.Format(AppConstants.Messages.DuplicateSkuInFile, sku) }); return; }
            if (_repository.ContainsSku(sku)) { result.Errors.Add(new ImportError { LineNumber = lineNumber, Message = string.Format(AppConstants.Messages.DuplicateSkuInRepository, sku) }); return; }

            skusInFile.Add(sku);
            productsToImport.Add(new Product { Sku = sku, Name = name, Price = price, Quantity = quantity });
        }

        private string GetValue(string[] values, string[] headers, string headerName)
        {
            int index = Array.FindIndex(headers, h => h.Equals(headerName, StringComparison.OrdinalIgnoreCase));
            return index >= 0 && index < values.Length ? values[index].Trim() : string.Empty;
        }
    }
}
```

### File Name: InMemoryProductRepository.cs
```csharp
using ProductImportPractice.Models;

namespace ProductImportPractice.Services
{
    public class InMemoryProductRepository : IProductRepository
    {
        private readonly List<Product> _products = new();
        private int _nextId = 1;

        public IReadOnlyList<Product> GetAll() => _products.AsReadOnly();

        public bool ContainsSku(string sku) => _products.Any(p => p.Sku.Equals(sku, StringComparison.OrdinalIgnoreCase));

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
            if (product != null) _products.Remove(product);
        }

        public void Clear() { _products.Clear(); _nextId = 1; }

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
```

## 4. Конфигурация

### File Name: Program.cs
```csharp
using ProductImportPractice.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<IProductRepository, InMemoryProductRepository>();
builder.Services.AddScoped<IProductImportService, CsvProductImportService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Product/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Product}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
```
