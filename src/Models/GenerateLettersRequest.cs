using Microsoft.AspNetCore.Http;

namespace LettersApp.Models;

public sealed class GenerateLettersRequest
{
    public IFormFile? Csv { get; set; }
}
