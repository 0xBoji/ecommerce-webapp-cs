using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ecommerce_webapp_cs.Models.Entities;
using ecommerce_webapp_cs.Models.BlogModels;
using System.Threading.Tasks;
using System.Linq;

namespace ecommerce_webapp_cs.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class blogsController : ControllerBase
{
    private readonly ArtsContext _context;

    public blogsController(ArtsContext context)
    {
        _context = context;
    }

    // get all blog posts
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BlogPost>>> GetBlogPosts()
    {
        return await _context.BlogPosts
                             .Include(bp => bp.BlogComments)
                             .OrderByDescending(bp => bp.PostedDate) // ensure posts are ordered by PostedDate
                             .ToListAsync();
    }


    // Get a single blog post by id
    [HttpGet("{id}")]
    public async Task<ActionResult<BlogPost>> GetBlogPost(int id)
    {
        var blogPost = await _context.BlogPosts.Include(bp => bp.BlogComments).FirstOrDefaultAsync(bp => bp.PostId == id);

        if (blogPost == null)
        {
            return NotFound();
        }

        return blogPost;
    }

    [HttpPost]
    public async Task<ActionResult<BlogPost>> PostBlogPost([FromForm] BlogCreateDto blogCreateDto)
    {
        var userIdString = HttpContext.Session.GetString("UserID");
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
        {
            return Unauthorized(new { message = "User is not authenticated" });
        }

        string postImgPath = null;
        if (blogCreateDto.PostImg != null)
        {
            try
            {
                postImgPath = await SaveBlogImage(blogCreateDto.PostImg);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while processing the image: {ex.Message}");
            }
        }

        var blogPost = new BlogPost
        {
            UserId = userId, // Use the UserId from the session
            Title = blogCreateDto.Title,
            PostImg = postImgPath,
            Content = blogCreateDto.Content,
            PostedDate = DateTime.UtcNow // Setting the PostedDate to the current time
        };

        _context.BlogPosts.Add(blogPost);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetBlogPost), new { id = blogPost.PostId }, blogPost);
    }


    private async Task<string> SaveBlogImage(IFormFile imageFile)
    {
        if (imageFile != null && imageFile.Length > 0)
        {
            // check if the file is an image
            if (!imageFile.ContentType.StartsWith("image/"))
            {
                throw new ArgumentException("Only image files are allowed.");
            }

            // check if the image size is less than 5MB
            if (imageFile.Length > 5 * 1024 * 1024)
            {
                throw new ArgumentException("Image size cannot exceed 5MB.");
            }

            var imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "BlogImages");

            if (!Directory.Exists(imagesPath))
            {
                Directory.CreateDirectory(imagesPath);
            }

            var fileName = Path.GetFileNameWithoutExtension(imageFile.FileName);
            var extension = Path.GetExtension(imageFile.FileName);
            var newFileName = $"{Guid.NewGuid()}{extension}"; // generate a new file name to prevent overwriting
            var filePath = Path.Combine(imagesPath, newFileName);

            try
            {
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                return newFileName; // returning the new file name or a relative path as needed
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while saving the image file.", ex);
            }
        }

        return null; // return null if no image was provided or if it didn't pass the checks
    }


    // Update an existing blog post
    [HttpPut("{id}")]
    public async Task<IActionResult> PutBlogPost(int id, BlogPost blogPost)
    {
        if (id != blogPost.PostId)
        {
            return BadRequest();
        }

        _context.Entry(blogPost).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!BlogPostExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // Delete a blog post
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBlogPost(int id)
    {
        var blogPost = await _context.BlogPosts.FindAsync(id);
        if (blogPost == null)
        {
            return NotFound();
        }

        _context.BlogPosts.Remove(blogPost);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{postId}/comments")]
    public async Task<ActionResult<BlogComment>> PostComment(int postId, [FromBody] CommentCreateDto commentDto)
    {
        var userIdString = HttpContext.Session.GetString("UserID");
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
        {
            return Unauthorized(new { message = "User is not authenticated" });
        }
        if (!BlogPostExists(postId))
        {
            return NotFound("Blog post not found.");
        }

        var comment = new BlogComment
        {
            PostId = postId,
            UserId = userId, 
            CommentText = commentDto.CommentText,
            CommentDate = DateTime.UtcNow 
        };

        _context.BlogComments.Add(comment);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetComment), new { id = comment.CommentId }, comment);
    }


    // Get a single comment
    [HttpGet("comments/{id}")]
    public async Task<ActionResult<BlogComment>> GetComment(int id)
    {
        var comment = await _context.BlogComments.FindAsync(id);

        if (comment == null)
        {
            return NotFound();
        }

        return comment;
    }

    private bool BlogPostExists(int id)
    {
        return _context.BlogPosts.Any(e => e.PostId == id);
    }

}
