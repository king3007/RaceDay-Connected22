namespace RACEDAY_API_POE.DTOs
{
    public class UserProfileDto
    {
        // This DTO is used to transfer user profile data between the client and server.
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
    }
}
