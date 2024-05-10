using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Instagram.Models;

public class Publication
{
    public int Id { get; set; }
    public string? Avatar { get; set; }
    [NotMapped]
    public IFormFile? ImageFile { get; set; }
    [Required]
    public string Description { get; set; }
    [BindNever]
    public ICollection<Like>? Likes { get; set; }
    [BindNever]
    public ICollection<Comment>? Comments { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
}