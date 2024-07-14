using System.Collections.Concurrent;
using HtmlAgilityPack;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace Docfx.Aspose.Tools;

public class LinksVerifier
{
    private const int MaxDegreeOfParallelism = 100;
    
    private readonly Dictionary<string, List<string>> _failedLinks = new();
    private readonly HashSet<string> _visitedLinks = new();
    private readonly VerifyModeArgs _opts;
    private readonly HttpClient _http;
    
    private string _genuineServerUrl;

    public LinksVerifier(HttpClient http, VerifyModeArgs opts)
    {
        _http = http;
        _opts = opts;
    }

    public async Task<int> VerifyAsync()
    {
        XDocument doc = XDocument.Parse(File.ReadAllText(_opts.Sitemap));
        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";

        foreach (XElement urlElement in doc.Root.Elements(ns + "url"))
        {
            XElement locElement = urlElement.Element(ns + "loc");
            if (locElement != null)
            {
                //Console.WriteLine($"Processing sitemap link: {locElement.Value}...");
                var newUrl = ConvertUrl(locElement.Value);
                await ProcessUrl(newUrl, null);
            }
        }

        if (_failedLinks.Any())
        {
            Console.WriteLine("\nVerification errors:\n=========");
            Console.WriteLine(JsonConvert.SerializeObject(_failedLinks, Formatting.Indented));

            var errorsCount = _failedLinks.SelectMany(x => x.Value).Count();
            Console.WriteLine($"Verification failed: {errorsCount} errors on {_failedLinks.Keys.Count} pages");

            if (!_opts.DryRun)
            {
                return 1;
            }
        }

        return 0;
    }

    private async Task ProcessUrl(string url, string sourceUrl)
    {
        if (!_visitedLinks.Add(url))
        {
            return;
        }

        string pageContent = null;
        try
        {
            pageContent = await _http.GetStringAsync(url);
        }
        catch (Exception e)
        {
            if (!_failedLinks.TryGetValue(sourceUrl ?? "<sitemap>", out var related))
            {
                related = new List<string>();
                _failedLinks.Add(sourceUrl ?? "sitemap", related);
            }
            
            related.Add(url);
            
            Console.Error.WriteLine($"Verification failed: {url}\n{e}");
            
            return;
        }
        
        HtmlDocument document = new HtmlDocument();
        document.LoadHtml(pageContent);

        var links = document.DocumentNode.SelectNodes("//a[@href]")
                            .Select(a => a.GetAttributeValue("href", string.Empty))
                            .Where(href => !string.IsNullOrEmpty(href) && IsSuitableLink(href))
                            .Select(href => ConvertUrl(href))
                            .Distinct()
                            .ToList();

        //Console.WriteLine($"Found {links.Count} links on {url}...");
        
        // await Task.WhenAll(links
        //     .AsParallel()
        //     .WithDegreeOfParallelism(MaxDegreeOfParallelism)
        //     .Select(link => Task.Run(async () =>
        //     {
        //         Console.WriteLine($"Processing page anchor link {link}...");
        //         await ProcessUrl(link, url);
        //     }))
        // );
        foreach (var link in links)
        {
            await ProcessUrl(link, url);
        }
    }

    private string ConvertUrl(string url)
    {
        if (string.IsNullOrEmpty(_opts.ServerUrl))
        {
            return url;
        }

        Uri uri = new Uri(url, UriKind.RelativeOrAbsolute);
        if (!uri.IsAbsoluteUri)
        {
            uri = new Uri(new Uri(_opts.ServerUrl), uri);
        }
        else
        {
            _genuineServerUrl = uri.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped);

            var substitution = new Uri(_opts.ServerUrl);

            var builder = new UriBuilder(uri)
            {
                Scheme = substitution.Scheme,
                Host = substitution.Host,
                Port = substitution.Port
            };

            uri = builder.Uri;
        }
        
        return uri.ToString();
    }

    private bool IsSuitableLink(string url)
    {
        if (string.IsNullOrEmpty(url) || url.Contains("#"))
        {
            return false;
        }
        
        return (!url.StartsWith("http://") && !url.StartsWith("https://"))
               || url.StartsWith(_opts.ServerUrl)
               || url.StartsWith(_genuineServerUrl);
    }
}