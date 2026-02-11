using System;
using System.Collections.Generic;

namespace Startup_IELTS.Models;

public partial class Writing
{
    public int Id { get; set; }

    public string? TaskType { get; set; }

    public string? ImageUrl { get; set; }

    public string Question { get; set; } = null!;

    public bool? Hide { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public DateTime? GradedAt { get; set; }

    public string? Title { get; set; }

    public string? Type { get; set; }

    public string? Source { get; set; }

    public string? Category { get; set; }
}
