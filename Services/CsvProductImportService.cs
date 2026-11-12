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
            if (!ValidateHeaders(headers, result))
            {
                return result;
            }

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
                var line = string.Join(AppConstants.Csv.Delimiter,
                    product.Sku,
                    product.Name,
                    product.Price.ToString(CultureInfo.InvariantCulture),
                    product.Quantity.ToString());
                sb.AppendLine(line);
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        private bool ValidateHeaders(string[] headers, ImportResult result)
        {
            var missing = AppConstants.Csv.RequiredHeaders
                .Where(r => !headers.Contains(r, StringComparer.OrdinalIgnoreCase))
                .ToList();

            if (missing.Any())
            {
                result.Errors.Add(new ImportError 
                { 
                    LineNumber = 1, 
                    Message = string.Format(AppConstants.Messages.MissingHeaders, string.Join(", ", missing))
                });
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

            if (string.IsNullOrWhiteSpace(sku))
            {
                result.Errors.Add(new ImportError { LineNumber = lineNumber, Message = AppConstants.Messages.EmptySku });
                return;
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                result.Errors.Add(new ImportError { LineNumber = lineNumber, Message = AppConstants.Messages.EmptyName });
                return;
            }

            if (!decimal.TryParse(priceStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price) && 
                !decimal.TryParse(priceStr, NumberStyles.Any, new CultureInfo("ru-RU"), out price))
            {
                result.Errors.Add(new ImportError { LineNumber = lineNumber, Message = AppConstants.Messages.InvalidPriceFormat });
                return;
            }

            if (price < 0)
            {
                result.Errors.Add(new ImportError { LineNumber = lineNumber, Message = AppConstants.Messages.NegativePrice });
                return;
            }

            if (!int.TryParse(quantityStr, out int quantity))
            {
                result.Errors.Add(new ImportError { LineNumber = lineNumber, Message = AppConstants.Messages.InvalidQuantityFormat });
                return;
            }

            if (quantity < 0)
            {
                result.Errors.Add(new ImportError { LineNumber = lineNumber, Message = AppConstants.Messages.NegativeQuantity });
                return;
            }

            if (skusInFile.Contains(sku))
            {
                result.Errors.Add(new ImportError { LineNumber = lineNumber, Message = string.Format(AppConstants.Messages.DuplicateSkuInFile, sku) });
                return;
            }

            if (_repository.ContainsSku(sku))
            {
                result.Errors.Add(new ImportError { LineNumber = lineNumber, Message = string.Format(AppConstants.Messages.DuplicateSkuInRepository, sku) });
                return;
            }

            skusInFile.Add(sku);
            productsToImport.Add(new Product
            {
                Sku = sku,
                Name = name,
                Price = price,
                Quantity = quantity
            });
        }

        private string GetValue(string[] values, string[] headers, string headerName)
        {
            int index = Array.FindIndex(headers, h => h.Equals(headerName, StringComparison.OrdinalIgnoreCase));
            return index >= 0 && index < values.Length ? values[index].Trim() : string.Empty;
        }
    }
}
