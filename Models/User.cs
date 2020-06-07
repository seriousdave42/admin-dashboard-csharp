using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Collections.Generic;

namespace UserAdmin.Models
{
    public class User
    {
        [Key]
        public int UserId {get; set;}
        [Required]
        [MinLength(2)]
        public string FirstName {get; set;}
        [Required]
        [MinLength(2)]
        public string LastName {get; set;}
        [Required]
        [EmailAddress]
        public string Email {get; set;}
        [DataType(DataType.Password)]
        [Required]
        [MinLength(8)]
        [RegularExpression(@"^(?=.*[a-zA-Z])(?=.*[0-9])(?=.*[!@#$%^&*])[a-zA-Z0-9!@#$%^&*]{8,}$", ErrorMessage="Password must contain at least one letter, one number, and one special character")]
        public string Password {get; set;}
        [NotMapped]
        [Compare("Password")]
        [DataType(DataType.Password)]
        public string ConfirmPW {get; set;}
        [MaxLength(50)]
        public string Description {get; set;}
        [Required]
        public int AdminLevel {get; set;} = 1;
        public List<Comment> UserComments {get; set;}
        [InverseProperty("Sender")]
        public List<Message> MessagesSent {get; set;}
        [InverseProperty("Recipient")]
        public List<Message> MessagesReceived {get; set;}
        public DateTime CreatedAt {get; set;} = DateTime.Now;
        public DateTime UpdatedAt {get; set;} = DateTime.Now;
    }
}