using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace Docfx.Aspose.Tools;

public abstract class CommandLineArgsBase
{
}

[Verb("sitemap", HelpText = "Post-process sitemap.")]
public class SitemapModeArgs : CommandLineArgsBase
{
    [Value(0, MetaName = "sitemap", Required = true, HelpText = "path to sitemap.xml file.")]
    public string Sitemap { get; set; }

    [Value(1, MetaName = "docfx", Required = true, HelpText = "path to docfx.json file.")]
    public string Docfx { get; set; }
}

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


[Verb("examples", HelpText = "fix <example> tags")]
public class ExamplesModeArgs : CommandLineArgsBase
{
    [Value(0, MetaName = "root-dir", Required = true, HelpText = "path to root dir where to seek for .cs files")]
    public string RootDir { get; set; }
    
    [Option("dry-run", Required = false, Default = false, HelpText = "if set to true - .cs files won't be updated")]
    public bool DryRun { get; set; }
}
