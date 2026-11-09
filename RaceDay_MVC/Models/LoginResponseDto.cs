namespace RaceDay_MVC.Models;
public class LoginResponseDto {
 public string Token { get; set; } = "";
 public int ExpiresInMinutes { get; set; }
 public UserProfileDto User { get; set; } = new();
}
