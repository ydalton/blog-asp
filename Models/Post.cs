using Contentful.Core.Models;

namespace blog.Models;

public class Post {
    // Store Contentful entry ID, date when created, etc...
    public SystemProperties? Sys { get; set; }
    // The title of the post
    public string? Title {get; set;}
    // The contents of the post
    public string? Content {get; set;}
}
