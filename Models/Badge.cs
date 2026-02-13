using System.ComponentModel.DataAnnotations.Schema;

namespace GrawiaaApp.API.Models
{
    public class Badge
    {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty; // ضيفي الوصف (عشان بند 1.14)
            public string IconUrl { get; set; } = string.Empty;
            public DateTime UnlockedAt { get; set; } = DateTime.UtcNow;

            public int ChildId { get; set; }
            [ForeignKey("ChildId")] // تأكيد الربط
            public virtual ApplicationUser Child { get; set; } = null!;
        }
    }
