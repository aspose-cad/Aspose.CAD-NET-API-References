using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Aspose.CAD;
using Docfx.Common;
using Docfx.Plugins;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Docfx.Aspose.Plugins.Processors;

[Export(nameof(UrlCustomizationProcessor), typeof(IPostProcessor))]
public class UrlCustomizationProcessor : IPostProcessor
{
    private static readonly Regex PackageVersionTemplateRegex = new Regex(
        "VER_S_ION", RegexOptions.Compiled);
    
    private static readonly Regex GitVersionTimestampRegex = new Regex(
        "API Reference Version\\: [a-zA-Z0-9]+", RegexOptions.Compiled);
    
    private static readonly Regex GitVersionTemplateRegex = new Regex(
        "GIT_COM_MIT", RegexOptions.Compiled);
    
    private static readonly Regex HrefRelSuffixRegex = new Regex("^([\\./]+)", RegexOptions.Compiled);

    private static readonly Type[] AsposeCadTypes;
    private static readonly string[] AsposeCadNamespaces;

    private UrlCustomizationSettings _settings;

    static UrlCustomizationProcessor()
    {
        AsposeCadTypes = typeof(Image).Assembly
            .GetExportedTypes()
            .SelectMany(x => x.GetNestedTypes().Union([x]))
            .Distinct()
            .ToArray();
        
        //AsposeCadTypes =typeof(System.Composition.Convention.ExportConventionBuilder).Assembly.GetExportedTypes();
        AsposeCadNamespaces = AsposeCadTypes.Select(x => x.Namespace).Distinct().ToArray();
    }

    public UrlCustomizationProcessor(UrlCustomizationSettings settings)
        : this()
    {
        _settings = settings;
    }

    public UrlCustomizationProcessor()
    {
    }

    public ImmutableDictionary<string, object> PrepareMetadata(ImmutableDictionary<string, object> metadata)
    {
        _settings = new UrlCustomizationSettings(metadata);
        return metadata;
    }

    public Manifest Process(Manifest manifest, string outputFolder)
    {
#if DEBUG
        Debugger.Launch();
#endif
        
        UpdateVersionTimestamp(outputFolder, ".html");
        UpdateVersionTimestamp(outputFolder, ".md");
        
        UpdateVersionOnIndex(outputFolder, ".html");
        UpdateVersionOnIndex(outputFolder, ".md");
        
        UpdateHrefsOnJson(Path.Combine(outputFolder, "index.json"));

        foreach (var manifestItem in manifest.Files)
        {
            // ManagedReference, Toc, Resource, Conceptual
            //if (manifestItem.Type == "ManagedReference" || manifestItem.Type == "Toc")
            {
                foreach (var manifestItemOutputFile in manifestItem.Output)
                {
                    if (manifestItemOutputFile.Key == ".html")
                    {
                        var sourcePath = PathLinux.Combine(
                            manifest.SourceBasePath, manifestItem.SourceRelativePath);

                        UpdateRelativeLinks(outputFolder, manifestItemOutputFile.Value);
                        RenameOutputFiles(outputFolder, manifestItemOutputFile.Value);
                    }
                    else if ( manifestItemOutputFile.Key == ".json")
                    {
                        var tocJsonPath = Path.Combine(outputFolder, manifestItemOutputFile.Value.RelativePath);
                        UpdateHrefsOnJson(tocJsonPath);
                    }
                }
            }
        }

        Logger.LogInfo("Finalized URL customization");

        return manifest;
    }
    
    public void UpdateVersionTimestamp(string outputFolder, string extension)
    {
        var indexFilePath = Path.Combine(outputFolder, "index" + extension);
        if (!File.Exists(indexFilePath))
        {
            return;
        }
        
        var html = File.ReadAllText(indexFilePath);
        
        var gitTag = GetGitCommit(outputFolder);
        
        html = GitVersionTimestampRegex.Replace(html, _ => "API Reference Version: " + gitTag);
        html = GitVersionTemplateRegex.Replace(html, _ => gitTag);
        
        File.WriteAllText(indexFilePath, html);
    }
    
    public void UpdateVersionOnIndex(string outputFolder, string extension)
    {
        var indexFilePath = Path.Combine(outputFolder, "index" + extension);
        if (!File.Exists(indexFilePath))
        {
            return;
        }
        
        var html = File.ReadAllText(indexFilePath);
        
        var packageVersion = typeof(Image).Assembly.GetName().Version!.ToString(2);
        
        html = PackageVersionTemplateRegex.Replace(html, _ => packageVersion);
        
        File.WriteAllText(indexFilePath, html);
    }
    
