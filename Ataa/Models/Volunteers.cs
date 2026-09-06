using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ataa.Models
{
    public class Volunteers
    {
        [Key]
        public int VolunteerID { get; set; }

        [Required(ErrorMessage = "الاسم مطلوب")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "صيغة البريد غير صحيحة")]
        [StringLength(255)]
        public string Email { get; set; }

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        public string Password { get; set; }

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [StringLength(20)]
        public string Phone { get; set; }

        [StringLength(10)]
        public string? Gender { get; set; }

        [StringLength(200)]
        public string? Location { get; set; }

        public string? Shift { get; set; }

        public string? AvailableDays { get; set; }

        [NotMapped]
        public string? Skills { get; set; }

        [NotMapped]
        public string? Interest { get; set; }
    }
}