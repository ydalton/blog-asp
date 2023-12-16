using Contentful.Core.Models;

namespace blog.Models;

public class Post {
    public SystemProperties? Sys { get; set; }
    public string? Title {get; set;}
    public string? Content {get; set;}
}
