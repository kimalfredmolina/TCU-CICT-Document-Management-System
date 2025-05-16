namespace Document_Management_System.Models
{
    public class TaskViewModel
    {
        public int Id { get; set; }
        public string TaskName { get; set; }
        public string Description { get; set; }
        public string CategoryName { get; set; }
        public DateTime Deadline { get; set; }
        public string Status { get; set; }
        public string CreatedBy { get; set; }
        public int TaskAmount { get; set; }
        public int FilesCount { get; set; }
        public int CompletionPercentage => TaskAmount > 0 ? Math.Min(100, (FilesCount * 100) / TaskAmount) : 0;
    }
}
