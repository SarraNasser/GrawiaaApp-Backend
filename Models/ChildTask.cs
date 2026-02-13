
namespace GrawiaaApp.API.Models
{
    public class ChildTask
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; } // وصف المهمة
        public int Points { get; set; } // تغيير لـ int ليتوافق مع الـ Controller
        public bool IsCompleted { get; set; } = false;
        public DateTime DueDate { get; set; } // ميعاد التسليم (مهم للـ SRS)
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; } // تاريخ الإنجاز الفعلي

        // علاقة المهمة بالطفل
        public int ChildId { get; set; }
        public virtual ApplicationUser Child { get; set; } = null!;
    }
}