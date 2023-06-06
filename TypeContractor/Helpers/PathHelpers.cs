using System.Text;

namespace TypeContractor.Helpers;

internal static class PathHelpers
{
    public static string RelativePath(string targetTypeName, string relativeToTypeName)
    {
        var targetPath = targetTypeName
            .Replace('.', Path.DirectorySeparatorChar)
            .Split(Path.DirectorySeparatorChar)
            .ToList();

        var relativePath = relativeToTypeName
            .Replace('.', Path.DirectorySeparatorChar)
            .Split(Path.DirectorySeparatorChar)
            .ToList();

        if (targetPath.SequenceEqual(relativePath))
            return ".";

        return RelativePathDiff(targetPath.ToArray(), relativePath.ToArray());
    }

    private static string RelativePathDiff(string[] absDirs, string[] relDirs)
    {
        // Get the shortest of the two paths
        int len = absDirs.Length < relDirs.Length ? absDirs.Length : relDirs.Length;

        // Use to determine where in the loop we exited
        int lastCommonRoot = -1;
        int index;

        // Find common root
        for (index = 0; index < len; index++)
        {
            if (absDirs[index] == relDirs[index]) lastCommonRoot = index;
            else break;
        }

        // If we didn't find a common prefix then throw
        if (lastCommonRoot == -1)
        {
            throw new ArgumentException($"Paths \"{Path.Combine(absDirs)}\" and \"{Path.Combine(relDirs)}\" do not have a common base");
        }

        // Build up the relative path
        var relativePath = new StringBuilder();

        // Add on the ..
        for (index = lastCommonRoot + 1; index < relDirs.Length; index++)
        {
            if (relDirs[index].Length > 0) relativePath.Append("../");
        }

        // If there are no dots, go the other way
        if (relativePath.Length == 0)
        {
            relativePath.Append("./");
        }

        // Add on the folders
        for (index = lastCommonRoot + 1; index <= absDirs.Length - 1; index++)
        {
            relativePath.Append(absDirs[index] + "/");
        }

        return relativePath.ToString();
    }
}
