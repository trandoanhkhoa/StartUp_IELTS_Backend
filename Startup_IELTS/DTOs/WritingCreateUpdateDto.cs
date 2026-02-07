namespace Startup_IELTS.DTOs
{
    public class WritingCreateUpdateDto
    {
        public string? TaskType { get; set; }
        public string? ImageUrl { get; set; }
        public string Question { get; set; } = null!;
        public bool? Hide { get; set; }
    }
}
