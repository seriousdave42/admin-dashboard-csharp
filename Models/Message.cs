using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;

namespace UserAdmin.Models
{
    public class Message
    {
        [Key]
        public int MessageID {get; set;}
        [Required]
        [MinLength(5)]
        public string MessageText {get; set;}
        public User Sender {get; set;}
        public User Recipient {get; set;}
        public List<Comment> MessageComments {get; set;}
        public DateTime CreatedAt {get; set;} = DateTime.Now;
        public DateTime UpdatedAt {get; set;} = DateTime.Now;
    }
}