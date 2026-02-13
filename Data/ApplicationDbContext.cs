
using GrawiaaApp.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GrawiaaApp.API.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<ChildProfile> ChildProfiles { get; set; }
        public DbSet<ParentLinkRequest> ParentLinkRequests { get; set; }
        public DbSet<DailyLogEntry> DailyLogEntries { get; set; }
        public DbSet<ParentFeedback> ParentFeedbacks { get; set; }
        public DbSet<ChildTask> ChildTasks { get; set; }

        // ده الاسم اللي هنثبته في كل الكنترولرات
        public DbSet<TrustScoreHistory> TrustScoreHistories { get; set; }
        public DbSet<DailyQuestionAnswer> DailyQuestionAnswers { get; set; }

        public DbSet<Hobby> Hobbies { get; set; }
        public DbSet<ChildHobby> ChildHobbies { get; set; }
        public DbSet<Badge> Badges { get; set; }
        public DbSet<Reward> Rewards { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // 1. علاقة الأب بالطفل (Self-Referencing)
            builder.Entity<ApplicationUser>()
                .HasOne(u => u.Parent)
                .WithMany(u => u.Children)
                .HasForeignKey(u => u.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            // 2. علاقة الـ Many-to-Many للهوايات
            builder.Entity<ChildHobby>()
                .HasKey(ch => new { ch.ChildId, ch.HobbyId });

            builder.Entity<ChildHobby>()
                .HasOne(ch => ch.Child)
                .WithMany(u => u.ChildHobbies)
                .HasForeignKey(ch => ch.ChildId);

            builder.Entity<ChildHobby>()
                .HasOne(ch => ch.Hobby)
                .WithMany(h => h.ChildHobbies)
                .HasForeignKey(ch => ch.HobbyId);

            // 3. علاقة المهام بالطفل
            builder.Entity<ChildTask>()
                .HasOne(t => t.Child)
                .WithMany(u => u.Tasks)
                .HasForeignKey(t => t.ChildId);

            // 4. علاقة السجل التاريخي للسكور بالطفل
            builder.Entity<TrustScoreHistory>()
                .HasOne(th => th.Child)
                .WithMany(u => u.TrustHistories)
                .HasForeignKey(th => th.ChildId);

            // 5. علاقة المكافآت بالطفل
            builder.Entity<Reward>()
                .HasOne(r => r.Child)
                .WithMany(u => u.Rewards)
                .HasForeignKey(r => r.ChildId);

            // 6. علاقة الأوسمة (Badges) بالطفل
            builder.Entity<Badge>()
                .HasOne(b => b.Child)
                .WithMany(u => u.Badges)
                .HasForeignKey(b => b.ChildId);

            // 7. علاقة اليوميات بالطفل
            builder.Entity<DailyLogEntry>()
                .HasOne(l => l.Child)
                .WithMany(u => u.Logs)
                .HasForeignKey(l => l.ChildId);

            builder.Entity<ParentFeedback>()
    .HasOne(f => f.DailyLogEntry)
    .WithMany(l => l.Feedbacks)
    .HasForeignKey(f => f.DailyLogEntryId); // نربط بالعمود الموجود فعلياً
        }
    }
}