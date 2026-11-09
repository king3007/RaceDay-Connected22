using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace RaceDay_MVC.Models;
public class RegisterDto
{
    [Required, StringLength(100)] public string FirstName { get; set; } = "";
    [Required, StringLength(100)] public string LastName { get; set; } = "";
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required, MinLength(8), DataType(DataType.Password)] public string Password { get; set; } = "";
    [JsonIgnore, Required, Compare(nameof(Password)), DataType(DataType.Password)] public string ConfirmPassword { get; set; } = "";
    [StringLength(20)] public string? PhoneNumber { get; set; }
    [Required, RegularExpression("^(Participant|Organiser)$")] public string Role { get; set; } = "Participant";
    [Required, DataType(DataType.Date)] public DateTime? DateOfBirth { get; set; }
    [Required, RegularExpression("^(Female|Male|Other)$")] public string Gender { get; set; } = "Other";
}
