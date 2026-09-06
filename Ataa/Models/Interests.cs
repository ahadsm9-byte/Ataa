using System.ComponentModel.DataAnnotations;

namespace Ataa.Models
{
    public class Interests
    {
        [Key]
        public int InterestID { get; set; }
        public string InterestName { get; set; }

        public ICollection<VolunteerInterests> VolunteerInterests { get; set; }
        public ICollection<EventInterests> EventInterests { get; set; }
    }
}
