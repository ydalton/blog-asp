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
    private readonly bool _canEdit;

    // Constructor for the home controller
    public HomeController(ILogger<HomeController> logger)
    {
        // Read the environment variables for the API keys instead of hard
        // coding them in the code
        var options = new ContentfulOptions {
            DeliveryApiKey = Environment.GetEnvironmentVariable("DELIVERY_KEY"),
            // The preview key is not required?
            //PreviewApiKey = Environment.GetEnvironmentVariable("PREVIEW_KEY"),
            SpaceId = Environment.GetEnvironmentVariable("SPACE_ID"),
            ManagementApiKey = Environment.GetEnvironmentVariable("MGMT_KEY")
        };
        // create HTTP client, needed for the Contentful client(s)
        var httpClient = new HttpClient();
        // create a Contentful client
        _client = new ContentfulClient(httpClient, options);
        // _canEdit means we can add a post. Only a person with the Management
        // API key should be allowed to do so.
        _canEdit = options.ManagementApiKey != null;
        if(_canEdit)
            _mgmtClient = new ContentfulManagementClient(httpClient, options);
        _logger = logger;
    }

    // Index page
    public async Task<IActionResult> Index()
    {
        ContentfulCollection<Post> entries;
        // try to get entries from Contentful
        try
        {
            entries = await _client.GetEntriesByType<Post>("post");
        }
        catch
        {
            // Throw error
            ViewData["ErrorMsg"] = "Could not get entries.";
            return View("Error");
        }
        // export them to the view
        ViewData["Entries"] = entries;
        ViewData["canEdit"] = _canEdit;
        return View();
    }
    

    // Page to add a new entry to Contentful
    public IActionResult NewPage()
    {
        // Check permissions. Return an error is we can't edit
        if (!_canEdit)
        {
            ViewData["ErrorMsg"] = 
                "You do not have the permissions to access this page.";
            return View("Error");
        }
        return View();
    }

    // Page that actually adds the the entry to Contentful
    public async Task<IActionResult> SubmitPost()
    {
        // Check permissions. Return an error is we can't edit
        if (!_canEdit)
        {
            ViewData["ErrorMsg"] = 
                "You do not have the permissions to access this page.";
            return View("Error");
        }
        // Get parameters from request
        string? title = Request.Form["title"];
        string? content = Request.Form["content"];
        // We don't want to add a null entry
        if (title == null || content == null)
        {
            ViewData["ErrorMsg"] = 
                "Didn't specify either post title or contents.";
            return View("Error");
        }
        Console.WriteLine($"Adding new entry with contents title \"{title}\" " +
                          $"and contents \"{content}\"");
        // Describe new post
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
    // View a single post
    public async Task<IActionResult> Post()
    {
        string? postId = Request.Query["id"];
        Post entry;
        try{
            entry = await _client.GetEntry<Post>(postId);
        } catch {
            // Return an error if the requested post cannot be found.
            ViewData["ErrorMsg"] = "The requested post could not be found.";
            return View("Error");
        }
        // export them to the view
        ViewData["entry"] = entry;
        ViewData["canEdit"] = _canEdit;
        return View();
    }

    // Return about page
    public IActionResult About()
    {
        ViewData["canEdit"] = _canEdit;
        return View();
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
