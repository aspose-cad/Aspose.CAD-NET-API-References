using CommandLine;

namespace Docfx.Aspose.Tools.Args;

[Verb("verify", HelpText = "Verify all links")]
public class VerifyModeArgs : CommandLineArgsBase
{
    [Value(0, MetaName = "sitemap", Required = true, HelpText = "path to sitemap.xml file.")]
    public string Sitemap { get; set; }
    
    [Option('s', "server", Required = false, HelpText = "absolute root url of a server")]
    public string ServerUrl { get; set; }
    
    [Option("dry-run", Required = false, Default = false, HelpText = "if set to true - error will not be raised")]
    public bool DryRun { get; set; }
}