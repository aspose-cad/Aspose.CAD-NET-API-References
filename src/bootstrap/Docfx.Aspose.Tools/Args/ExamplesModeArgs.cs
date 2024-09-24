using CommandLine;

namespace Docfx.Aspose.Tools.Args;

[Verb("examples", HelpText = "fix <example> tags")]
public class ExamplesModeArgs : CommandLineArgsBase
{
    [Value(0, MetaName = "root-dir", Required = true, HelpText = "path to root dir where to seek for .cs files")]
    public string RootDir { get; set; }
    
    [Option("dry-run", Required = false, Default = false, HelpText = "if set to true - .cs files won't be updated")]
    public bool DryRun { get; set; }
}
