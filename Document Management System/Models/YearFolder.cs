public class YearFolder
{
    public int YearFolderId { get; set; }
    public int CourseId { get; set; }
    public int Year { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public Course Course { get; set; }
}
