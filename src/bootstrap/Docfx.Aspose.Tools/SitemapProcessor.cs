using System.Xml.Linq;
using Docfx.Aspose.Plugins;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Docfx.Aspose.Tools;

public class SitemapProcessor
{
    private readonly SitemapModeArgs _opts;

    public SitemapProcessor(SitemapModeArgs opts)
    {
        _opts = opts;
    }

    public int Process()
    {
        XDocument doc = XDocument.Parse(File.ReadAllText(_opts.Sitemap));
        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";

        var docfxJson = JsonConvert.DeserializeObject(File.ReadAllText(_opts.Docfx)) as JObject;
        var settings = new UrlCustomizationSettings()
        {
            LowerCaseFiles = (bool)docfxJson.SelectToken("build.globalMetadata._lowerCaseFiles"),
            SuppressExtensions = ((JArray)docfxJson.SelectToken("build.globalMetadata._suppressExtensions"))
                .Select(x => x.Value<string>())
                .ToArray(),
            SuppressPrefixes = ((JArray)docfxJson.SelectToken("build.globalMetadata._suppressPrefixes"))
                .Select(x => x.Value<string>())
                .ToArray(),
            SymbolsSeparator = (string)docfxJson.SelectToken("build.globalMetadata._symbolsSeparator"),
            TrailingSlash = (bool)docfxJson.SelectToken("build.globalMetadata._trailingSlash"),
            VirtualPath = (string)docfxJson.SelectToken("build.globalMetadata._virtualPath"),
            CtorToClassName = (bool)docfxJson.SelectToken("build.globalMetadata._ctorToClassName")
        };

        var processor = new UrlCustomizationProcessor(settings);

        foreach (XElement urlElement in doc.Root.Elements(ns + "url"))
        {
            XElement locElement = urlElement.Element(ns + "loc");
            if (locElement != null)
            {
                var uri = new Uri(locElement.Value);
                var schemeAndServer = new Uri(uri.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped));

                string newLoc = processor.UpdateLink(uri.PathAndQuery.Substring(settings.VirtualPath!.Length));
                locElement.SetValue(new Uri(schemeAndServer, newLoc).AbsoluteUri);
            }
        }

        doc.Save(_opts.Sitemap);
        Console.WriteLine("Sitemap has been updated.");

        return 0;
    }
}