namespace Docfx.Aspose.Tools;

public static class TaskExtensions
{
    public static T AsSync<T>(this Task<T> task)
    {
        return task.ConfigureAwait(false).GetAwaiter().GetResult();
    }
}