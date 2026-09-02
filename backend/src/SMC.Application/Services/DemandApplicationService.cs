using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using SMC.Application.DTOs;
using SMC.Application.Interfaces;
using SMC.Domain.Entities;
using SMC.Domain.Enums;

namespace SMC.Application.Services;

public interface IDemandApplicationService
{
    Task<List<DemandApplicationDto>> GetAllAsync();
    Task<DemandApplicationDto?> GetByIdAsync(int id);
    Task<DemandApplicationDto> CreateAsync(CreateDemandApplicationDto dto, string userName);
    Task<bool> UpdateAsync(int id, UpdateDemandApplicationDto dto, string userName);
    Task<bool> DeleteAsync(int id, string userName);
    Task<DemandApplicationDto?> SubmitAsync(int id, string userName);
    Task<DemandApplicationDocumentDto> AddDocumentAsync(int id, string type, Stream stream, string fileName, string contentType, string userName);
    Task<DemandApplicationDocumentDto> AddSiteInspectionPhotoAsync(int id, Stream stream, string fileName, string contentType, string userName);
    Task<bool> DeleteDocumentAsync(int id, int documentId, string userName);
    Task<DemandApplicationDocument?> GetDocumentAsync(int documentId);
    Task<DemandApplicationDocumentDto> SetDocumentVerificationAsync(int applicationId, int documentId, string status, string userName, string? remark = null);
    Task<PublicDemandApplicationSessionDto> CreatePublicAsync(CreateDemandApplicationDto dto);
    Task<DemandApplicationDto?> GetPublicAsync(int id, string? accessToken);
    Task<DemandApplicationDto?> UpdatePublicAsync(int id, UpdateDemandApplicationDto dto, string? accessToken);
    Task<DemandApplicationDto?> SubmitPublicAsync(int id, string? accessToken);
    Task<DemandApplicationDocumentDto?> AddPublicDocumentAsync(int id, string type, Stream stream, string fileName, string contentType, string? accessToken);
    Task<bool> DeletePublicDocumentAsync(int id, int documentId, string? accessToken);
    Task<DemandApplicationDocument?> GetPublicDocumentAsync(int id, int documentId, string? accessToken);
    Task<DemandApplicationDocumentDto?> ResubmitPublicDocumentAsync(int id, int documentId, Stream stream, string fileName, string contentType, string? requestToken);
}

public class DemandApplicationService : IDemandApplicationService
{
    private readonly IApplicationDbContext _db;
    private readonly IAuditService _audit;
    private readonly IFileStorageService _storage;
    public DemandApplicationService(IApplicationDbContext db, IAuditService audit, IFileStorageService storage) { _db = db; _audit = audit; _storage = storage; }

    private static DemandApplicationDto ToDto(DemandApplication x) => new()
    {
        Id=x.Id, ApplicationNumber=x.ApplicationNumber, ServiceType=x.ServiceType, BusinessType=x.BusinessType, OtherBusinessType=x.OtherBusinessType,
        ApplicantType=x.ApplicantType, ApplicantName=x.ApplicantName, Mobile=x.Mobile, Email=x.Email, IdentityNumber=x.IdentityNumber, PanNumber=x.PanNumber, GstNumber=x.GstNumber,
        PermanentAddress=x.PermanentAddress, CorrespondenceAddress=x.CorrespondenceAddress, SameAddress=x.SameAddress, State=x.State, District=x.District, City=x.City, Taluka=x.Taluka, PinCode=x.PinCode,
        Prabhag=x.Prabhag, Location=x.Location, AvailableSpace=x.AvailableSpace, AreaSqFt=x.AreaSqFt, LengthFt=x.LengthFt, WidthFt=x.WidthFt, CalculatedRate=x.CalculatedRate, ServiceDescription=x.ServiceDescription, SpaceRequirement=x.SpaceRequirement, OtherInformation=x.OtherInformation,
        StartDate=x.StartDate, EndDate=x.EndDate, RequiredDuration=x.RequiredDuration, ElectricityRequired=x.ElectricityRequired, WaterRequired=x.WaterRequired, OtherFacilities=x.OtherFacilities, WasteManagement=x.WasteManagement,
        DeclarationAccepted=x.DeclarationAccepted, FeeAmount=x.FeeAmount, PaymentStatus=x.PaymentStatus, Status=x.Status, CreatedAt=x.CreatedAt, SubmittedAt=x.SubmittedAt,
        Documents=x.Documents.Where(d=>!d.IsDeleted).Select(d=>new DemandApplicationDocumentDto { Id=d.Id, DocumentType=d.DocumentType, FileName=d.FileName, ContentType=d.ContentType, FileSizeBytes=d.FileSizeBytes, UploadedAt=d.CreatedAt, VerificationStatus=d.VerificationStatus, RequestRemark=d.RequestRemark, RequestedAt=d.RequestedAt, RespondedAt=d.RespondedAt }).ToList()
    };
    private static DemandApplicationDto ToPublicDto(DemandApplication x)
    {
        var dto = ToDto(x);
        dto.Documents = dto.Documents.Where(d => !string.Equals(d.DocumentType, "SiteInspectionPhoto", StringComparison.Ordinal)).ToList();
        return dto;
    }

