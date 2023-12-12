using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using blog.Models;
using Contentful.Core;
using Contentful.Core.Configuration;

namespace blog.Controllers;

public class HomeController : Controller
{
    // Read the environment variables for the API key instead of hardcoding
    // them in the code
    private readonly ILogger<HomeController> _logger;
    private readonly IContentfulClient _client;
    // create a Contentful client
    private readonly ContentfulOptions options = new ContentfulOptions {
        DeliveryApiKey = Environment.GetEnvironmentVariable("DELIVERY_KEY"),
        PreviewApiKey = Environment.GetEnvironmentVariable("PREVIEW_KEY"),
        SpaceId = Environment.GetEnvironmentVariable("SPACE_ID")
    };

    public HomeController(ILogger<HomeController> logger)
    {
        _client = new ContentfulClient(new HttpClient(), options)
            ?? throw new NullReferenceException("what");
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        // get entries from Contentful
        var entries = await _client.GetEntriesByType<Post>("post");
        if(entries == null)
            throw new Exception("No entries");
        // export them to the view
        ViewData["Entries"] = entries;
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
