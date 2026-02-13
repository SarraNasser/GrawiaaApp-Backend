namespace GrawiaaApp.API.Models
{
    public class DailyQuestionAnswer
    {
        public int Id { get; set; }
        public int ChildId { get; set; }
        public virtual ApplicationUser Child { get; set; }
        public string Answer { get; set; } = string.Empty;
        public DateTime DateAnswered { get; set; }
    }
}
