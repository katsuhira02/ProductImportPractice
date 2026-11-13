namespace ProductImportPractice.Models
{
    public class Product
    {
        public int Id { get; set; }

        public string Sku { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public string Description { get; set; } = string.Empty;

        public string Currency { get; set; } = "RUB";

        public string CategoryName { get; set; } = "Без категории";

        public int Stock { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}
