using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GrawiaaApp.API.DTOs
{
    // ==================== AUTH ====================
    public class RegisterParentDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;
    }

    public class RegisterChildDto
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full name is required")]
        public string FullName { get; set; } = string.Empty;

        [Range(4, 15, ErrorMessage = "This app is for children aged 4 to 15 only")]
        public int Age { get; set; }

        public string? CharacterType { get; set; }
        public string? CharacterName { get; set; }
        public string? ParentEmail { get; set; }
    }

    public class LoginDto
    {
        [Required(ErrorMessage = "Enter username or email")]
        public string UsernameOrEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter password")]
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int UserId { get; set; }
        public bool PasswordResetRequired { get; set; }
    }

    // ==================== DAILY LOGS ====================
    public class AutoSaveLogDto
    {
        public string? MoodEmoji { get; set; }
        public int WaterCups { get; set; }
        public string? Note { get; set; }
        public string? HomeworkStatus { get; set; }
    }

    public class CreateDailyLogDto
    {
        public string? MoodEmoji { get; set; }
        public int WaterCups { get; set; }
        public string? FoodNote { get; set; }
        public string? ActivityNote { get; set; }
        public string? LearningNote { get; set; }
        public string? SleepNote { get; set; }
    }

    // ==================== GAMING SYSTEM ====================
    public class AddTaskDto
    {
        public int ChildId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Points { get; set; }
        public DateTime DueDate { get; set; }
    }

    public class AddRewardDto
    {
        public string Title { get; set; } = string.Empty;
        public int Cost { get; set; }
        public string? Description { get; set; }
        public int ChildId { get; set; }
    }

    public class ClaimRewardDto
    {
        public int RewardId { get; set; }
    }

    // ==================== PARENT FEATURES ====================
    public class ParentFeedbackDto
    {
        public int LogId { get; set; }
        public string Content { get; set; } = string.Empty;
    }

    public class ConfirmLinkDto
    {
        public string Token { get; set; } = string.Empty;
    }

    // ==================== COMMUNICATION ====================
    public class ChildMessageDto
    {
        public string Content { get; set; } = string.Empty;
    }

    public class DailyQuestionAnswerDto
    {
        public string Answer { get; set; } = string.Empty;
    }

    // ==================== PROFILE & STATS ====================
    public class ChildProfileDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public int TrustScore { get; set; }
        public string? CharacterName { get; set; }
        public string? CharacterType { get; set; }
        public bool IsLinked { get; set; }
        public List<BadgeDto> Badges { get; set; } = new();
    }

    public class BadgeDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string IconUrl { get; set; } = string.Empty;
        public DateTime UnlockedAt { get; set; }
    }

    public class StatsResponseDto
    {
        public List<DailyLogSummaryDto> WeeklyLogs { get; set; } = new();
        public CategoryDistribution PieChartData { get; set; } = new();
    }

    public class DailyLogSummaryDto
    {
        public string Date { get; set; } = string.Empty;
        public int Water { get; set; }
        public string Mood { get; set; } = string.Empty;
    }

    public class CategoryDistribution
    {
        public float StudyPercentage { get; set; }
        public float SportsPercentage { get; set; }
        public float FoodPercentage { get; set; }
    }

    // ==================== ANALYSIS (NLP Team) ⭐ ====================
    public class AnalyzeTextDto
    {
        public string Emotion { get; set; } = string.Empty;    // "Joy", "Sadness"
        public double Sentiment { get; set; }                  // 0.0 to 1.0
        public bool IsHonest { get; set; }
        public string? DetectedText { get; set; }
    }

    // ==================== ADMIN ====================
    public class AddHobbyDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public string Category { get; set; } = string.Empty;   // "Sports", "Study"
    }
}

