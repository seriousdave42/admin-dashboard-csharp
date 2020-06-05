using System.ComponentModel.DataAnnotations;
namespace UserAdmin.Models
{
    public class UserInfo
    {
        [Required]
        [MinLength(2)]
        public string FirstName {get; set;}
        [Required]
        [MinLength(2)]
        public string LastName {get; set;}
        [Required]
        [EmailAddress]
        public string Email {get; set;}
        [Required]
        public int AdminLevel {get; set;} = 1;
    }
}