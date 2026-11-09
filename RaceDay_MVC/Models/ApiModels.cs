using System.ComponentModel.DataAnnotations;
namespace RaceDay_MVC.Models;
public class UserProfileDto {
 public int UserId { get; set; }
 [Required, StringLength(100)] public string FirstName { get; set; } = "";
 [Required, StringLength(100)] public string LastName { get; set; } = "";
 public string Email { get; set; } = "";
 [StringLength(20)] public string? PhoneNumber { get; set; }
 public string Role { get; set; } = "";
}
public class EnrolmentDto {
 public int UniqueId { get; set; }
 public string Status { get; set; } = "";
 public string EventName { get; set; } = "";
 public DateTime EventDate { get; set; }
 public string CategoryName { get; set; } = "";
 public string ParticipantName { get; set; } = "";
}
public class ResultDto {
 public int ResultId { get; set; }
 public string EventName { get; set; } = "";
 public string ParticipantName { get; set; } = "";
 public string CategoryName { get; set; } = "";
 public TimeSpan? FinishTime { get; set; }
 public int? FinishingPosition { get; set; }
}
public class ParticipantDashboardModel {
 public List<EnrolmentDto> Enrolments { get; set; } = new();
 public List<ResultDto> Results { get; set; } = new();
}
public class AddCategoryModel {
 [Required, StringLength(100)] public string Name { get; set; } = "";
 [Required, StringLength(20)] public string CategoryType { get; set; } = "";
}
public class CaptureResultModel {
 [Range(1,int.MaxValue)] public int EnrolmentId { get; set; }
 public TimeSpan FinishTime { get; set; }
 [Range(1,int.MaxValue)] public int FinishingPosition { get; set; }
}
