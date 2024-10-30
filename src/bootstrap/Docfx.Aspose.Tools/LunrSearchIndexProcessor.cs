using System.Xml.Linq;
using Docfx.Aspose.Plugins;
using Docfx.Aspose.Plugins.Processors;
using Docfx.Aspose.Tools.Args;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Docfx.Aspose.Tools;

public class LunrSearchIndexProcessor
{
    private readonly LunrIndexModeArgs _opts;
    private readonly UrlCustomizationProcessor _urlCustomizationProcessor;
    
    public LunrSearchIndexProcessor(LunrIndexModeArgs opts)
    {
        _opts = opts;
        _urlCustomizationProcessor = new UrlCustomizationProcessor(new UrlCustomizationSettings(opts.Docfx));
    }

    public int Process()
    {
        var indexJson = Path.Combine(Path.GetDirectoryName(_opts.Lunr), "index.json");
        
        _urlCustomizationProcessor.UpdateHrefsOnJson(indexJson);
        _urlCustomizationProcessor.UpdateVersionTimestamp(Path.GetDirectoryName(_opts.Lunr), ".html");
        _urlCustomizationProcessor.UpdateVersionOnIndex(Path.GetDirectoryName(_opts.Lunr), ".html");
        
        return 0;
    }
}