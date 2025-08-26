using System.Text;
namespace LettersApp.Services;
public static class CsvUtils
{
    public static List<Dictionary<string,string>> Parse(string csvText)
    {
        var rows = new List<Dictionary<string, string>>();
        using var reader = new StringReader(csvText);
        string? headerLine = reader.ReadLine();

        if (headerLine is null) 
            return rows;

        var headers = CSVLineSplit(headerLine).Select(h => h.Trim()).ToArray();

        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var cells = CSVLineSplit(line);
            var dict = new Dictionary<string, string>(StringComparer.Ordinal);
            for (int i = 0; i < headers.Length; i++)
            {
                var val = i < cells.Count ? cells[i] : "";
                dict[headers[i]] = val;
            }
            rows.Add(dict);
        }
        return rows;
    }
    private static List<string> CSVLineSplit(string line)
    {
        var result = new List<string>();
        var sb = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '\"')
            {
                if (inQuotes && i + 1 < line.Length && line[i+1] == '\"')
                {
                    // Escaped quote
                    sb.Append('\"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(sb.ToString());
                sb.Clear();
            }
            else
            {
                sb.Append(c);
            }
        }
        result.Add(sb.ToString());
        return result;
    }
}
