using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Instagram.Models;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Не указан Email")]
    [DataType(DataType.EmailAddress)]
    [EmailAddress(ErrorMessage = "Некорректный Email")]
    public string Email { get; set; }
    
    [Required(ErrorMessage = "Не указан UserName")]
    public string UserName { get; set; }
    
    [Required(ErrorMessage = "Не указан пароль")]
    [MinLength(3, ErrorMessage = "Пароль должен содержать не менее 8 символов")]
    /*[RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$", ErrorMessage = "Пароль должен содержать минимум 1 цифру, больше 8 символов и одну букву в верхнем регистре")]*/
    [DataType(DataType.Password)]
    public string Password { get; set; }
    
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Пароли не совпадают")]
    public string ConfirmPassword { get; set; }
    
    public string? Avatar { get; set; }
    [NotMapped]
    public IFormFile? ImageFile { get; set; }
    public string? NickName { get; set; }
    public string? Description { get; set; }
    public string? ContactPhone { get; set; }
    public string? Gender { get; set; }
}