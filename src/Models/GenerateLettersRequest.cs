using Microsoft.AspNetCore.Http;

namespace LettersApp.Models;

public sealed class GenerateLettersRequest
{
    public IFormFile? Csv { get; set; }
    //public IFormFile? TemplateHtml { get; set; } // optional, default template used if null
    public string? DateFormat { get; set; } // optional: parse/format dates
}
