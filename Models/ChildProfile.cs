using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrawiaaApp.API.Models
{
    public class ChildProfile
    {
        [Key]
        public int Id { get; set; }

        public int ChildId { get; set; }

        [ForeignKey("ChildId")]
        public virtual ApplicationUser Child { get; set; } = null!;

        
        public string? SelectedCharacterAvatar { get; set; }
        public int CurrentLevel { get; set; } = 1;
        public double ExperiencePoints { get; set; } = 0;

       
        public int TotalTasksCompleted { get; set; } = 0;
        public int TotalWaterCupsDrank { get; set; } = 0;
        public int StreakDays { get; set; } = 0;


        public string? FavoriteHobby { get; set; }
        public string? HealthGoal { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}