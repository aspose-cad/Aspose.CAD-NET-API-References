using System.Collections.Immutable;

namespace Docfx.Aspose.Plugins;

public class UrlCustomizationSettings
{
    public UrlCustomizationSettings()
    {
    }

    public UrlCustomizationSettings(ImmutableDictionary<string, object> metadata)
    {
        // "_suppressPrefixes": [ "/api" ],
        SuppressPrefixes = Array.Empty<string>();
        // "_suppressExtensions": [ ".html" ]
        SuppressExtensions = Array.Empty<string>();

        if (metadata.TryGetValue("_suppressPrefixes", out var suppressPrefixesRaw))
        {
            SuppressPrefixes = ((object[])suppressPrefixesRaw).Select(x => x.ToString()).ToArray();
        }

        if (metadata.TryGetValue("_suppressExtensions", out var suppressExtensionsRaw))
        {
            SuppressExtensions = ((object[])suppressExtensionsRaw).Select(x => x.ToString()).ToArray();
        }

        if (metadata.TryGetValue("_trailingSlash", out var trailingSlashRaw))
        {
            TrailingSlash = (bool)trailingSlashRaw;
        }

        if (metadata.TryGetValue("_lowerCaseFiles", out var lowerCaseFilesRaw))
        {
            LowerCaseFiles = (bool)lowerCaseFilesRaw;
        }

        if (metadata.TryGetValue("_ctorToClassName", out var ctorToClassNameRaw))
        {
            CtorToClassName = (bool)ctorToClassNameRaw;
        }

        if (metadata.TryGetValue("_symbolsSeparator", out var symbolsSeparatorRaw))
        {
            SymbolsSeparator = (string)symbolsSeparatorRaw;
        }

        if (metadata.TryGetValue("_virtualPath", out var virtualPathRaw))
        {
            VirtualPath = '/' + ((string)virtualPathRaw).Trim('/') + '/';
        }
    }

    public string[] SuppressPrefixes { get; set; }
    public string[] SuppressExtensions { get; set; }
    public bool TrailingSlash { get; set; }
    public bool LowerCaseFiles { get; set; }
    public bool CtorToClassName { get; set; }
    public string SymbolsSeparator { get; set; }
    public string VirtualPath { get; set; }

    public string RenameCtorIfNeeded(string fileNameNoExt, bool hasDirStructure)
    {
        if (!fileNameNoExt.EndsWith("-ctor", StringComparison.InvariantCultureIgnoreCase))
        {
            return fileNameNoExt;
        }

        if (hasDirStructure)
        {
            var classNamePath = Path.GetDirectoryName(fileNameNoExt);
            var className = Path.GetFileName(classNamePath);

            return PathLinux.Combine(classNamePath, className);
        }

        var dirPath = Path.GetFileNameWithoutExtension(fileNameNoExt);
        var dirName = Path.GetExtension(dirPath).Trim('.');

        return dirPath + "." + dirName;
    }
}