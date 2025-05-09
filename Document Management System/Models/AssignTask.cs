using Document_Management_System.Models;
using System.ComponentModel.DataAnnotations.Schema;

[Table("AssignTask")]
public class AssignTask 
{
    public int Id { get; set; }
    public int FolderAccessId { get; set; }
    public string UserId { get; set; }
    public string TaskName { get; set; }
    public string Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime Deadline { get; set; }
    public string Status { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedByUserId { get; set; }
    public FolderAccess FolderAccess { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; }
    public Users User { get; set; }
    public Users CreatedByUser { get; set; }
}
