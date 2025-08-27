using LettersApp.Models;
using Microsoft.AspNetCore.Mvc;
namespace LettersApp.Controllers;
public class UploadController : Controller
{
    private readonly ILogger<UploadController> _logger;
    private readonly ApiSettings _apiSettings;
    private readonly IWebHostEnvironment _env;

    public UploadController(ILogger<UploadController> logger, IWebHostEnvironment env, ApiSettings apiSettings)
    {
        _logger = logger;
        _env = env;
        _apiSettings = apiSettings;
    }

    [HttpGet]
    public IActionResult Index() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Generate(IFormFile csv)
    {
        try
        {
            if (csv is null || csv.Length == 0)
            {
                TempData["Error"] = "Please upload a CSV file.";
                return RedirectToAction(nameof(Index));
            }

            var http = new HttpClient();
            var content = new MultipartFormDataContent();

            var csvStream = csv.OpenReadStream();
            content.Add(new StreamContent(csvStream), "Csv", csv.FileName);

            var baseUrl = _apiSettings.BaseUrl;
            var resp = await http.PostAsync($"{baseUrl}/api/APILetters/Generate", content);

            if (!resp.IsSuccessStatusCode)
            {
                TempData["Error"] = $"API error: {resp.StatusCode}";
                return RedirectToAction(nameof(Index));
            }

            var bytes = await resp.Content.ReadAsByteArrayAsync();
            var fileName = resp.Content.Headers.ContentDisposition?.FileName?.Trim('\"') ?? "letters.html";
            return File(bytes, "text/html", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            throw;
        }
    }
}
