using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RACEDAY_API_POE.Models
{
    // The Category class represents a category within an event, which can have multiple enrolments.
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        public int EventId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string CategoryType { get; set; } = string.Empty;

        [ForeignKey("EventId")]
        public Event Event { get; set; } = null!;

        public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
    }
}
