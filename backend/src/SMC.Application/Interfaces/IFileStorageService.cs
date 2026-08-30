namespace SMC.Application.Interfaces;

public interface IFileStorageService
{
    /// <summary>सुरक्षित file upload: extension/size validation करून disk वर साठवते.</summary>
    Task<(string storedFileName, string filePath, long size, string contentType)> SaveFileAsync(
        Stream fileStream, string originalFileName, string contentType, string subFolder);

    void DeleteFile(string filePath);
    bool IsAllowedFile(string fileName, long fileSize);
}
