using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Instagram.Models;

public class User : IdentityUser<int>
{
    public string? Avatar { get; set; }
    [Required]
    public string? NickName { get; set; }
    public string? Description { get; set; }
    public string? ContactPhone { get; set; }
    public string? Gender { get; set; }
    [NotMapped]
    public IFormFile? ImageFile { get; set; }
}