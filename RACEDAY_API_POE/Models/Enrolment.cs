using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RACEDAY_API_POE.Models
{
    // The Enrolment class represents an enrolment of a participant in a specific category of an event.
    public class Enrolment
    {
        [Key]
        public int UniqueId { get; set; }

        [Required]
        public int ParticipantId { get; set; }

        [Required]
        public int EventId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Registered";

        [ForeignKey("ParticipantId")]
        public Participant Participant { get; set; } = null!;

        [ForeignKey("EventId")]
        public Event Event { get; set; } = null!;

        [ForeignKey("CategoryId")]
        public Category Category { get; set; } = null!;

        public Result? Result { get; set; }
    }
}
