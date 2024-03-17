using System.ComponentModel.DataAnnotations;

namespace Rocky_Models.Models
{
    public class Like
    {
        [Key]
        public int Id { get; set; }

        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        public int PostId { get; set; }
        public Post Post { get; set; }

    }
}
