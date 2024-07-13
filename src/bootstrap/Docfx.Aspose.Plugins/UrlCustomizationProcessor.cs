using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using Aspose.CAD;
using Docfx.Common;
using Docfx.Plugins;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Docfx.Aspose.Plugins;

[Export(nameof(UrlCustomizationProcessor), typeof(IPostProcessor))]
public class UrlCustomizationProcessor : IPostProcessor
{
    private static readonly Regex HrefRelSuffixRegex = new Regex("^([\\./]+)", RegexOptions.Compiled);

    private static readonly Type[] AsposeCadTypes;
    private static readonly string[] AsposeCadNamespaces;

    private UrlCustomizationSettings _settings;

    static UrlCustomizationProcessor()
    {
        //AsposeCadTypes = typeof(Image).Assembly.GetExportedTypes();
        AsposeCadTypes =typeof(System.Composition.Convention.ExportConventionBuilder).Assembly.GetExportedTypes();
        AsposeCadNamespaces = AsposeCadTypes.Select(x => x.Namespace).Distinct().ToArray();
    }

    public ImmutableDictionary<string, object> PrepareMetadata(ImmutableDictionary<string, object> metadata)
    {
        _settings = new UrlCustomizationSettings(metadata);
        return metadata;
    }

    public Manifest Process(Manifest manifest, string outputFolder)
    {
        Debugger.Launch();

        foreach (var manifestItem in manifest.Files)
        {
            // ManagedReference, Toc, Resource, Conceptual
            //if (manifestItem.Type == "ManagedReference" || manifestItem.Type == "Toc")
            {
                foreach (var manifestItemOutputFile in manifestItem.Output)
                {
                    if (manifestItemOutputFile.Key == ".html")
                    {
                        var sourcePath = PathLinux.Combine(manifest.SourceBasePath, manifestItem.SourceRelativePath);

                        UpdateRelativeLinks(outputFolder, manifestItemOutputFile.Value);
                        RenameOutputFiles(outputFolder, manifestItemOutputFile.Value);
                    }
                    else if ( manifestItemOutputFile.Key == ".json")
                    {
                        UpdateTocHrefs(outputFolder, manifestItemOutputFile.Value);
                    }
                }
            }
        }

        return manifest;
    }

    private void UpdateTocHrefs(string outputFolder, OutputFileInfo manifestItemOutputFile)
    {
        var tocJsonPath = Path.Combine(outputFolder, manifestItemOutputFile.RelativePath);
        var json = JsonConvert.DeserializeObject(File.ReadAllText(tocJsonPath)) as JToken;

        TraverseTocHrefs(json);

        File.WriteAllText(tocJsonPath, JsonConvert.SerializeObject(json, Formatting.Indented));
    }

    private void RenameOutputFiles(string outputFolder, OutputFileInfo manifestItemOutputFile)
    {
        var outputPath = PathUniversal.Combine(outputFolder, manifestItemOutputFile.RelativePath);
        var relativePath = manifestItemOutputFile.RelativePath;

        // RENAMING
        // stage 1 (-ctor to class name)
        if (_settings.CtorToClassName)
        {
            var fileNameNoExt = Path.GetFileNameWithoutExtension(manifestItemOutputFile.RelativePath);
            fileNameNoExt = _settings.RenameCtorIfNeeded(fileNameNoExt, false);

            relativePath = PathLinux.Combine(
                Path.GetDirectoryName(manifestItemOutputFile.RelativePath)!,
                fileNameNoExt + Path.GetExtension(manifestItemOutputFile.RelativePath));
        }
                
        // RENAMING
        // stage 2 (lower case)
        if (_settings.LowerCaseFiles)
        {
            relativePath = PathLinux.Combine(
                Path.GetDirectoryName(manifestItemOutputFile.RelativePath)!,
                Path.GetFileName(manifestItemOutputFile.RelativePath).ToLower());
        }
 
        manifestItemOutputFile.RelativePath = relativePath;
        var newOutputPath = PathUniversal.Combine(outputFolder, manifestItemOutputFile.RelativePath);
        if (newOutputPath != outputPath)
        {
            File.Move(outputPath, newOutputPath, true);
        }
    }

