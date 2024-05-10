using Instagram.Models;
using Instagram.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Instagram.Controllers;

public class UserController : Controller
{
    public InstaContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IWebHostEnvironment _hostEnvironment;
    private readonly SignInManager<User> _signInManager;

    public UserController(InstaContext context, UserManager<User> userManager, IWebHostEnvironment hostEnvironment, SignInManager<User> signInManager)
    {
        _context = context;
        _userManager = userManager;
        _hostEnvironment = hostEnvironment;
        _signInManager = signInManager;
    }
    
    [Authorize]
    public IActionResult Index()
    {
        int? userId = Convert.ToInt32(_userManager.GetUserId(User));
        var user = _context.Users.Include(ff => ff.Followers).Include(ss => ss.Subscriptions).FirstOrDefault(u => u.Id == userId);
        var posts = _context.Publications.Include(p => p.Likes).Include(p => p.Comments).Where(p => p.UserId == userId).ToList();
        UserViewModel userViewModel = new UserViewModel()
        {
            User = user,
            Publications = posts
        };
        ViewBag.TargetUserId = userId;
        return View(userViewModel);
    }
    [Authorize]
    public IActionResult Search(string searchResult)
    {
        int? userId = Convert.ToInt32(_userManager.GetUserId(User));
        var users = _context.Users
            .Where(u => u.UserName.Contains(searchResult) || u.Email.Contains(searchResult) || u.NickName.Contains(searchResult) || u.Description.Contains(searchResult))
            .ToList();
        users = users.Where(u => u.Id != userId && !_userManager.IsInRoleAsync(u, "admin").Result).ToList();

        return View(users);
    }
    [Authorize]
    public async Task<IActionResult> Edit()
    {
        int? userId = Convert.ToInt32(_userManager.GetUserId(User));
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound();
        }
        return PartialView("/Views/User/_EditProfilePartialView.cshtml", user);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(User user)
    {
        User identityUser = await _userManager.FindByEmailAsync(user.Email);
        if (identityUser != null)
        {
            
            if (ModelState.IsValid)
            {
                if (user.ImageFile != null && user.ImageFile.Length > 0)
                {
                    
                    var uploadPath = Path.Combine(_hostEnvironment.WebRootPath, "images");
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(user.ImageFile.FileName);
                    var fullPath = Path.Combine(uploadPath, fileName);
                    
                    using (var fileStream = new FileStream(fullPath, FileMode.Create))
                    {
                        await user.ImageFile.CopyToAsync(fileStream);
                    }
                    
                    user.Avatar = "/images/" + fileName;
                }

                identityUser.NickName = user.NickName;
                identityUser.Description = user.Description;
                identityUser.Avatar = user.Avatar;
                identityUser.Gender = user.Gender;
                identityUser.PhoneNumber = user.PhoneNumber;
                var result = await _userManager.UpdateAsync(identityUser);
                if (result.Succeeded)
                {
                   
                    return RedirectToAction("Index", "User");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                    
            }
        }
        

        return PartialView("/Views/User/_EditProfilePartialView.cshtml", user);
    }
    [Authorize]
    public IActionResult Profile(int id)
    {
        int? targetUserId = Convert.ToInt32(_userManager.GetUserId(User));
        var res = _context.UserSubscriptions.FirstOrDefault(us =>
            us.TargetUserId == targetUserId && us.SubscriberId == id);
        if (res != null)
        {
            ViewBag.IsSubscribed = true;
        }
        else
        {
            ViewBag.IsSubscribed = false;

        }
        var user = _context.Users.Include(ff => ff.Followers).Include(ss => ss.Subscriptions).FirstOrDefault(u => u.Id == id);
        if (user.Id == targetUserId)
        {
            return RedirectToAction("Index");
        }
        var posts = _context.Publications.Include(p => p.Likes).Include(p => p.Comments).Where(p => p.UserId == id).ToList();
        UserViewModel userViewModel = new UserViewModel()
        {
            User = user,
            Publications = posts
        };
        return View(userViewModel);
    }
    [Authorize]
    public IActionResult Subcribe(int userId)
    {
        int? targetUserId = Convert.ToInt32(_userManager.GetUserId(User));
        if (userId == targetUserId)
        {
            return NotFound();
        }
        UserSubscription userSubscription = new UserSubscription()
        {
            SubscriberId = userId,
            TargetUserId = targetUserId.Value
        };
        _context.UserSubscriptions.Add(userSubscription);
        _context.SaveChanges();
        return RedirectToAction("Profile", "User", new { id = userId });
    }
    [Authorize]
    public async Task<IActionResult> Describe(int userId)
    {
        int? targetUserId = Convert.ToInt32(_userManager.GetUserId(User));
        if (userId == targetUserId)
        {
            return NotFound();
        }
        UserSubscription? us = await _context.UserSubscriptions.FirstOrDefaultAsync(us =>
            us.TargetUserId == targetUserId && us.SubscriberId == userId);
        _context.Entry(us).State = EntityState.Deleted;
        await _context.SaveChangesAsync();
        return RedirectToAction("Profile", "User", new { id = userId });
    }
}