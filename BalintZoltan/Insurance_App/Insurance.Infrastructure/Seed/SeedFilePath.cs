namespace Infrastructure.Seed;

internal static class SeedFilePath
{
    public static string Get(string contentRootPath, string basePath, string fileName)
    {
        return Path.GetFullPath(Path.Combine(
            contentRootPath,
            basePath,
            fileName));
    }
}
