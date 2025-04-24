public class Area
{
    public int AreaId { get; set; }
    public int CategoryId { get; set; }
    public string Name { get; set; }

    // Navigation
    public Category Category { get; set; }
    public ICollection<Course> Courses { get; set; }
}
