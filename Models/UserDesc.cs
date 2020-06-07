using System.ComponentModel.DataAnnotations;
namespace UserAdmin.Models
{
    public class UserDesc
    {
        [MaxLength(50)]
        public string Description {get; set;}
    }
}