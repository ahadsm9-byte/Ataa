namespace Ataa.Models
{
    public class EventRegistrations
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public int VolunteerId { get; set; }
        public string Status { get; set; } = "مؤكد";
        public string AttendanceStatus { get; set; } = "لم يحضر"; 
        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        public int? Rating { get; set; } 

        public virtual Events Event { get; set; }
        public virtual Volunteers Volunteer { get; set; }
    }
}
