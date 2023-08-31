using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBlog.Models
{
    public class Category
    {
        //Primary Key
        public int Id { get; set; }

        [Required]
        [Display(Name = "Category Name")]
        [StringLength(30, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        public string? Name { get; set; }

        [StringLength(200, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        public string? Description { get; set; }

        public byte[]? ImageData { get; set; }

        public string? ImageType { get; set; }

        [NotMapped]
        public IFormFile? ImageFile { get; set; }

        public ICollection<BlogPost> BlogPosts { get; set; } = new HashSet<BlogPost>();
    }
}