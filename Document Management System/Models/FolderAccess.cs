using Document_Management_System.Models;

public class FolderAccess
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string UserId { get; set; }
    public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
    public string AssignedByUserId { get; set; }

    public Category Category { get; set; }
    public Users User { get; set; }              // The user who gets access
    public Users AssignedByUser { get; set; }    // The user who assigned the access
}
