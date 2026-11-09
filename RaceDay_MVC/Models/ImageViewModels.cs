using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
namespace RaceDay_MVC.Models;
public class EventBannerViewModel {
 public int EventId { get; set; }
 [BindNever] public string? EventName { get; set; }
 [BindNever] public string? BannerPath { get; set; }
 public IFormFile? BannerImage { get; set; }
}
public class ProfileViewModel {
 [Required, StringLength(100)] public string FirstName { get; set; } = "";
 [Required, StringLength(100)] public string LastName { get; set; } = "";
 [StringLength(20)] public string? PhoneNumber { get; set; }
 [BindNever] public string? Email { get; set; }
 [BindNever] public string? Role { get; set; }
 [BindNever] public string? PhotoPath { get; set; }
 public IFormFile? ProfileImage { get; set; }
}
