using Microsoft.EntityFrameworkCore;
using SMC.Application.DTOs;
using SMC.Application.Interfaces;
using SMC.Domain.Entities;
using SMC.Domain.Enums;

namespace SMC.Application.Services;

public interface IDocumentService
{
    Task<List<DocumentDto>> GetByEntityAsync(string entityType, int entityId);
    Task<DocumentDto> UploadAsync(string entityType, int entityId, Stream fileStream, string fileName, string contentType, string uploadedBy);
    Task<Document?> GetEntityAsync(int documentId);
    Task<bool> DeleteAsync(int documentId, string deletedBy);
}

public class DocumentService : IDocumentService
{
    private readonly IApplicationDbContext _db;
    private readonly IFileStorageService _fileStorage;
    public DocumentService(IApplicationDbContext db, IFileStorageService fileStorage)
    {
        _db = db;
        _fileStorage = fileStorage;
    }

    private static DocumentDto ToDto(Document d) => new()
    {
        Id = d.Id, EntityType = d.EntityType.ToString(), EntityId = d.EntityId, FileName = d.FileName,
        FilePath = d.FilePath, ContentType = d.ContentType, FileSizeBytes = d.FileSizeBytes,
        UploadedBy = d.CreatedBy, UploadedAt = d.CreatedAt
    };

    public async Task<List<DocumentDto>> GetByEntityAsync(string entityType, int entityId)
    {
        var type = Enum.Parse<DocumentEntityType>(entityType);
        var docs = await _db.Documents.AsNoTracking().Where(d => d.EntityType == type && d.EntityId == entityId && !d.IsDeleted)
            .OrderByDescending(d => d.CreatedAt).ToListAsync();
        return docs.Select(ToDto).ToList();
    }

    public async Task<DocumentDto> UploadAsync(string entityType, int entityId, Stream fileStream, string fileName, string contentType, string uploadedBy)
    {
        if (!_fileStorage.IsAllowedFile(fileName, fileStream.Length))
            throw new InvalidOperationException("अनुज्ञेय नसलेला फाईल प्रकार किंवा आकार. फक्त PDF/JPG/PNG/DOCX (कमाल 10MB) परवानगी आहे.");

        var type = Enum.Parse<DocumentEntityType>(entityType);
        var subFolder = type.ToString().ToLower();
        var (storedFileName, filePath, size, ct) = await _fileStorage.SaveFileAsync(fileStream, fileName, contentType, subFolder);

        var doc = new Document
        {
            EntityType = type,
            EntityId = entityId,
            FileName = fileName,
            StoredFileName = storedFileName,
            FilePath = filePath,
            ContentType = ct,
            FileSizeBytes = size,
            CreatedBy = uploadedBy
        };

        switch (type)
        {
            case DocumentEntityType.Property: doc.PropertyId = entityId; break;
            case DocumentEntityType.Lease: doc.LeaseId = entityId; break;
            case DocumentEntityType.RecoveryCase: doc.RecoveryCaseId = entityId; break;
            case DocumentEntityType.Scheme: doc.SchemeApplicationId = entityId; break;
            case DocumentEntityType.Allocation: doc.AllocationProcessId = entityId; break;
            case DocumentEntityType.Calculation: doc.CalculationId = entityId; break;
        }

        _db.Documents.Add(doc);
        await _db.SaveChangesAsync();
        return ToDto(doc);
    }

    public async Task<Document?> GetEntityAsync(int documentId) =>
        await _db.Documents.FirstOrDefaultAsync(d => d.Id == documentId && !d.IsDeleted);

    public async Task<bool> DeleteAsync(int documentId, string deletedBy)
    {
        var doc = await _db.Documents.FirstOrDefaultAsync(d => d.Id == documentId && !d.IsDeleted);
        if (doc is null) return false;
        doc.IsDeleted = true;
        doc.DeletedBy = deletedBy;
        doc.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }
}
