
using Ataa.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ataa.Models
{
    public class VolunteerInterests
    {

        [Key]
        public int VolunteerInterestID { get; set; }

        public int VolunteerID { get; set; }
        public int InterestID { get; set; }

        public Volunteers Volunteer { get; set; }
        public Interests Interest { get; set; }
    }
}
