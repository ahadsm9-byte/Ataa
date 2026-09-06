using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Ataa.Models
{
    public class DonationItems
    {
        [Key]
        public int DonationItemId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public int DonationId { get; set; }
        [ForeignKey("DonationId")]
        public virtual Donations Donation { get; set; }

        [Required]
        public int ProjectId { get; set; }
        [ForeignKey("ProjectId")]
        public virtual Projects Project { get; set; }
    }
}