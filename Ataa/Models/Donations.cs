using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace Ataa.Models
{
    public class Donations
    {
        [Key]
        public int DonationId { get; set; }

        [Required]
        public DateTime DonationDate { get; set; }
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        public int DonorId { get; set; }
        [ForeignKey("DonorId")]
        public virtual Donors Donor { get; set; }

        public virtual ICollection<DonationItems> DonationItems { get; set; }
    }
}