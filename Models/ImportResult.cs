namespace ProductImportPractice.Models
{
    public class ImportResult
    {
        public int ImportedCount { get; set; }

        public List<ImportError> Errors { get; set; } = new();

        public bool HasErrors => Errors.Count > 0;
    }
}
