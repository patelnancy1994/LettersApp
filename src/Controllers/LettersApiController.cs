using System.Text;
using LettersApp.Models;
using LettersApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace LettersApp.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public sealed class LettersApiController : ControllerBase
{
    private readonly IHtmlTemplateSetup _engine;
    private readonly SenderSettings _sender;
    private readonly IWebHostEnvironment _env;

    public LettersApiController(IHtmlTemplateSetup engine, IOptions<SenderSettings> sender, IWebHostEnvironment env)
    {
        _engine = engine;
        _sender = sender.Value;
        _env = env;
    }

    [HttpPost]
    [RequestSizeLimit(1024L * 1024 * 100)]
    public async Task<IActionResult> Generate([FromForm] GenerateLettersRequest req)
    {
        if (req.Csv is null || req.Csv.Length == 0)
            return BadRequest("CSV file is required.");

        string templateHtml;
        //if (req.TemplateHtml is not null && req.TemplateHtml.Length > 0)
        //{
        //    using var ms = new MemoryStream();
        //    await req.TemplateHtml.CopyToAsync(ms);
        //    templateHtml = Encoding.UTF8.GetString(ms.ToArray());
        //}
        //else
        {
            var defaultPath = Path.Combine(_env.WebRootPath ?? "wwwroot", "templates", "SixtyDaysLetterPrompt.html");
            if (!System.IO.File.Exists(defaultPath)) return NotFound("Default template not found.");
            templateHtml = await System.IO.File.ReadAllTextAsync(defaultPath);
        }

        // Read CSV text
        using var csvMs = new MemoryStream();
        await req.Csv.CopyToAsync(csvMs);
        var csvText = Encoding.UTF8.GetString(csvMs.ToArray());

        var rows = CsvUtils.Parse(csvText);

        // Build per-page letters
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html><html><head><meta charset=\"utf-8\" />");
        sb.AppendLine("<title>Batch Letters</title>");
        sb.AppendLine("<style>@media print {.page { page-break-after: always; }} .page{margin:0;padding:0;}</style>");
        sb.AppendLine("</head><body>");

        foreach (var row in rows)
        {
            row["MyCompanyPhoneNumber"] = _sender.PhoneNumber;
            var pageHtml = _engine.Apply(templateHtml, row);
            sb.AppendLine("<section class=\"page\">");
            sb.AppendLine(pageHtml);
            sb.AppendLine("</section>");
        }

        sb.AppendLine("</body></html>");
        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        var fileName = $"letters_{DateTime.UtcNow:yyyyMMdd_HHmmss}.html";
        return File(bytes, "text/html", fileName);
    }
}
