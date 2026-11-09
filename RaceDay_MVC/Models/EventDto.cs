namespace RaceDay_MVC.Models;
public class EventDto {
 public int EventId { get; set; }
 public string Name { get; set; } = "";
 public string? Description { get; set; }
 public DateTime EventDate { get; set; }
 public string? OrganiserName { get; set; }
 public List<CategoryDto> Categories { get; set; } = new();
}
public class CategoryDto {
 public int CategoryId { get; set; }
 public string Name { get; set; } = "";
 public string CategoryType { get; set; } = "";
}
