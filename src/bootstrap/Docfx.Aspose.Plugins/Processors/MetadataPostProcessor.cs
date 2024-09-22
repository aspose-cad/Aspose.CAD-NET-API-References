using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using Docfx.Build.Common;
using Docfx.Common;
using Docfx.Plugins;
using YamlDotNet.RepresentationModel;

namespace Docfx.Aspose.Plugins.Processors;

[Export(typeof(IDocumentProcessor))]
public class MetadataPostProcessor : IDocumentProcessor
{
    private static readonly List<IDocumentBuildStep> _steps = 
    [
        new FixExamplesBuildStep()
    ];
    
    public string Name => nameof(MetadataPostProcessor);
    
    [ImportMany(nameof(MetadataPostProcessor))]
    public IEnumerable<IDocumentBuildStep> BuildSteps { get; set; } = _steps;
    
    public ProcessingPriority GetProcessingPriority(FileAndType file)
    {
        if (file.File.EndsWith(".yml") || file.File.EndsWith(".yaml"))
        {
            return ProcessingPriority.Normal;
        }
        
        return ProcessingPriority.NotSupported;
    }
    
    public FileModel Load(FileAndType file, ImmutableDictionary<string, object> metadata)
    {
        return new FileModel(file, null);
    }
    
    public SaveResult Save(FileModel model)
    {
        return new SaveResult
        {
            DocumentType = "Yaml",
            FileWithoutExtension = model.File
        };
    }
    
    public void UpdateHref(FileModel model, IDocumentBuildContext context)
    {
    }
}