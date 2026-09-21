namespace StudyGPT.Web.Models
{
    public class TeacherMaterialDetailsViewModel
    {
        public int MaterialId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string? FileType { get; set; }

        public DateTime UploadedDate { get; set; }

        public TeacherInfoViewModel? Teacher { get; set; }

        public SubjectInfoViewModel? Subject { get; set; }
    }

    public class TeacherInfoViewModel
    {
        public int TeacherId { get; set; }

        public string? TeacherName { get; set; }
    }

    public class SubjectInfoViewModel
    {
        public int SubjectId { get; set; }

        public string? SubjectName { get; set; }

        public string? Description { get; set; }
    }
}