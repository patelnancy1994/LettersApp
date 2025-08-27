using LetterAPI.Services;
using LettersApp.Controllers;
using LettersApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace LetterAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class APILettersController : ControllerBase
    {
        private readonly IHtmlTemplateSetup _engine;
        private readonly SenderSettings _sender;
        private readonly ILogger<APILettersController> _logger;
        private readonly IWebHostEnvironment _env;

        public APILettersController(IHtmlTemplateSetup engine, SenderSettings sender, ILogger<APILettersController> logger, IWebHostEnvironment env)
        {
            _engine = engine;
            _sender = sender;
            _logger = logger;
            _env = env;
        }

        [HttpPost]
        [RequestSizeLimit(1024L * 1024 * 100)]
        public async Task<IActionResult> Generate([FromForm]GenerateLettersRequest req)
        {
            try
            {
                if (req.Csv is null || req.Csv.Length == 0)
                    return BadRequest("CSV file is required.");

                string templateHtml;
                var defaultPath = Path.Combine(_env.WebRootPath ?? "wwwroot", "templates", "SixtyDaysLetterPrompt.html");
                if (!System.IO.File.Exists(defaultPath)) return NotFound("Default template not found.");
                templateHtml = await System.IO.File.ReadAllTextAsync(defaultPath);

                using var csvMs = new MemoryStream();
                await req.Csv.CopyToAsync(csvMs);
                var csvText = Encoding.UTF8.GetString(csvMs.ToArray());
                var rows = CsvUtils.Parse(csvText);

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
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }
    }
}
