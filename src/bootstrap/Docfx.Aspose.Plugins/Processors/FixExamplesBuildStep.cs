using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Docfx.Common;
using Docfx.Plugins;
using YamlDotNet.RepresentationModel;

namespace Docfx.Aspose.Plugins.Processors;

[Export(nameof(MetadataPostProcessor), typeof(IDocumentBuildStep))]
public class FixExamplesBuildStep : IDocumentBuildStep
{
    public string Name => nameof(FixExamplesBuildStep);
    
    public int BuildOrder => int.MaxValue;
    
    public IEnumerable<FileModel> Prebuild(ImmutableList<FileModel> models, IHostService host)
    {
        return models;
    }
    
    public void Build(FileModel model, IHostService host)
    {
    }
    
    public void Postbuild(ImmutableList<FileModel> models, IHostService host)
    {
//#if DEBUG
//        Debugger.Launch();
//#endif
        
        // TODO: fix <example> tags - they have to contain <code> tag
        return;
        
        foreach (var model in models)
        {
            TryFixIfNeeded(model.File);
        }
    }
    
    private void TryFixIfNeeded(string filePath)
    {
        return;
        
        // filePath = EnvironmentContext.FileAbstractLayer.GetPhysicalPath(filePath);
        //
        // if (!File.Exists(filePath))
        // {
        //     return;
        // }
        //
        // var wasFixed = false;
        //
        // var input = new StreamReader(filePath);
        // var yaml = new YamlStream();
        // yaml.Load(input);
        //
        // var items = (YamlSequenceNode)yaml.Documents[0].RootNode["items"];
        // foreach (var item in items.OfType<YamlMappingNode>())
        // {
        //     if (item.Children.TryGetValue("example", out var examplesRaw))
        //     {
        //         var examples = (YamlSequenceNode)examplesRaw;
        //         foreach (var example in examples.Cast<YamlScalarNode>())
        //         {
        //             var value = Regex.Replace(example.Value!, "<pre><code.*?>|</pre></code>", string.Empty);
        //             if (value != example.Value)
        //             {
        //                 example.Value = value;
        //                 wasFixed = true;
        //             }
        //         }
        //     }
        // }
        //
        // input.Close();
        //
        // var output = new StreamWriter(filePath);
        // yaml.Save(output);
        // output.Close();
        //
        // if (wasFixed)
        // {
        //     Logger.LogInfo($"Fixed examples: {filePath}");
        // }
    }
}