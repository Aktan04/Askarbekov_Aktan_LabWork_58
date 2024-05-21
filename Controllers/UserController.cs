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
    [HttpPost]
    public async Task<IActionResult> DeletePost(int postId)
    {
        var post = await _context.Publications.FindAsync(postId);
        if (post == null || post.UserId != Convert.ToInt32(_userManager.GetUserId(User)))
        {
            return Json(new { success = false, error = "Post not found or user unauthorized." });
        }

        _context.Publications.Remove(post);
        await _context.SaveChangesAsync();
        int targetUserId = Convert.ToInt32(_userManager.GetUserId(User));
        var posts = _context.Publications.Where(p => p.UserId == targetUserId).ToList();
        var postsCounter = posts.Count;
        return Json(new { success = true, postCount = postsCounter });
    }
    
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> EditPostDescription(int postId, string description)
    {
        var post = await _context.Publications.FindAsync(postId);
        if (post == null || post.UserId != Convert.ToInt32(_userManager.GetUserId(User)))
        {
            return Json(new { success = false, error = "Post not found or user unauthorized." });
        }

        post.Description = description;
        _context.Publications.Update(post);
        await _context.SaveChangesAsync();
        return Json(new { success = true });
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
    [HttpPost]
    public async Task<IActionResult> ToggleSubscription(int userId)
    {
        int? currentUserId = Convert.ToInt32(_userManager.GetUserId(User));
        if (userId == currentUserId)
        {
            return BadRequest("You cannot subscribe to yourself.");
        }

        var subscription = await _context.UserSubscriptions
            .FirstOrDefaultAsync(us => us.SubscriberId == userId && us.TargetUserId == currentUserId);
        bool isSubscribed;
        if (subscription == null)
        {
            UserSubscription newSubscription = new UserSubscription()
            {
                SubscriberId = userId,
                TargetUserId = currentUserId.Value
            };
            _context.UserSubscriptions.Add(newSubscription);
            isSubscribed = true;
        }
        else
        {
            _context.UserSubscriptions.Remove(subscription);
            isSubscribed = false;
        }
        await _context.SaveChangesAsync();
        int followingCount = await _context.UserSubscriptions.CountAsync(us => us.SubscriberId == userId);

        return Json(new 
        { 
            isSubscribed = isSubscribed, 
            followingCount = followingCount 
        });
    }
}