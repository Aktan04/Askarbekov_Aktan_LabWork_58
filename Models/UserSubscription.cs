namespace Instagram.Models;

public class UserSubscription
{
    public int Id { get; set; }
    public int SubscriberId { get; set; } 
    public User? Subscriber { get; set; } 

    public int TargetUserId { get; set; } 
    public User? TargetUser { get; set; } 
}