    private string GetGitCommit(string absPathDir)
    {
        var gitDir = absPathDir;
        
        while (!Directory.Exists(Path.Combine(gitDir, ".git")))
        {
            gitDir = Directory.GetParent(gitDir)?.FullName;
            if (gitDir == null)
            {
                throw new InvalidOperationException("Not a git repository");
            }
        }
        
        var processInfo = new ProcessStartInfo("git", "rev-parse HEAD")
        {
            WorkingDirectory = gitDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        
        using (var process = System.Diagnostics.Process.Start(processInfo))
        {
            var output = process!.StandardOutput.ReadToEnd();
            process.WaitForExit();
            
            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException($"Git command failed: {process.StandardError.ReadToEnd()}");
            }
            
            return output.Trim();
        }
    }
    
    public void UpdateHrefsOnJson(string jsonFilePath)
    {
        var json = JsonConvert.DeserializeObject(File.ReadAllText(jsonFilePath)) as JToken;

        TraverseTocOrIndexHrefs(json, true);

        File.WriteAllText(jsonFilePath, JsonConvert.SerializeObject(json, Formatting.Indented));
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
                Path.GetFileName(relativePath).ToLower());
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
            link.SetAttributeValue("href", UpdateLink(href) ?? href);
        }

        html.Save(outputPath);
    }

    public string UpdateLink(string href)
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
        foreach (var prefixRaw in _settings.SuppressPrefixes)
        {
            var suffix = prefixRaw.TrimStart('.', '/');
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

            var cadType = AsposeCadTypes.FirstOrDefault(
                x => x.FullName!.Equals(
                        noMember.AsUnsafeGenericName().Replace("-ctor", ".ctor"),
                        StringComparison.InvariantCultureIgnoreCase)
                    || x.FullName!.Equals(
                        noMember.AsUnsafeGenericName().Replace("-ctor", ".ctor").ReplaceLast(".", "+"),
                        StringComparison.InvariantCultureIgnoreCase));

            if (cadType != null)
            {
                // property, method, event etc.
                href = $"{prefixMatch.Value}{cadType.Namespace}" +
                       $"{separator}{cadType.Name.AsSafeGenericName()}" +
                       $"{separator}{member}";
            }
            else
            {
                cadType = AsposeCadTypes.FirstOrDefault(
                    x => x.FullName!.Equals(
                        href.AsUnsafeGenericName(),
                        StringComparison.InvariantCultureIgnoreCase));
                
                if (cadType != null)
                {
                    // type
                    href = $"{prefixMatch.Value}{cadType.Namespace}{separator}{member}";
                }
                else
                {
                    var cadNs = AsposeCadNamespaces.FirstOrDefault(
                        x => x.Equals(href, StringComparison.InvariantCultureIgnoreCase));
                    
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

        return _settings.VirtualPath + href.TrimStart('/') + anchor;
    }

    private void TraverseTocOrIndexHrefs(JToken token, bool processRootPropertyNames)
    {
        if (token.Type == JTokenType.Object)
        {
            var obj = (JObject)token;
            foreach (var property in obj.Properties().ToList())
            {
                if (property.Name == "href")
                {
                    var value = property.Value.Value<string>();
                    if (value.EndsWith(".html"))
                    {
                        property.Value = UpdateLink(value);
                    }
                }
                
                if (processRootPropertyNames && property.Name.EndsWith(".html"))
                {
                    var oldName = property.Name;
                    var value = property.Value;
                    
                    obj.Remove(oldName);
                    
                    var prepared = property
                        .Name
                        .Replace("/cad/net/", string.Empty)
                        .Replace("api/cad/net/", string.Empty);
                        
                    var newName = UpdateLink(prepared);
                    obj.Add(newName, value);
                }

                TraverseTocOrIndexHrefs(property.Value, false);
            }
        }
        else if (token.Type == JTokenType.Array)
        {
            foreach (var item in (JArray)token)
            {
                TraverseTocOrIndexHrefs(item, false);
            }
        }
    }

    private class FromTo
    {
        public string From { get; set; }   
        public string To { get; set; }
    }
}