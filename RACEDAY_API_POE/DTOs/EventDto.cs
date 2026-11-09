namespace RACEDAY_API_POE.DTOs
{
    public class CreateEventDto
    {
        // Properties for the CreateEventDto class
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime EventDate { get; set; }
    }

    public class UpdateEventDto
    {
        // Properties for the UpdateEventDto class
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime EventDate { get; set; }
    }
}
