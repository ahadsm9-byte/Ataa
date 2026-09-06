using System.ComponentModel.DataAnnotations;

namespace Ataa.Models
{
    public class Donors
    {
        [Key]
        public int DonorId { get; set; }

        [Required(ErrorMessage = "رقم الجوال مطلوب")]
        [Display(Name = "رقم الجوال")]
        public string Phone { get; set; }

        [Display(Name = "الاسم (اختياري)")]
        public string Name { get; set; }

        public virtual ICollection<Donations> Donations { get; set; }
    }
}