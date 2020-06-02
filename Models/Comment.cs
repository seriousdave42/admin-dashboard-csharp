using System;
using System.ComponentModel.DataAnnotations;

namespace UserAdmin.Models
{
    public class Comment
    {
        [Key]
        public int CommentID {get; set;}
        [Required]
        [MinLength(5)]
        public string CommentText {get; set;}
        public User Commenter {get; set;}
        public Message MessageCommented {get; set;}
        public DateTime CreatedAt {get; set;} = DateTime.Now;
        public DateTime UpdatedAt {get; set;} = DateTime.Now;
    }
}