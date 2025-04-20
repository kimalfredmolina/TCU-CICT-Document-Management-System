using System;
using System.Collections.Generic;

namespace Document_Management_System.Models
{
    public class FolderModel
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty; // Full path to the folder
        public string Category { get; set; } = string.Empty; // Category (e.g., Thesis, ALCU)
        public DateTime ModifiedDate { get; set; } = DateTime.Now;
        public int ItemCount { get; set; } = 0;
        public List<FolderModel> SubFolders { get; set; } = new List<FolderModel>();
        public bool IsExpanded { get; set; } = false;
    }
}
