using System.Text.RegularExpressions;

namespace Docfx.Aspose.Tools;

public class ExamplesSourceValidator
{
    private readonly ExamplesModeArgs _args;
    
    public ExamplesSourceValidator(ExamplesModeArgs args)
    {
        _args = args;
    }
    
    public async Task<int> ProcessAsync()
    {
        if (!Directory.Exists(_args.RootDir))
        {
            Console.WriteLine("Directory does not exist.");
            return -1;
        }

        var csFiles = Directory.GetFiles(_args.RootDir, "*.cs", SearchOption.AllDirectories);

        foreach (var file in csFiles)
        {
            if (file.Contains("Aspose.Drawing")
                || file.Contains("Aspose.Extensions")
                || file.Replace("\\", "/").Contains("Core/src"))
            {
                continue;
            }
            
            string content = await File.ReadAllTextAsync(file);
            ValidateXmlTags(file, content);
        }
        
        return 0;
    }

    private void ValidateXmlTags(string filePath, string fileContent)
    {
        // Regex to find XML documentation comments (/// ... comments)
        var xmlCommentPattern = new Regex(@"///\s*<([^>]+)>.*?<\/\1>", RegexOptions.Singleline);

        var matches = xmlCommentPattern.Matches(fileContent);

        foreach (Match match in matches)
        {
            string xmlContent = match.Value;

            // Regex to find <example>...</example> blocks within comments
            var examplePattern = new Regex(@"<example\b[^>]*>(.*?)<\/example>", RegexOptions.Singleline);
            var codePattern = new Regex(@"<code\b[^>]*>(.*?)<\/code>", RegexOptions.Singleline);

            var exampleMatches = examplePattern.Matches(xmlContent);

            foreach (Match exampleMatch in exampleMatches)
            {
                if (!codePattern.IsMatch(exampleMatch.Value))
                {
                    Console.WriteLine(
                        $"File: {filePath} - Missing <code> tag inside <example> tag in XML documentation.");
                }
            }

            // Validate the correct order and matching of XML tags within comments
            ValidateXmlOrderAndMatching(filePath, xmlContent);
        }
    }

    private void ValidateXmlOrderAndMatching(string filePath, string xmlContent)
    {
        var stack = new Stack<string>();
        var tagPattern = new Regex(@"<\/?(\w+)(?:\s+[^>]+)?>", RegexOptions.Singleline);

        foreach (Match match in tagPattern.Matches(xmlContent))
        {
            if (match.Groups[1].Value != "example" && match.Groups[1].Value != "code")
            {
                continue;
            }
            
            string tagName = match.Groups[1].Value;
            bool isClosingTag = match.Value.StartsWith("</");
            
            if (match.Value.EndsWith("/>"))
            {
                continue;
            }

            if (!isClosingTag)
            {
                stack.Push(tagName);
            }
            else
            {
                if (stack.Count == 0 || stack.Peek() != tagName)
                {
                    Console.WriteLine(
                        $"File: {filePath}[{match.Index}] - "
                        + $"Incorrect tag order or mismatched tag in XML documentation: {match.Value}");
                    
                    return;
                }
                
                stack.Pop();
            }
        }

        if (stack.Count > 0)
        {
            Console.WriteLine($"File: {filePath} - Unmatched opening tags found in XML documentation.");
        }
    }
}