namespace RACEDAY_API_POE.DTOs
{
    public class RegisterDto
    {
        // Properties for the RegisterDto class
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Role { get; set; } = "Participant"; // "Participant" or "Organiser"


        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
    }
}
