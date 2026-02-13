
using Microsoft.AspNetCore.Identity;

namespace GrawiaaApp.API.Models
{
    public class ApplicationUser : IdentityUser<int>
    {
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // "Parent" or "Child"
        public int? Age { get; set; }

        // --- تعديلات الشخصية (صورة 1 و 6) ---
        public string? CharacterName { get; set; } // الاسم اللي الطفل اختاره (مثلاً: ليفي)
        public string? CharacterType { get; set; } // نوع الشخصية (Sprout, Robot, الخ) - مهم للـ SRS

        public int TrustScore { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // --- علاقات الأب والطفل (SRS 1.3) ---
        public int? ParentId { get; set; }

        public bool IsBanned { get; set; } = false;
        public string? BannedReason { get; set; }
        public DateTime? BannedAt { get; set; }

        public virtual ApplicationUser? Parent { get; set; }
        public virtual ICollection<ApplicationUser> Children { get; set; } = new List<ApplicationUser>();

        // --- المجموعات المرتبطة (Collections) ---
        public virtual ICollection<ChildTask> Tasks { get; set; } = new List<ChildTask>();
        public virtual ICollection<DailyLogEntry> Logs { get; set; } = new List<DailyLogEntry>();
        public virtual ICollection<ParentLinkRequest> ParentLinkRequests { get; set; } = new List<ParentLinkRequest>();
        public virtual ICollection<Badge> Badges { get; set; } = new List<Badge>();
        public virtual ICollection<Reward> Rewards { get; set; } = new List<Reward>();
        public virtual ICollection<TrustScoreHistory> TrustHistories { get; set; } = new List<TrustScoreHistory>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public virtual ICollection<ChildHobby> ChildHobbies { get; set; } = new List<ChildHobby>();

        // --- بند الحماية (SRS 1.5) ---
        public bool PasswordResetRequired { get; set; } = false;

        // ملاحظة: ChildProfile ممكن نستغنى عنه لو كل بياناته موجودة هنا مباشرة لتسهيل الكود
        public virtual ChildProfile? ChildProfile { get; set; }
    }
}