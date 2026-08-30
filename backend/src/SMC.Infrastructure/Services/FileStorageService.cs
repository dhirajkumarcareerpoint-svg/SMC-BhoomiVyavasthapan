using Microsoft.Extensions.Configuration;
using SMC.Application.Interfaces;

namespace SMC.Infrastructure.Services;

/// <summary>
/// सुरक्षित file upload: फक्त परवानगी असलेले extensions, आकार मर्यादा, आणि random file नाव
/// जेणेकरून path traversal किंवा overwrite होणार नाही.
/// </summary>
public class FileStorageService : IFileStorageService
{
    private readonly string _rootPath;
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        { ".pdf", ".jpg", ".jpeg", ".png", ".docx", ".doc", ".xlsx" };
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    public FileStorageService(IConfiguration config)
    {
        _rootPath = config["FileStorage:RootPath"] ?? Path.Combine(AppContext.BaseDirectory, "UploadedFiles");
        Directory.CreateDirectory(_rootPath);
    }

    public bool IsAllowedFile(string fileName, long fileSize)
    {
        var ext = Path.GetExtension(fileName);
        return AllowedExtensions.Contains(ext) && fileSize > 0 && fileSize <= MaxFileSizeBytes;
    }

    public async Task<(string storedFileName, string filePath, long size, string contentType)> SaveFileAsync(
        Stream fileStream, string originalFileName, string contentType, string subFolder)
    {
        var safeSubFolder = string.Concat(subFolder.Where(char.IsLetterOrDigit));
        var folder = Path.Combine(_rootPath, safeSubFolder);
        Directory.CreateDirectory(folder);

        var ext = Path.GetExtension(originalFileName);
        var storedFileName = $"{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(folder, storedFileName);

        await using (var output = File.Create(fullPath))
        {
            fileStream.Position = 0;
            await fileStream.CopyToAsync(output);
        }

        var relativePath = Path.Combine(safeSubFolder, storedFileName).Replace("\\", "/");
        return (storedFileName, relativePath, new FileInfo(fullPath).Length, contentType);
    }

    public void DeleteFile(string filePath)
    {
        var fullPath = Path.Combine(_rootPath, filePath);
        if (File.Exists(fullPath)) File.Delete(fullPath);
    }
}
