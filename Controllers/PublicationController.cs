using Instagram.Models;
using Instagram.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Instagram.Controllers;

public class PublicationController : Controller
{
    public InstaContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IWebHostEnvironment _hostEnvironment;
    private IHttpContextAccessor _httpContextAccessor;
    
    public PublicationController(InstaContext context, UserManager<User> userManager, IWebHostEnvironment hostEnvironment, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _userManager = userManager;
        _hostEnvironment = hostEnvironment;
        _httpContextAccessor = httpContextAccessor;
    }
    [Authorize]
    public IActionResult Index()
    {
        int? userId = Convert.ToInt32(_userManager.GetUserId(User));
        var users = _context.UserSubscriptions.Where(u => u.TargetUserId == userId).Select(u => u.SubscriberId).ToList();
        var subscribedUsers= _context.Users.Where(u => users.Contains(u.Id)).ToList();
        List<Publication> posts = new List<Publication>();
        foreach (var user in subscribedUsers)
        {
            var userPosts = _context.Publications
                .Include(l => l.Likes)
                .Include(c => c.Comments)
                .Include(u => u.User)
                .Where(p => p.UserId == user.Id)
                .ToList();
            posts.AddRange(userPosts);
        }
        ViewBag.TargetUserId = userId;
        posts = posts.OrderByDescending(p => p.Id).ToList();
        return View(posts);
    }
    [Authorize]
    public IActionResult Create()
    {
        int? userId = Convert.ToInt32(_userManager.GetUserId(User));
        ViewBag.TargetUserId = userId;
        return PartialView("/Views/User/_CreatePublicationPartialView.cshtml");
    }

    [HttpPost]
    public async Task<IActionResult> Create(Publication publication)
    {
        if (ModelState.IsValid)
        {
            if (publication.ImageFile != null && publication.ImageFile.Length > 0)
            {
                
                var uploadPath = Path.Combine(_hostEnvironment.WebRootPath, "images");
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(publication.ImageFile.FileName);
                var fullPath = Path.Combine(uploadPath, fileName);
            
                using (var fileStream = new FileStream(fullPath, FileMode.Create))
                {
                    await publication.ImageFile.CopyToAsync(fileStream);
                }
            
                publication.Avatar = "/images/" + fileName;
                await _context.Publications.AddAsync(publication);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Home");// Change to user
            }
        }
        int? userId = Convert.ToInt32(_userManager.GetUserId(User));
        ViewBag.TargetUserId = userId;
        return PartialView("/Views/User/_CreatePublicationPartialView.cshtml", publication);
    }
    [Authorize]
    public IActionResult CreateComment(int postId)
    {
        var userPost = _context.Publications
            .Include(l => l.Likes)
            .Include(c => c.Comments)
            .Include(u => u.User)
            .FirstOrDefault(p => p.Id == postId);
        var comments = _context.Comments
            .Include(c => c.User)
            .Include(c => c.Publication)
            .Where(c => c.PublicationId == userPost.Id)
            .ToList();
        comments = comments.OrderBy(c => c.Id).ToList();
        PostWithCommentViewModel postWithCommentViewModel = new PostWithCommentViewModel()
        {
            Comments = comments,
            Publication = userPost
        };
        return View(postWithCommentViewModel);
    }

    [HttpPost]
    public IActionResult CreateComment(int postId, string text)
    {
        int? userId = Convert.ToInt32(_userManager.GetUserId(User));
        var post = _context.Publications.FirstOrDefault(p => p.Id == postId && p.UserId == userId);
        
        Comment comment = new Comment()
        {
            Text = text,
            UserId = userId.Value,
            PublicationId = postId
        };
        _context.Comments.Add(comment);
        _context.SaveChanges();
        return RedirectToAction("CreateComment", "Publication", new {postId = postId});
    }
    [Authorize]
    public IActionResult CreateLike(int postId, string? returnUrl)
    {
        var referrer = _httpContextAccessor.HttpContext.Request.Headers["Referer"].ToString();
        var userPost = _context.Publications.FirstOrDefault(p => p.Id == postId);
        int? userId = Convert.ToInt32(_userManager.GetUserId(User));
        if (_context.Likes.FirstOrDefault(l => l.UserId == userId && l.PublicationId == userPost.Id) == null)
        {
            Like like = new Like()
            {
                UserId = userId.Value,
                PublicationId = userPost.Id
            };
            _context.Likes.Add(like);
            _context.SaveChanges();
        }
        else
        {
            var like = _context.Likes.FirstOrDefault(l => l.UserId == userId && l.PublicationId == userPost.Id);
            _context.Entry(like).State = EntityState.Deleted;
            _context.SaveChanges();
        }
        
        return Redirect(referrer);
    }
}