    public async Task<List<DemandApplicationDto>> GetAllAsync()
    {
        var applications = await _db.DemandApplications.AsNoTracking().Include(x => x.Documents).Where(x => !x.IsDeleted).OrderByDescending(x => x.CreatedAt).ToListAsync();
        var applicationIds = applications.Select(x => x.Id).ToList();
        var workflows = await _db.DemandApplicationWorkflows.AsNoTracking()
            .Where(x => applicationIds.Contains(x.DemandApplicationId))
            .Select(x => new { x.DemandApplicationId, x.Stage, x.PaymentStatus })
            .ToDictionaryAsync(x => x.DemandApplicationId);
        return applications.Select(x =>
        {
            var dto = ToDto(x);
            if (workflows.TryGetValue(x.Id, out var workflow))
            {
                dto.WorkflowStage = workflow.Stage;
                dto.WorkflowPaymentStatus = workflow.PaymentStatus;
            }
            return dto;
        }).ToList();
    }
    public async Task<DemandApplicationDto?> GetByIdAsync(int id) { var x=await _db.DemandApplications.AsNoTracking().Include(x=>x.Documents).FirstOrDefaultAsync(x=>x.Id==id&&!x.IsDeleted); return x is null?null:ToDto(x); }
    public async Task<DemandApplicationDto> CreateAsync(CreateDemandApplicationDto dto, string userName) { Validate(dto); var x=Map(new DemandApplication(),dto); x.ApplicationNumber="DRAFT-"+Guid.NewGuid().ToString("N")[..8].ToUpperInvariant(); x.CreatedBy=userName; _db.DemandApplications.Add(x); await _db.SaveChangesAsync(); await _audit.LogAsync("Create",nameof(DemandApplication),x.Id); return ToDto(x); }
    public async Task<PublicDemandApplicationSessionDto> CreatePublicAsync(CreateDemandApplicationDto dto)
    {
        Validate(dto);
        var accessToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var x = Map(new DemandApplication(), dto);
        x.ApplicationNumber = "DRAFT-" + Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        x.ApplicantAccessTokenHash = HashToken(accessToken);
        x.CreatedBy = "Applicant";
        _db.DemandApplications.Add(x);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("Create", nameof(DemandApplication), x.Id);
        return new PublicDemandApplicationSessionDto { Application = ToPublicDto(x), AccessToken = accessToken };
    }
    public async Task<DemandApplicationDto?> GetPublicAsync(int id, string? accessToken)
    {
        var x = await GetPublicEntityAsync(id, accessToken, includeDocuments: true);
        return x is null ? null : ToPublicDto(x);
    }
    public async Task<DemandApplicationDto?> UpdatePublicAsync(int id, UpdateDemandApplicationDto dto, string? accessToken)
    {
        Validate(dto);
        var x = await GetPublicEntityAsync(id, accessToken, includeDocuments: true);
        if (x is null || x.SubmittedAt is not null) return null;
        Map(x, dto); x.UpdatedBy = "Applicant"; x.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(); await _audit.LogAsync("Update", nameof(DemandApplication), id);
        return ToPublicDto(x);
    }
    public async Task<DemandApplicationDto?> SubmitPublicAsync(int id, string? accessToken)
    {
        var x = await GetPublicEntityAsync(id, accessToken, includeDocuments: true);
        if (x is null || x.SubmittedAt is not null) return null;
        if (!x.DeclarationAccepted) throw new InvalidOperationException("घोषणा स्वीकारणे आवश्यक आहे.");
        var number = ""; do { number = $"2026{Random.Shared.Next(0,10000):0000}"; } while (await _db.DemandApplications.AnyAsync(a => a.ApplicationNumber == number && a.Id != id));
        x.ApplicationNumber = number; x.Status = DemandApplicationStatus.Submitted; x.SubmittedAt = DateTime.UtcNow; x.UpdatedBy = "Applicant"; x.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(); await _audit.LogAsync("Final Submission", nameof(DemandApplication), id);
        return ToPublicDto(x);
    }
    public async Task<DemandApplicationDocumentDto?> AddPublicDocumentAsync(int id, string type, Stream stream, string fileName, string contentType, string? accessToken)
    {
        var x = await GetPublicEntityAsync(id, accessToken);
        if (x is null || x.SubmittedAt is not null) return null;
        return await AddDocumentAsync(id, type, stream, fileName, contentType, "Applicant");
    }
    public async Task<bool> DeletePublicDocumentAsync(int id, int documentId, string? accessToken)
    {
        var x = await GetPublicEntityAsync(id, accessToken);
        if (x is null || x.SubmittedAt is not null) return false;
        return await DeleteDocumentAsync(id, documentId, "Applicant");
    }
    public async Task<DemandApplicationDocument?> GetPublicDocumentAsync(int id, int documentId, string? accessToken)
    {
        var x = await GetPublicEntityAsync(id, accessToken);
        if (x is null) return null;
        return await _db.DemandApplicationDocuments.AsNoTracking().FirstOrDefaultAsync(d => d.Id == documentId && d.DemandApplicationId == id && d.DocumentType != "SiteInspectionPhoto" && !d.IsDeleted);
    }
    public async Task<DemandApplicationDocumentDto?> ResubmitPublicDocumentAsync(int id, int documentId, Stream stream, string fileName, string contentType, string? requestToken)
    {
        var request = await _db.DemandApplicationDocuments.FirstOrDefaultAsync(d => d.Id == documentId && d.DemandApplicationId == id && !d.IsDeleted);
        if (request is null || request.VerificationStatus != "Requested" || request.RequestTokenConsumedAt is not null || string.IsNullOrWhiteSpace(requestToken) || !TokenMatches(request.RequestTokenHash, requestToken) || !IsValidApplicantDocument(fileName, contentType, stream.Length)) return null;
        request.VerificationStatus = "Responded"; request.RespondedAt = DateTime.UtcNow; request.RequestTokenConsumedAt = DateTime.UtcNow;
        var saved = await _storage.SaveFileAsync(stream, fileName, contentType, "demandapplications");
        var replacement = new DemandApplicationDocument { DemandApplicationId=id, DocumentType=request.DocumentType, FileName=fileName, StoredFileName=saved.storedFileName, FilePath=saved.filePath, ContentType=saved.contentType, FileSizeBytes=saved.size, VerificationStatus="Unchecked", CreatedBy="Applicant" };
        _db.DemandApplicationDocuments.Add(replacement); await _db.SaveChangesAsync(); await _audit.LogAsync("Applicant resubmitted requested document", nameof(DemandApplication), id, null, null, request.DocumentType);
        return new DemandApplicationDocumentDto { Id=replacement.Id, DocumentType=replacement.DocumentType, FileName=replacement.FileName, ContentType=replacement.ContentType, FileSizeBytes=replacement.FileSizeBytes, UploadedAt=replacement.CreatedAt, VerificationStatus=replacement.VerificationStatus };
    }
    public async Task<bool> UpdateAsync(int id, UpdateDemandApplicationDto dto, string userName) { Validate(dto); var x=await _db.DemandApplications.FirstOrDefaultAsync(x=>x.Id==id&&!x.IsDeleted); if(x is null)return false; Map(x,dto); x.UpdatedBy=userName; x.UpdatedAt=DateTime.UtcNow; await _db.SaveChangesAsync(); await _audit.LogAsync("Update",nameof(DemandApplication),id); return true; }
    public async Task<bool> DeleteAsync(int id, string userName)
    {
        var application = await _db.DemandApplications
            .Include(x => x.Documents)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (application is null) return false;

        var deletedAt = DateTime.UtcNow;
        application.IsDeleted = true;
        application.DeletedBy = userName;
        application.DeletedAt = deletedAt;

        foreach (var document in application.Documents.Where(x => !x.IsDeleted))
        {
            document.IsDeleted = true;
            document.DeletedBy = userName;
            document.DeletedAt = deletedAt;
        }

        var workflow = await _db.DemandApplicationWorkflows
            .FirstOrDefaultAsync(x => x.DemandApplicationId == id && !x.IsDeleted);
        if (workflow is not null)
        {
            workflow.IsDeleted = true;
            workflow.DeletedBy = userName;
            workflow.DeletedAt = deletedAt;
        }

        // One SaveChanges call keeps the parent and all related soft-delete
        // markers atomic while retaining the records required for audit history.
        await _db.SaveChangesAsync();
        await _audit.LogAsync("Delete", nameof(DemandApplication), id, null, null, "Officer permanently deleted application");
        return true;
    }
    public async Task<DemandApplicationDto?> SubmitAsync(int id,string userName){var x=await _db.DemandApplications.Include(x=>x.Documents).FirstOrDefaultAsync(x=>x.Id==id&&!x.IsDeleted);if(x is null)return null;if(!x.DeclarationAccepted)throw new InvalidOperationException("घोषणा स्वीकारणे आवश्यक आहे.");var number="";do{number=$"2026{Random.Shared.Next(0,10000):0000}";}while(await _db.DemandApplications.AnyAsync(a=>a.ApplicationNumber==number&&a.Id!=id));x.ApplicationNumber=number;x.Status=DemandApplicationStatus.Submitted;x.SubmittedAt=DateTime.UtcNow;x.UpdatedBy=userName;x.UpdatedAt=DateTime.UtcNow;await _db.SaveChangesAsync();await _audit.LogAsync("Final Submission",nameof(DemandApplication),id);return ToDto(x);}
    public async Task<DemandApplicationDocumentDto> AddDocumentAsync(int id,string type,Stream stream,string fileName,string contentType,string userName){var x=await _db.DemandApplications.FirstOrDefaultAsync(x=>x.Id==id&&!x.IsDeleted)??throw new InvalidOperationException("अर्ज सापडला नाही.");if(!IsValidApplicantDocument(fileName,contentType,stream.Length))throw new InvalidOperationException("फक्त PDF किंवा DOCX स्वरूपातील कागदपत्र (कमाल 5 MB) परवानगी आहे.");var previous=await _db.DemandApplicationDocuments.Where(d=>d.DemandApplicationId==id&&d.DocumentType==type&&!d.IsDeleted).ToListAsync();foreach(var old in previous){old.IsDeleted=true;old.DeletedBy=userName;old.DeletedAt=DateTime.UtcNow;_storage.DeleteFile(old.FilePath);}var saved=await _storage.SaveFileAsync(stream,fileName,contentType,"demandapplications");var d=new DemandApplicationDocument{DemandApplicationId=x.Id,DocumentType=type,FileName=fileName,StoredFileName=saved.storedFileName,FilePath=saved.filePath,ContentType=saved.contentType,FileSizeBytes=saved.size,CreatedBy=userName};_db.DemandApplicationDocuments.Add(d);await _db.SaveChangesAsync();await _audit.LogAsync(previous.Count>0?"Document Replace":"Document Upload",nameof(DemandApplication),id,null,null,type);return new DemandApplicationDocumentDto{Id=d.Id,DocumentType=d.DocumentType,FileName=d.FileName,ContentType=d.ContentType,FileSizeBytes=d.FileSizeBytes,UploadedAt=d.CreatedAt};}
    public async Task<DemandApplicationDocumentDto> SetDocumentVerificationAsync(int applicationId, int documentId, string status, string userName, string? remark = null)
    {
        if (status is not ("Checked" or "Unchecked" or "Requested")) throw new InvalidOperationException("अवैध कागदपत्र स्थिती.");
        var document = await _db.DemandApplicationDocuments.FirstOrDefaultAsync(d => d.Id == documentId && d.DemandApplicationId == applicationId && !d.IsDeleted) ?? throw new InvalidOperationException("कागदपत्र सापडले नाही.");
        if (status == "Requested" && (string.IsNullOrWhiteSpace(remark) || document.VerificationStatus == "Requested")) throw new InvalidOperationException("विनंती शेरा आवश्यक आहे किंवा विनंती आधीच प्रलंबित आहे.");
        string? requestToken = null;
        if (status == "Requested") { requestToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)); document.RequestTokenHash = HashToken(requestToken); document.RequestTokenCreatedAt = DateTime.UtcNow; document.RequestTokenConsumedAt = null; }
        document.VerificationStatus=status; document.RequestRemark=status=="Requested" ? remark.Trim() : null; document.RequestedAt=status=="Requested" ? DateTime.UtcNow : null; document.RequestedBy=status=="Requested" ? userName : null;
        await _db.SaveChangesAsync(); await _audit.LogAsync(status=="Requested" ? "OS requested document" : "OS verified document", nameof(DemandApplication), applicationId, null, null, document.DocumentType);
        return new DemandApplicationDocumentDto { Id=document.Id, DocumentType=document.DocumentType, FileName=document.FileName, ContentType=document.ContentType, FileSizeBytes=document.FileSizeBytes, UploadedAt=document.CreatedAt, VerificationStatus=document.VerificationStatus, RequestRemark=document.RequestRemark, RequestedAt=document.RequestedAt, RespondedAt=document.RespondedAt, RequestToken=requestToken };
    }
    public async Task<DemandApplicationDocumentDto> AddSiteInspectionPhotoAsync(int id, Stream stream, string fileName, string contentType, string userName)
    {
        var application = await _db.DemandApplications.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted) ?? throw new InvalidOperationException("अर्ज सापडला नाही.");
        var workflow = await _db.DemandApplicationWorkflows.FirstOrDefaultAsync(x => x.DemandApplicationId == id);
        if (workflow is null || !new[] { "OSPending", "PaymentRequired", "PaymentVerificationPending" }.Contains(workflow.Stage))
            throw new InvalidOperationException("या टप्प्यावर प्रत्यक्ष जागेचा फोटो बदलता येत नाही.");
        if (!IsValidSitePhoto(fileName, contentType, stream.Length)) throw new InvalidOperationException("फक्त JPG किंवा PNG प्रतिमा (कमाल 10 MB) परवानगी आहे.");
        var previous = await _db.DemandApplicationDocuments.Where(d => d.DemandApplicationId == id && d.DocumentType == "SiteInspectionPhoto" && !d.IsDeleted).ToListAsync();
        foreach (var old in previous) { old.IsDeleted = true; old.DeletedBy = userName; old.DeletedAt = DateTime.UtcNow; _storage.DeleteFile(old.FilePath); }
        var saved = await _storage.SaveFileAsync(stream, fileName, contentType, "siteinspection");
        var photo = new DemandApplicationDocument { DemandApplicationId = application.Id, DocumentType = "SiteInspectionPhoto", FileName = fileName, StoredFileName = saved.storedFileName, FilePath = saved.filePath, ContentType = saved.contentType, FileSizeBytes = saved.size, CreatedBy = userName };
        _db.DemandApplicationDocuments.Add(photo);
        await _db.SaveChangesAsync();
        await _audit.LogAsync(previous.Count > 0 ? "Site inspection photo replaced" : "Site inspection photo uploaded", nameof(DemandApplication), id);
        return new DemandApplicationDocumentDto { Id = photo.Id, DocumentType = photo.DocumentType, FileName = photo.FileName, ContentType = photo.ContentType, FileSizeBytes = photo.FileSizeBytes, UploadedAt = photo.CreatedAt };
    }
    private static bool IsValidApplicantDocument(string fileName, string contentType, long length) => length is > 0 and <= 5 * 1024 * 1024 && ((fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) && string.Equals(contentType, "application/pdf", StringComparison.OrdinalIgnoreCase)) || (fileName.EndsWith(".docx", StringComparison.OrdinalIgnoreCase) && string.Equals(contentType, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", StringComparison.OrdinalIgnoreCase)));
    private static bool IsValidSitePhoto(string fileName, string contentType, long length) => length is > 0 and <= 10 * 1024 * 1024 && (fileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || fileName.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) || fileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) && (string.Equals(contentType, "image/jpeg", StringComparison.OrdinalIgnoreCase) || string.Equals(contentType, "image/png", StringComparison.OrdinalIgnoreCase));
    public async Task<bool> DeleteDocumentAsync(int id,int documentId,string userName){var d=await _db.DemandApplicationDocuments.FirstOrDefaultAsync(x=>x.Id==documentId&&x.DemandApplicationId==id&&!x.IsDeleted);if(d is null)return false;d.IsDeleted=true;d.DeletedBy=userName;d.DeletedAt=DateTime.UtcNow;await _db.SaveChangesAsync();_storage.DeleteFile(d.FilePath);await _audit.LogAsync("Document Delete",nameof(DemandApplication),id,null,null,d.FileName);return true;}
    public async Task<DemandApplicationDocument?> GetDocumentAsync(int documentId)=>await _db.DemandApplicationDocuments.AsNoTracking().FirstOrDefaultAsync(x=>x.Id==documentId&&!x.IsDeleted);
    private async Task<DemandApplication?> GetPublicEntityAsync(int id, string? accessToken, bool includeDocuments = false)
    {
        if (string.IsNullOrWhiteSpace(accessToken)) return null;
        IQueryable<DemandApplication> query = _db.DemandApplications;
        if (includeDocuments) query = query.Include(x => x.Documents);
        var x = await query.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        return x is not null && TokenMatches(x.ApplicantAccessTokenHash, accessToken) ? x : null;
    }
    private static string HashToken(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    private static bool TokenMatches(string? storedHash, string suppliedToken)
    {
        if (string.IsNullOrWhiteSpace(storedHash)) return false;
        var expected = Convert.FromHexString(storedHash);
        var actual = SHA256.HashData(Encoding.UTF8.GetBytes(suppliedToken));
        return CryptographicOperations.FixedTimeEquals(expected, actual);
    }
    private static DemandApplication Map(DemandApplication x,CreateDemandApplicationDto d){x.ServiceType=d.ServiceType;x.BusinessType=d.BusinessType;x.OtherBusinessType=(d.OtherBusinessType??string.Empty).Trim();x.ApplicantType=d.ApplicantType;x.ApplicantName=(d.ApplicantName??string.Empty).Trim();x.Mobile=d.Mobile;x.Email=(d.Email??string.Empty).Trim();x.IdentityNumber=(d.IdentityNumber??string.Empty).Trim();x.PanNumber=(d.PanNumber??string.Empty).Trim();x.GstNumber=(d.GstNumber??string.Empty).Trim();x.PermanentAddress=d.PermanentAddress;x.CorrespondenceAddress=d.SameAddress?d.PermanentAddress:d.CorrespondenceAddress;x.SameAddress=d.SameAddress;x.State=d.State;x.District=d.District;x.City=d.City;x.Taluka=d.Taluka;x.PinCode=d.PinCode;x.Prabhag=d.Prabhag;x.Location=d.Location??string.Empty;x.AvailableSpace=d.AvailableSpace??string.Empty;if(d.LengthFt is > 0 && d.WidthFt is > 0){var area=decimal.Round(d.LengthFt.Value*d.WidthFt.Value,2,MidpointRounding.AwayFromZero);x.LengthFt=d.LengthFt;x.WidthFt=d.WidthFt;x.AreaSqFt=area;x.CalculatedRate=area;}else{x.LengthFt=null;x.WidthFt=null;x.AreaSqFt=d.AreaSqFt;x.CalculatedRate=d.CalculatedRate;}x.ServiceDescription=d.ServiceDescription;x.SpaceRequirement=d.SpaceRequirement;x.OtherInformation=d.OtherInformation;x.StartDate=d.StartDate;x.EndDate=d.EndDate;x.RequiredDuration=d.RequiredDuration;x.ElectricityRequired=d.ElectricityRequired;x.WaterRequired=d.WaterRequired;x.OtherFacilities=d.OtherFacilities;x.WasteManagement=d.WasteManagement;x.DeclarationAccepted=d.DeclarationAccepted;return x;}
    private static void Validate(CreateDemandApplicationDto d)
    {
        // ===== STEP 1: SERVICE INFORMATION =====
        if (d.ServiceType == 0)
            throw new InvalidOperationException("सेवेचा प्रकार निवडणे आवश्यक आहे.");
        
        // Conditional: If "विविध व्यवसायासाठी जागा मागणी" (VariousBusinessSpace) is selected
        if (d.ServiceType == DemandServiceType.VariousBusinessSpace)
        {
            if (d.BusinessType == null || d.BusinessType == 0)
                throw new InvalidOperationException("व्यवसायाचा प्रकार निवडणे आवश्यक आहे.");
            
            // If "Other" is selected, OtherBusinessType must be provided and non-empty after trim
            if (d.BusinessType == DemandBusinessType.Other && string.IsNullOrWhiteSpace(d.OtherBusinessType))
                throw new InvalidOperationException("कृपया व्यवसायाचा प्रकार प्रविष्ट करा.");
        }
        
        // ===== STEP 2: APPLICANT INFORMATION =====
        // Mandatory applicant fields: name and mobile only. Other identity/contact
        // fields are intentionally optional for the demand-application workflow.
        if (string.IsNullOrWhiteSpace(d.ApplicantName))
            throw new InvalidOperationException("कृपया अर्जदाराचे पूर्ण नाव प्रविष्ट करा.");
        
        if (d.ApplicantName.Trim().Length > 100)
            throw new InvalidOperationException("नाव 100 अक्षरांपेक्षा जास्त असू शकत नाही.");
        
        // Mandatory: Mobile
        if (string.IsNullOrWhiteSpace(d.Mobile))
            throw new InvalidOperationException("कृपया 10 अंकी मोबाईल क्रमांक प्रविष्ट करा.");
        
        if (!Regex.IsMatch(d.Mobile, "^\\d{10}$"))
            throw new InvalidOperationException("कृपया 10 अंकी मोबाईल क्रमांक प्रविष्ट करा.");
        
        // Optional: Email (only validate if provided)
        var email = d.Email?.Trim();
        if (!string.IsNullOrWhiteSpace(email) && !Regex.IsMatch(email, "^[^\\s@]+@[^\\s@]+\\.[^\\s@]+$"))
            throw new InvalidOperationException("कृपया वैध ई-मेल आयडी प्रविष्ट करा.");
        
        // Optional: PAN (only validate if provided)
        if (!string.IsNullOrWhiteSpace(d.PanNumber) && !Regex.IsMatch(d.PanNumber, "^[A-Z]{5}[0-9]{4}[A-Z]$"))
            throw new InvalidOperationException("कृपया वैध PAN क्रमांक प्रविष्ट करा.");
        
        // Optional: GST (only validate if provided)
        if (!string.IsNullOrWhiteSpace(d.GstNumber) && !Regex.IsMatch(d.GstNumber, "^[0-9A-Z]{15}$"))
            throw new InvalidOperationException("कृपया वैध GST क्रमांक प्रविष्ट करा.");
        
        // Optional: Aadhaar/IdentityNumber (only validate if provided - 12-16 digits)
        if (!string.IsNullOrWhiteSpace(d.IdentityNumber) && !Regex.IsMatch(d.IdentityNumber, "^\\d{12,16}$"))
            throw new InvalidOperationException("कृपया वैध आधार/ओळखपत्र क्रमांक प्रविष्ट करा.");
        
        // ===== STEP 3: ADDRESS AND LOCATION =====
        if (string.IsNullOrWhiteSpace(d.PermanentAddress))
            throw new InvalidOperationException("कृपया कायमचा पत्ता प्रविष्ट करा.");
        
        if (string.IsNullOrWhiteSpace(d.CorrespondenceAddress))
            throw new InvalidOperationException("कृपया पत्रव्यवहाराचा पत्ता प्रविष्ट करा.");
        
        if (string.IsNullOrWhiteSpace(d.State))
            throw new InvalidOperationException("कृपया राज्य निवडा.");
        
        if (string.IsNullOrWhiteSpace(d.District))
            throw new InvalidOperationException("कृपया जिल्हा निवडा.");
        
        if (string.IsNullOrWhiteSpace(d.City))
            throw new InvalidOperationException("कृपया शहर निवडा.");
        
        if (string.IsNullOrWhiteSpace(d.Taluka))
            throw new InvalidOperationException("कृपया तालुका निवडा.");
        
        if (string.IsNullOrWhiteSpace(d.PinCode))
            throw new InvalidOperationException("कृपया पिनकोड प्रविष्ट करा.");
        
        if (!Regex.IsMatch(d.PinCode, "^\\d{6}$"))
            throw new InvalidOperationException("कृपया वैध 6 अंकी पिनकोड प्रविष्ट करा.");
        
        if (string.IsNullOrWhiteSpace(d.Prabhag))
            throw new InvalidOperationException("कृपया प्रभाग निवडा.");
        
        if (string.IsNullOrWhiteSpace(d.ServiceDescription))
            throw new InvalidOperationException("कृपया सेवा/विक्रीचा प्रकार प्रविष्ट करा.");
        
        if (string.IsNullOrWhiteSpace(d.SpaceRequirement))
            throw new InvalidOperationException("कृपया स्टॉल/जागेची आवश्यकता प्रविष्ट करा.");
        
        // ===== STEP 4: DURATION AND DATES =====
        if (string.IsNullOrWhiteSpace(d.RequiredDuration))
            throw new InvalidOperationException("कृपया आवश्यक कालावधी प्रविष्ट करा.");
        
        // Date validation
        if (d.EndDate < d.StartDate)
            throw new InvalidOperationException("समाप्ती तारीख प्रारंभ तारखेनंतर असावी.");

        var expectedDuration = (d.EndDate.Date - d.StartDate.Date).Days + 1;
        if (!int.TryParse(d.RequiredDuration, out var actualDuration) || actualDuration != expectedDuration)
            throw new InvalidOperationException("आवश्यक कालावधी निवडलेल्या तारखांशी जुळत नाही.");
        
        // Numeric validations
        if (d.LengthFt.HasValue || d.WidthFt.HasValue)
        {
            if (d.LengthFt is not > 0 || d.WidthFt is not > 0) throw new InvalidOperationException("लांबी आणि रुंदी शून्यापेक्षा जास्त असणे आवश्यक आहे.");
        }
        if (d.AreaSqFt is < 0)
            throw new InvalidOperationException("क्षेत्रफळ ऋण असू शकत नाही.");
    }
}
