
namespace GrawiaaApp.API.Models
{
    public class ParentLinkRequest
    {
        public int Id { get; set; }
        public string ParentEmail { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public bool IsUsed { get; set; }

        public int ChildId { get; set; }

        // تم إضافة هذا السطر لإصلاح خطأ CS1061 في الـ DbContext
        public virtual ApplicationUser Child { get; set; } = null!;
   
    }
}