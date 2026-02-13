
using GrawiaaApp.API.Models;

public class TrustScoreHistory
{
    public int Id { get; set; }
    // غيرنا double لـ int عشان تتوافق مع الـ TrustScore اللي في الـ User
    public int ScoreAtTimestamp { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    public string Reason { get; set; } = string.Empty;
    public int ChildId { get; set; }
    public virtual ApplicationUser Child { get; set; } = null!;
}