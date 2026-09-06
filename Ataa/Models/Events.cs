using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ataa.Models
{
    public class Events
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم الفعالية مطلوب")]
        [StringLength(200)]
        public string Name { get; set; }

        [Required(ErrorMessage = "وصف الفعالية مطلوب")]
        public string Description { get; set; }

        [Required(ErrorMessage = "تاريخ ووقت الفعالية مطلوب")]
        [DataType(DataType.DateTime)] 
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "الموقع مطلوب")]
        [StringLength(255)]
        public string Location { get; set; }

        [Required(ErrorMessage = "السعة المطلوبة غير محددة")]
        public int Capacity { get; set; }

        [Required]
        [StringLength(100)]
        public string Category { get; set; } 
        [Required]
        [Column(TypeName = "decimal(5, 2)")]
        public decimal Hours { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; }
        public string? Image { get; set; }

        [Required(ErrorMessage = "الجهة المنظمة مطلوبة")]
        [StringLength(200)]
        [Column("organizing_entity")]
        public string OrganizingEntity { get; set; }


        [NotMapped]
        public IFormFile ImageFile { get; set; }
        public string? ExecutionMechanism { get; set; } 
        public string? GoalsReport { get; set; }    
        public string? ExtraImages { get; set; }

        public virtual ICollection<EventRegistrations>? EventRegistrations { get; set; }
    }
}