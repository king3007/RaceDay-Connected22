using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RACEDAY_API_POE.Models
{
    public class Result
    {
        [Key]
        public int ResultId { get; set; }

        [Required]
        public int EnrolmentId { get; set; }

        public TimeSpan? FinishTime { get; set; }

        public int? FinishingPosition { get; set; }

        [ForeignKey("EnrolmentId")]
        public Enrolment Enrolment { get; set; } = null!;
    }
}
