using Instagram.Models;

namespace Instagram.ViewModels;

public class UserViewModel
{
    public User? User { get; set; }
    public List<Publication>? Publications { get; set; }
}