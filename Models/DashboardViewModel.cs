namespace StudyGPT.Web.Models
{
    public class DashboardViewModel
    {
        public int TotalMaterials { get; set; }

        public int TotalChatSessions { get; set; }

        public int TotalQuestions { get; set; }

        public List<RecentMaterialViewModel> RecentMaterials { get; set; }
            = new();

        public List<RecentChatViewModel> RecentChats { get; set; }
            = new();
    }

    public class RecentMaterialViewModel
    {
        public int MaterialId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string FileName { get; set; } = string.Empty;

        public DateTime UploadedDate { get; set; }
    }

    public class RecentChatViewModel
    {
        public int ChatSessionId { get; set; }

        public string? Title { get; set; }

        public DateTime CreatedAt { get; set; }

        public int MessageCount { get; set; }
    }
}