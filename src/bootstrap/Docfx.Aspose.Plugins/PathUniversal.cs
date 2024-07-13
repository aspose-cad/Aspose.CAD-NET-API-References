namespace Docfx.Aspose.Plugins;

public static class PathUniversal
{
    public static string Combine(params string[] paths)
    {
        var correct = Path.DirectorySeparatorChar;
        var incorrect = correct == '/' ? '\\' : '/';

        return Path.Combine(paths).Replace(incorrect, correct);
    }
}
