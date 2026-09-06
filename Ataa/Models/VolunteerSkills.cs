using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Ataa.Models
{
    public class VolunteerSkills
    {
        [Key]
        public int VolunteerSkillID { get; set; }

        public int VolunteerID { get; set; }
        public int SkillID { get; set; }

        
        public Volunteers Volunteer { get; set; }
        public Skills Skill { get; set; }
    }
}
