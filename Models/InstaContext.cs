using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Instagram.Models;

public class InstaContext : IdentityDbContext<User, IdentityRole<int>, int>
{
    public InstaContext(DbContextOptions<InstaContext> options) : base(options){}
    
    public DbSet<User> Users { get; set; }
}