namespace ProductImportPractice.Models
{
    public static class AppConstants
    {
        public static class Csv
        {
            public const string Delimiter = ",";
            public const string Extension = ".csv";
            
            public const string HeaderSku = "sku";
            public const string HeaderName = "name";
            public const string HeaderDescription = "description";
            public const string HeaderPrice = "price";
            public const string HeaderCurrency = "currency";
            public const string HeaderCategoryName = "category_name";
            public const string HeaderStock = "stock";
            public const string HeaderImageUrl = "image_url";
            public const string HeaderIsActive = "is_active";

            public static readonly string[] RequiredHeaders = 
            { 
                HeaderSku, HeaderName, HeaderDescription, HeaderPrice, 
                HeaderCurrency, HeaderCategoryName, HeaderStock, 
                HeaderImageUrl, HeaderIsActive 
            };
        }

        public static class Messages
        {
            public const string EmptyFile = "Файл пуст";
            public const string InvalidColumnCount = "Некорректное количество столбцов";
            public const string MissingHeaders = "Отсутствуют обязательные столбцы: {0}";
            public const string EmptySku = "SKU не может быть пустым";
            public const string EmptyName = "Название не может быть пустым";
            public const string InvalidPriceFormat = "Некорректный формат цены";
            public const string PriceMustBePositive = "Цена должна быть строго больше 0";
            public const string InvalidStockFormat = "Некорректный формат остатка";
            public const string NegativeStock = "Остаток не может быть отрицательным";
            public const string InvalidIsActiveFormat = "Поле is_active должно быть true/false/1/0";
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
