
//using System;
//using System.Collections.Generic;

//namespace GrawiaaApp.API.Models
//{
//    public class DailyLogEntry
//    {
//        public int Id { get; set; }
//        public int ChildId { get; set; }
//        public virtual ApplicationUser Child { get; set; } = null!;

//        public DateTime LogDate { get; set; } = DateTime.UtcNow;

//        // الحقول الأساسية
//        public int WaterCups { get; set; }
//        public string? MoodEmoji { get; set; }
//        public string? FoodFruitsVeggies { get; set; }

//        // التعديلات لتطابق الـ Controller والـ SRS
//        public string? ReligiousPractice { get; set; } // بدلاً من Religion
//        public string? ChoresStatus { get; set; }      // بدلاً من Chores
//        public string? HomeworkStatus { get; set; }    // الحقل اللي كان ناقص ومسبب الإيرور
//        public string? Sports { get; set; }
//        public string? HobbiesToday { get; set; }
//        public string? Note { get; set; }              // بدلاً من MessageToParent لتشمل أي ملاحظات

//        // الحقول الإضافية المطلوبة في الـ SRS
//        public string? SocialInteraction { get; set; } // كان مع مين النهاردة؟

//        public virtual ICollection<ParentFeedback> Feedbacks { get; set; } = new List<ParentFeedback>();
//    }
//}
using GrawiaaApp.API.Models;

public class DailyLogEntry
{
    public int Id { get; set; }
    public int ChildId { get; set; }
    public virtual ApplicationUser Child { get; set; } = null!;
    public DateTime LogDate { get; set; } = DateTime.UtcNow;

    public int WaterCups { get; set; }
    public string? MoodEmoji { get; set; }
    public string? FoodFruitsVeggies { get; set; }
    public string? ReligiousPractice { get; set; }
    public string? ChoresStatus { get; set; }
    public string? HomeworkStatus { get; set; }
    public string? Sports { get; set; }
    public string? HobbiesToday { get; set; }
    public string? SocialInteraction { get; set; }

    // --- حقول الـ NLP (استقبال شغل تيم الـ NLP) ---
    public string? DetectedEmotion { get; set; } // Joy, Sad, Anger...
    public double SentimentScore { get; set; }  // 0.0 to 1.0
    public bool IsHonest { get; set; } = true;  // Honesty Check
    public int SleepHours { get; set; } // عشان صورة رقم 4

    // --- التشفير (SRS R-Privacy) ---
    public string? Note { get; set; } // هذا النص سيتم تشفيره قبل الحفظ

    public virtual ICollection<ParentFeedback> Feedbacks { get; set; } = new List<ParentFeedback>();
}