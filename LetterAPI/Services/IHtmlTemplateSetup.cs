using System.Text.RegularExpressions;
namespace LetterAPI.Services;

public interface IHtmlTemplateSetup
{
    string Apply(string template, IReadOnlyDictionary<string, string> bag);
}

public class HtmlTemplateSetup : IHtmlTemplateSetup
{
    private static readonly Regex TokenPattern = new(@"\$(\w+)", RegexOptions.Compiled);
    
    public string Apply(string template, IReadOnlyDictionary<string, string> bag)
    {
        return TokenPattern.Replace(template, m =>
        {
            var key = m.Groups[1].Value;
            if (bag.TryGetValue(key, out var value)) return value ?? "";
            if (string.Equals(key, "CardNunber", StringComparison.Ordinal)) 
            {
                if (bag.TryGetValue("CardNumber", out var alt)) return alt ?? "";
            }
            return m.Value;
        });
    }
}
