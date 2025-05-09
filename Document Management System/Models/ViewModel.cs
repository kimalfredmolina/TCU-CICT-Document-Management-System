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
    }
}
