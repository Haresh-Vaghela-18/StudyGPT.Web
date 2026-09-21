namespace StudyGPT.Web.Models
{
    public class TeacherMaterialViewModel
    {
        public int MaterialId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string? FileType { get; set; }

        public DateTime UploadedDate { get; set; }
    }
}