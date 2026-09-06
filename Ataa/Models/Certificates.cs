using System.ComponentModel.DataAnnotations.Schema;

namespace Ataa.Models
{
    public class Certificates
    {
        public int Id { get; set; }
        public int RegistrationId { get; set; }
        public string SupervisorName { get; set; }
        public DateTime IssueDate { get; set; }
        [Column(TypeName = "nvarchar(max)")] 
        public string? AddSignature { get; set; }
     
        public virtual EventRegistrations Registration { get; set; }
    }
}
