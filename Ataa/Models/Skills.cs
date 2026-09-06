using System.ComponentModel.DataAnnotations;

namespace Ataa.Models
{
    public class Skills
    {
        [Key]
        public int SkillID { get; set; }
        public string SkillName { get; set; }

        public ICollection<VolunteerSkills> VolunteerSkills { get; set; }
        public ICollection<EventSkills> EventSkills { get; set; }
    }
}
