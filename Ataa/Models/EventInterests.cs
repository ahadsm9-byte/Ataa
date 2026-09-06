using System.ComponentModel.DataAnnotations;

namespace Ataa.Models
{
    public class EventInterests
    {

        [Key]
        public int EventInterestID { get; set; }

        public int EventID { get; set; }
        public int InterestID { get; set; }

        public Events Event { get; set; }
        public Skills Skill { get; set; }
    
}
}
