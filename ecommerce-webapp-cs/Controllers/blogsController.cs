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

    // Get all blog posts
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BlogPost>>> GetBlogPosts()
    {
        return await _context.BlogPosts.Include(bp => bp.BlogComments).ToListAsync();
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
    public async Task<ActionResult<BlogPost>> PostBlogPost([FromBody] BlogCreateDto blogCreateDto)
    {

        var blogPost = new BlogPost
        {
            UserId = blogCreateDto.UserId, // Use the UserId from the session
            Title = blogCreateDto.Title,
            PostImg = blogCreateDto.PostImg,
            Content = blogCreateDto.Content,
            PostedDate = DateTime.UtcNow // Setting the PostedDate to the current time
        };

        _context.BlogPosts.Add(blogPost);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetBlogPost), new { id = blogPost.PostId }, blogPost);
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
        if (!BlogPostExists(postId))
        {
            return NotFound("Blog post not found.");
        }

        var comment = new BlogComment
        {
            PostId = postId,
            UserId = commentDto.UserId, 
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
