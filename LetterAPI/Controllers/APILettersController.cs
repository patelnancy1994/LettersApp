using CsvHelper;
using LetterAPI.Services;
using LettersApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Globalization;
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
        private readonly IConfiguration _configuration;

        public APILettersController(IHtmlTemplateSetup engine, SenderSettings sender, ILogger<APILettersController> logger, IWebHostEnvironment env, IConfiguration configuration)
        {
            _engine = engine;
            _sender = sender;
            _logger = logger;
            _env = env;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> Generate([FromForm]GenerateLettersRequest req)
        {
            try
            {                
                if (req.Csv is null || req.Csv.Length == 0)
                    return BadRequest("CSV file is required.");

                int FileSizeValid = _configuration.GetValue<int>("FileSize");
                long maxFileSizeInBytes = FileSizeValid * 1024 * 1024;
                if (req.Csv.Length > maxFileSizeInBytes)
                    return BadRequest($"File size is exceeds {FileSizeValid} mb limit.");

                string templateHtml;
                var defaultPath = Path.Combine(_env.WebRootPath ?? "wwwroot", "templates", "SixtyDaysLetterPrompt.html");
                if (!System.IO.File.Exists(defaultPath)) return NotFound("Default template not found.");
                templateHtml = await System.IO.File.ReadAllTextAsync(defaultPath);

                using var csvMs = req.Csv.OpenReadStream();
                var sb = new StringBuilder();
                using (var reader = new StreamReader(csvMs))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var rows = new List<Dictionary<string, string>>();
                    csv.Read();
                    csv.ReadHeader();
                    var headers = csv.HeaderRecord;

                    while (csv.Read())
                    {
                        var dict = new Dictionary<string, string>();
                        foreach (var header in headers)
                        {
                            dict[header] = csv.GetField(header);
                        }
                        rows.Add(dict);
                    }           
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
                }
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
