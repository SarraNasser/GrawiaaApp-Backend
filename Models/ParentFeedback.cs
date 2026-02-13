
using GrawiaaApp.API.Models;

public class ParentFeedback
{
    public int Id { get; set; }

    // غيرنا LogId لـ DailyLogEntryId عشان تطابق الميجريشن بتاعك
    public int DailyLogEntryId { get; set; }

    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // العلاقة
    public virtual DailyLogEntry DailyLogEntry { get; set; } = null!;
}