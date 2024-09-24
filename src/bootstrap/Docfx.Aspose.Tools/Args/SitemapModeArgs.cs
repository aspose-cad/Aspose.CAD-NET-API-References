using CommandLine;

namespace Docfx.Aspose.Tools.Args;

[Verb("sitemap", HelpText = "Post-process sitemap.")]
public class SitemapModeArgs : CommandLineArgsBase
{
    [Value(0, MetaName = "sitemap", Required = true, HelpText = "path to sitemap.xml file.")]
    public string Sitemap { get; set; }
    
    [Value(1, MetaName = "docfx", Required = true, HelpText = "path to docfx.json file.")]
    public string Docfx { get; set; }
}