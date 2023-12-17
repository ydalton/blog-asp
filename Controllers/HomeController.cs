using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using blog.Models;
using Contentful.Core;
using Contentful.Core.Configuration;
using Contentful.Core.Models;

namespace blog.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    // The client which will fetch Contentful entries
    private readonly IContentfulClient _client;
    // The client which will add Contentful entries
    private readonly IContentfulManagementClient? _mgmtClient;
    private bool _canEdit;

    public HomeController(ILogger<HomeController> logger)
    {
        // Read the environment variables for the API keys instead of hardcoding
        // them in the code
        var options = new ContentfulOptions {
            DeliveryApiKey = Environment.GetEnvironmentVariable("DELIVERY_KEY"),
            // The preview key is not required?
            //PreviewApiKey = Environment.GetEnvironmentVariable("PREVIEW_KEY"),
            SpaceId = Environment.GetEnvironmentVariable("SPACE_ID"),
            ManagementApiKey = Environment.GetEnvironmentVariable("MGMT_KEY")
        };
        var httpClient = new HttpClient();
        // create a Contentful client
        _client = new ContentfulClient(httpClient, options);
        _canEdit = options.ManagementApiKey != null;
        if(_canEdit)
            _mgmtClient = new ContentfulManagementClient(httpClient, options);
        _logger = logger;
    }

    // Index page
    public async Task<IActionResult> Index()
    {
        // get entries from Contentful
        ContentfulCollection<Post> entries;
        try
        {
            entries = await _client.GetEntriesByType<Post>("post");
        }
        catch
        {
            return Error();
        }
        // export them to the view
        ViewData["Entries"] = entries;
        ViewData["canEdit"] = _canEdit;
        return View();
    }

    // Page to add a new entry to Contentful
    public IActionResult NewPage()
    {
        return View();
    }

    // Page that actually adds the the entry to Contentful
    public async Task<IActionResult> SubmitPost()
    {
        if (!_canEdit)
            return Error();
        string? title = Request.Form["title"];
        string? content = Request.Form["content"];
        // We don't want to add a null entry
        if (title == null || content == null)
            return View();
        Console.WriteLine($"Adding new entry with contents title \"{title}\" " +
                          $"and contents \"{content}\"");
        var newPost = new Entry<dynamic>
        {
            Fields = new
            {
                Title = new Dictionary<string, string>()
                {
                    {"en-US", title}
                },
                Content = new Dictionary<string, string>()
                {
                    {"en-US", content}
                }
            }
        };
        // Attempt to create and publish a new entry to Contentful
        try {
            Debug.Assert(_mgmtClient != null);
            var newEntry = await _mgmtClient.CreateEntry(newPost, "post");
            await _mgmtClient.PublishEntry(newEntry.SystemProperties.Id,
                                        newEntry.SystemProperties.Version ?? 0);
        } catch {
            Console.WriteLine("Could not create entry.");
        }
        return View();
    }
    public async Task<IActionResult> Post()
    {
        string? postId = Request.Query["id"];
        Post entry;
        try{
            entry = await _client.GetEntry<Post>(postId);
        } catch {
            // Create a post with an entry stating that the requested url could
            // not be found.
            entry = new Post();
            entry.Title = "Post not found";
            entry.Content = "The requested post could not be found.";
        }
        // export them to the view
        ViewData["entry"] = entry;
        return View();
    }

    public IActionResult About()
    {
        return View();
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
