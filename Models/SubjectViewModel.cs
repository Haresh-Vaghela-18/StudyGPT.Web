namespace StudyGPT.Web.Models
{
    public class SubjectViewModel
    {
        public int SubjectId { get; set; }

        public string SubjectName { get; set; } = string.Empty;

        public string? Description { get; set; }
    }
}