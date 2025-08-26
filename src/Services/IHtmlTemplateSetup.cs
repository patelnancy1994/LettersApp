using System.Text.RegularExpressions;
namespace LettersApp.Services;

public interface IHtmlTemplateSetup
{
    /// <summary>
    /// Replaces placeholders like $Token in the template using values from the bag (case-sensitive).
    /// Missing tokens are left as it is.
    /// </summary>
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
            // Support the known typo "$CardNunber" in the provided template by mapping to "CardNumber"
            if (string.Equals(key, "CardNunber", StringComparison.Ordinal)) 
            {
                if (bag.TryGetValue("CardNumber", out var alt)) return alt ?? "";
            }
            return m.Value;
        });
    }
}
