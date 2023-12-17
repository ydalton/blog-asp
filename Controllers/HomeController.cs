﻿using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using blog.Models;
using Contentful.Core;
using Contentful.Core.Configuration;
using Contentful.Core.Models;
using Contentful.Core.Errors;

namespace blog.Controllers;

public class HomeController : Controller
{
    // Read the environment variables for the API key instead of hardcoding
    // them in the code
    private readonly ILogger<HomeController> _logger;
    private readonly IContentfulClient _client;
    private readonly IContentfulManagementClient _mgmtClient;
    // create a Contentful client
    private readonly ContentfulOptions options;
    private readonly HttpClient _httpClient;

    public HomeController(ILogger<HomeController> logger)
    {
        options = new ContentfulOptions {
            DeliveryApiKey = Environment.GetEnvironmentVariable("DELIVERY_KEY"),
            PreviewApiKey = Environment.GetEnvironmentVariable("PREVIEW_KEY"),
            SpaceId = Environment.GetEnvironmentVariable("SPACE_ID"),
            ManagementApiKey = Environment.GetEnvironmentVariable("MGMT_KEY")
        };

        _httpClient = new HttpClient();
        _client = new ContentfulClient(_httpClient, options);
        _mgmtClient = new ContentfulManagementClient(_httpClient, options);
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

    public IActionResult NewPage()
    {
        return View();
    }

    public async Task<IActionResult> SubmitPost()
    {
        string title = Request.Form["title"];
        string content = Request.Form["content"];
        Console.WriteLine($"Title: {title}; Contents: {content}");
        // var newPost = new Post();
        // newPost.Title = title;
        // newPost.Content = content;
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
        try {
            var newEntry = await _mgmtClient.CreateEntry(newPost, "post");
            await _mgmtClient.PublishEntry(newEntry.SystemProperties.Id,
                                        newEntry.SystemProperties.Version ?? 0);
        } catch (ContentfulException err) {
            Console.WriteLine("Could not create entry.");
        }
        return View();
    }
    public async Task<IActionResult> Post()
    {
        string postId = Request.Query["id"];
        Post entry;
        try{
            entry = await _client.GetEntry<Post>(postId);
        } catch (ContentfulException err) {
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



    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
