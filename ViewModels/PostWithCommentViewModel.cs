using Instagram.Models;

namespace Instagram.ViewModels;

public class PostWithCommentViewModel
{
    public List<Comment>? Comments { get; set; }
    public Publication? Publication { get; set; }
}