using CommandLine;

namespace Docfx.Aspose.Tools.Args;

[Verb("index", HelpText = "Post-process sitemap.")]
public class LunrIndexModeArgs : CommandLineArgsBase
{
    [Value(0, MetaName = "lunr", Required = true, HelpText = "path to index.json file.")]
    public string Lunr { get; set; }
    
    [Value(1, MetaName = "docfx", Required = true, HelpText = "path to docfx.json file.")]
    public string Docfx { get; set; }
}