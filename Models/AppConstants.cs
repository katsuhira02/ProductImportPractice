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
