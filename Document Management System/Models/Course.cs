public class Course
{
    public int CourseId { get; set; }
    public int AreaId { get; set; }
    public string Name { get; set; }

    // Navigation
    public Area Area { get; set; }
    public ICollection<YearFolder> YearFolders { get; set; }
}
