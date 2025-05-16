using System;

namespace Document_Management_System.Models
{
    public class TaskProgressViewModel
    {
        public int Id { get; set; }
        public string TaskName { get; set; }
        public string Description { get; set; }
        public string CategoryName { get; set; }
        public string FolderPath { get; set; }
        public int CurrentProgress { get; set; }
        public int TaskAmount { get; set; }
        public DateTime Deadline { get; set; }
        public string Status { get; set; }
        public string CreatedBy { get; set; }

        // Calculate percentage for progress bar
        public int ProgressPercentage => TaskAmount > 0 ? (int)Math.Min(100, (CurrentProgress * 100) / TaskAmount) : 0;

        // Format for display
        public string ProgressText => $"{CurrentProgress}/{TaskAmount}";

        // Progress bar color based on percentage and status
        public string ProgressBarColor
        {
            get
            {
                if (Status == "Completed") return "bg-success";
                if (Status == "Overdue") return "bg-danger";
                if (ProgressPercentage < 30) return "bg-danger";
                if (ProgressPercentage < 70) return "bg-warning";
                return "bg-success";
            }
        }
    }
}