namespace Journals.Model
{
    public class ValidationResult
    {
        public string Entity { get; set; }
        public string Key { get; set; }

        public string Message { get; set; }

        public bool IsValid { get; set; }

    }
}