
using System.ComponentModel.DataAnnotations;
namespace Ataa.Models


{
    public class Users
    {
        [Key] 
        public int Id { get; set; }

        [Required] 
        [StringLength(50)]
        public string Role { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; }

        [Required]
        [StringLength(255)]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}