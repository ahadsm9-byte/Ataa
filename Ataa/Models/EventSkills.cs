using System.ComponentModel.DataAnnotations;

namespace Ataa.Models
{
    public class EventSkills
    {
        [Key]
        public int EventSkillID { get; set; }

        public int EventID { get; set; }
        public int SkillID { get; set; }

        public Events Event { get; set; }
        public Interests Interest { get; set; }
    }
}
