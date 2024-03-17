using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rocky_Models.Models
{
    public class ApplicationUser
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; }

        public string UserName {get; set; }

        public string Email { get; set; }

        public bool EmailConfirmed { get; set; }

        public string PhoneNumber { get; set; }

        [NotMapped]
        public string StreetAddress { get; set; }

        [NotMapped]
        public string City { get; set; }

        [NotMapped]
        public string State { get; set; }

        [NotMapped]
        public string PostalCode { get; set; }
        
    }
}
