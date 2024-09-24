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
        string jsonPath = _opts.Lunr;
        string json = File.ReadAllText(jsonPath);
        JObject root = JObject.Parse(json);
        
        JObject updatedRoot = new JObject();
        foreach (var property in root.Properties())
        {
            //string key = property.Name;
            if (property.Value is JObject value && value.TryGetValue("href", out JToken hrefToken))
            {
                var hrefValue = hrefToken.ToString(); //_urlCustomizationProcessor.UpdateLink(hrefToken.ToString()));
                updatedRoot[hrefValue] = property.Value;
            }
        }
        
        string updatedJson = updatedRoot.ToString(Formatting.Indented);
        File.WriteAllText(jsonPath, updatedJson);
        Console.WriteLine("Done Lunr index.");
        
        return 0;
    }
}