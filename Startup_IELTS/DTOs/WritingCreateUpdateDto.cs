namespace Startup_IELTS.DTOs
{
    public class WritingCreateUpdateDto
    {
        public string? Title { get; set; }
        public string? Source { get; set; }
        public string? Type { get; set; }
        public string? TaskType { get; set; }
        public string? ImageUrl { get; set; }
        public string Question { get; set; } = null!;
        public string? Category { get; set; }
        public bool? Hide { get; set; }
    }
}
