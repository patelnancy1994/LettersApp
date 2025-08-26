using Microsoft.AspNetCore.Mvc;

namespace LettersApp.Controllers;

public sealed class UploadController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<UploadController> _logger;
    private readonly IWebHostEnvironment _env;

    public UploadController(IHttpClientFactory? httpClientFactory, ILogger<UploadController> logger, IWebHostEnvironment env)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory), "Add services.AddHttpClient() if needed.");
        _logger = logger;
        _env = env;
    }

    [HttpGet]
    public IActionResult Index() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Generate(IFormFile csv, IFormFile? templateHtml)
    {
        if (csv is null || csv.Length == 0)
        {
            TempData["Error"] = "Please upload a CSV file.";
            return RedirectToAction(nameof(Index));
        }

        using var http = new HttpClient();
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        using var content = new MultipartFormDataContent();

        var csvStream = csv.OpenReadStream();
        content.Add(new StreamContent(csvStream), "Csv", csv.FileName);

        if (templateHtml is not null && templateHtml.Length > 0)
        {
            var tplStream = templateHtml.OpenReadStream();
            content.Add(new StreamContent(tplStream), "TemplateHtml", templateHtml.FileName);
        }

        var resp = await http.PostAsync($"{baseUrl}/api/LettersApi/Generate", content);
        if (!resp.IsSuccessStatusCode)
        {
            TempData["Error"] = $"API error: {resp.StatusCode}";
            return RedirectToAction(nameof(Index));
        }

        var bytes = await resp.Content.ReadAsByteArrayAsync();
        var fileName = resp.Content.Headers.ContentDisposition?.FileName?.Trim('\"') ?? "letters.html";
        return File(bytes, "text/html", fileName);
    }
}
