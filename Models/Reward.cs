namespace GrawiaaApp.API.Models
{
    public class Reward
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Cost { get; set; }
        public bool IsRedeemed { get; set; } = false;

        public string Description { get; set; } = string.Empty;
        public int ChildId { get; set; }
        public virtual ApplicationUser Child { get; set; } = null!;
    }
}