    private void UpdateRelativeLinks(
        string outputFolder, OutputFileInfo manifestItemOutputFile)
    {
        var outputPath = Path.Combine(outputFolder, manifestItemOutputFile.RelativePath);
        
        var html = new HtmlDocument();
        html.Load(outputPath);

        foreach (var link in html.DocumentNode.SelectNodes("//a"))
        {
            var href = link.GetAttributeValue<string>("href", string.Empty);
            link.SetAttributeValue("href", UpdateLink(href, true) ?? href);
        }

        html.Save(outputPath);
    }

    private string UpdateLink(string href, bool addVirtualPath)
    {
        if (string.IsNullOrEmpty(href)
                || href.StartsWith("http:")
                || href.StartsWith("https:")
                || href.StartsWith("#"))
        {
            return null;
        }

        var anchor = string.Empty;
        var anchorIndex = href.IndexOf("#");
        if (anchorIndex != -1)
        {
            anchor = href.Substring(anchorIndex);
            href = href.Remove(anchorIndex);
        }

        // RE-LINKING
        // stage 1
        // /api/* => /*
        var relSuffixMatch = HrefRelSuffixRegex.Match(href);
        href = href.Substring(relSuffixMatch.Length);
        foreach (var preffixRaw in _settings.SuppressPrefixes)
        {
            var suffix = preffixRaw.TrimStart('.', '/');
            if (href.StartsWith(suffix))
            {
                href = href.Substring(suffix.Length);
                break;
            }
        }

        href = relSuffixMatch.Value + href;

        // RE-LINKING
        // stage 2
        // *.html => *
        foreach (var suffixRaw in _settings.SuppressExtensions)
        {
            if (href.EndsWith(suffixRaw, StringComparison.InvariantCultureIgnoreCase))
            {
                href = href.Remove(href.Length - suffixRaw.Length, suffixRaw.Length);
                break;
            }
        }

        // RE-LINKING
        // stage 3
        // detect symbols AND split with slashes
        if (!string.IsNullOrEmpty(_settings.SymbolsSeparator))
        {
            var separator = _settings.SymbolsSeparator;

            var member = Path.GetExtension(href).TrimStart('.');
            var noMember = Path.GetFileNameWithoutExtension(href);
            var prefixMatch = HrefRelSuffixRegex.Match(href);
            var noPrefix = href.Substring(prefixMatch.Length);

            var cadType = AsposeCadTypes.FirstOrDefault(
                x => x.FullName!.Equals(noMember.Replace('-', '`').Replace("-ctor", ".ctor")));

            if (cadType != null)
            {
                // property, method, event etc.
                href = $"{prefixMatch.Value}{cadType.Namespace}{separator}{cadType.Name}{separator}{member}";
            }
            else
            {
                cadType = AsposeCadTypes.FirstOrDefault(x => x.FullName!.Equals(href.Replace('-', '`')));
                if (cadType != null)
                {
                    // type
                    href = $"{prefixMatch.Value}{cadType.Namespace}{separator}{member}";
                }
                else
                {
                    var cadNs = AsposeCadNamespaces.FirstOrDefault(x => x.Equals(href));
                    if (!string.IsNullOrEmpty(cadNs))
                    {
                        // namespace
                    }
                }
            }
        }

        // RE-LINKING
        // stage 4
        // /<class>.-ctor => 
        if (_settings.CtorToClassName)
        {
            href = _settings.RenameCtorIfNeeded(href, true);
        }

        // RE-LINKING
        // stage 5
        // add trailing slash
        href += "/";

        // RE-LINKING
        // stage 6
        // lower case
        href = href.ToLower();

        return (addVirtualPath ? _settings.VirtualPath : string.Empty) + href + anchor;
    }

    private void TraverseTocHrefs(JToken token)
    {
        if (token.Type == JTokenType.Object)
        {
            var obj = (JObject)token;
            foreach (var property in obj.Properties())
            {
                if (property.Name.EndsWith("href", StringComparison.InvariantCultureIgnoreCase))
                {
                    property.Value = UpdateLink(property.Value.Value<string>(), false);
                }

                TraverseTocHrefs(property.Value);
            }
        }
        else if (token.Type == JTokenType.Array)
        {
            foreach (var item in (JArray)token)
            {
                TraverseTocHrefs(item);
            }
        }
    }

    private class FromTo
    {
        public string From { get; set; }   
        public string To { get; set; }
    }
}