using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace Ataa.Models
{
    public class Projects
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم المشروع مطلوب")]
        [StringLength(255)]
        [Display(Name = "اسم المشروع")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "وصف المشروع مطلوب")]
        [Display(Name = "الوصف")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "نسبة الإنجاز")]
        public int CompletionPercentage { get; set; } 
     

        [Required(ErrorMessage = "المبلغ المستهدف مطلوب")]
        [Column(TypeName = "decimal(18, 2)")]
        [Display(Name = "المبلغ المستهدف")]
        public decimal TargetAmount { get; set; }

        [Required(ErrorMessage = "المبلغ الحالي مطلوب")]
        [Column(TypeName = "decimal(18, 2)")]
        [Display(Name = "المبلغ الحالي")]
        public decimal CurrentAmount { get; set; }

        [Required(ErrorMessage = "الجهة المنظمة مطلوبة")]
        [StringLength(200)]
        [Display(Name = "الجهة المنظمة")]
        public string OrganizingEntity { get; set; } = string.Empty;

        [Required(ErrorMessage = "تاريخ البداية مطلوب")]
        [DataType(DataType.Date)]
        [Display(Name = "التاريخ")]
        public DateTime Date { get; set; } = DateTime.Now;

        public string? Image { get; set; }

        [NotMapped]
        [Display(Name = "صورة المشروع")]
        public IFormFile? ImageFile { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "حالة المشروع")]
        public string Status { get; set; } = "Published";
    }
}