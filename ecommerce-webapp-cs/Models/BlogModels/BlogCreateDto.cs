using ecommerce_webapp_cs.Models.Entities;

namespace ecommerce_webapp_cs.Models.BlogModels;

public class BlogCreateDto
{
    public string Title { get; set; }
    public IFormFile PostImg { get; set; }
    public string Content { get; set; }

}

