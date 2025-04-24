public class Category
{
    public int CategoryId { get; set; }
    public string Name { get; set; }

    // Navigation property
    public ICollection<Area> Areas { get; set; }
}
