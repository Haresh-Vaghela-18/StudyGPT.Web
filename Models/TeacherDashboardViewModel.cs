namespace StudyGPT.Web.Models
{
    public class TeacherDashboardViewModel
    {
        public int TotalSubjects { get; set; }

        public int TotalMaterials { get; set; }

        public int TotalStudents { get; set; }

        public int PdfCount { get; set; }

        public int DocCount { get; set; }

        public int PptCount { get; set; }

        public int ImageCount { get; set; }

        public int TotalDocuments { get; set; }

        public int TotalChunks { get; set; }

        public int TotalEmbeddings { get; set; }

        public List<TeacherRecentMaterialViewModel> RecentUploads { get; set; }
            = new();
    }


    public class TeacherRecentMaterialViewModel
    {
        public int MaterialId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string FileName { get; set; } = string.Empty;

        public DateTime UploadedDate { get; set; }
    }
}