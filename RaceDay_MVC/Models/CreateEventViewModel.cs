using System.ComponentModel.DataAnnotations;
namespace RaceDay_MVC.Models;
public class CreateEventViewModel {
 [Required, StringLength(200)] public string Name { get; set; } = "";
 public string? Description { get; set; }
 [Required] public DateTime EventDate { get; set; } = DateTime.Today.AddDays(1);
 public IFormFile? BannerImage { get; set; }
}
