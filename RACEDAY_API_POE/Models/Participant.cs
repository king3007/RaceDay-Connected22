using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RACEDAY_API_POE.Models
{
    //Participant class represents a participant in the race day application. It contains properties for the participant's ID, associated user ID, date of birth, gender, and navigation properties for the related User and Enrolments.
    public class Participant
    {
        [Key]
        public int ParticipantId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [MaxLength(10)]
        public string Gender { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
    }
}
