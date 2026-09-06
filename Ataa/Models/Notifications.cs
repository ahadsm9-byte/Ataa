using System;
using System.ComponentModel.DataAnnotations;

namespace Ataa.Models
{
    public class Notifications
    {
        [Key]
        public int Id { get; set; }
        public int VolunteerId { get; set; }
        public string Message { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public bool IsRead { get; set; } = false;

        public virtual Volunteers Volunteer { get; set; }
    }
}
