using ecommerce_webapp_cs.Models.Entities;

namespace ecommerce_webapp_cs.Models.BlogModels;

public class BlogCreateDto
{
    public int UserId { get; set; }
    public string Title { get; set; }
    public string PostImg { get; set; }
    public string Content { get; set; }

}

