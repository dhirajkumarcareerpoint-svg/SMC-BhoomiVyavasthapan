using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SMC.Application.Interfaces;

namespace SMC.Application.Services;

/// <summary>मालमत्ता/हस्तांतरण/वसुली/थकबाकी/Audit इ. साठी Excel व PDF अहवाल तयार करते.</summary>
public interface IReportService
{
    Task<byte[]> ExportPropertiesExcelAsync();
    Task<byte[]> ExportLeasesExcelAsync();
    Task<byte[]> ExportRecoveryExcelAsync();
    Task<byte[]> ExportAuditExcelAsync();
    Task<byte[]> ExportPropertiesPdfAsync();
    Task<byte[]> ExportRecoveryPdfAsync();
}

public class ReportService : IReportService
{
    private readonly IApplicationDbContext _db;

    public ReportService(IApplicationDbContext db)
    {
        _db = db;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> ExportPropertiesExcelAsync()
    {
        var data = await _db.Properties.AsNoTracking().Where(p => !p.IsDeleted).OrderBy(p => p.Category).ThenBy(p => p.PropertyCode).ToListAsync();
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("मालमत्ता अहवाल");
        string[] headers = { "अ.क्र.", "विभाग", "मालमत्ता क्र.", "नाव", "प्रभाग", "पत्ता", "क्षेत्रफळ", "मासिक भाडे", "वार्षिक मागणी", "स्थिती", "सध्याचा धारक", "शेरा" };
        for (int i = 0; i < headers.Length; i++) ws.Cell(1, i + 1).Value = headers[i];
        ws.Row(1).Style.Font.Bold = true;
        ws.Row(1).Style.Fill.BackgroundColor = XLColor.FromHtml("#0B3D91");
        ws.Row(1).Style.Font.FontColor = XLColor.White;

        int row = 2;
        foreach (var p in data)
        {
            ws.Cell(row, 1).Value = row - 1;
            ws.Cell(row, 2).Value = p.Category.ToString();
            ws.Cell(row, 3).Value = p.PropertyCode;
            ws.Cell(row, 4).Value = p.Name;
            ws.Cell(row, 5).Value = p.Ward;
            ws.Cell(row, 6).Value = p.Address;
            ws.Cell(row, 7).Value = (double?)p.AreaSqFt;
            ws.Cell(row, 8).Value = (double)p.MonthlyRent;
            ws.Cell(row, 9).Value = (double)p.AnnualDemand;
            ws.Cell(row, 10).Value = p.Status.ToString();
            ws.Cell(row, 11).Value = p.CurrentOccupant;
            ws.Cell(row, 12).Value = p.Shera;
            row++;
        }
        ws.Columns().AdjustToContents();
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    public async Task<byte[]> ExportLeasesExcelAsync()
    {
        var data = await _db.Leases.AsNoTracking().Include(l => l.Property).Where(l => !l.IsDeleted).OrderByDescending(l => l.CreatedAt).ToListAsync();
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("हस्तांतरण अहवाल");
        string[] headers = { "अ.क्र.", "मालमत्ता", "भाडेकरू", "दस्त क्र.", "दस्त तारीख", "कालावधी प्रकार", "सुरुवात", "समाप्ती", "भाडे रक्कम", "स्थिती", "शेरा" };
        for (int i = 0; i < headers.Length; i++) ws.Cell(1, i + 1).Value = headers[i];
        ws.Row(1).Style.Font.Bold = true;
        int row = 2;
        foreach (var l in data)
        {
            ws.Cell(row, 1).Value = row - 1;
            ws.Cell(row, 2).Value = l.Property?.Name;
            ws.Cell(row, 3).Value = l.LesseeName;
            ws.Cell(row, 4).Value = l.DeedNumber;
            ws.Cell(row, 5).Value = l.DeedDate.ToString("dd-MM-yyyy");
            ws.Cell(row, 6).Value = l.DurationType.ToString();
            ws.Cell(row, 7).Value = l.StartDate.ToString("dd-MM-yyyy");
            ws.Cell(row, 8).Value = l.EndDate.ToString("dd-MM-yyyy");
            ws.Cell(row, 9).Value = (double)l.RentAmount;
            ws.Cell(row, 10).Value = l.Status.ToString();
            ws.Cell(row, 11).Value = l.Shera;
            row++;
        }
        ws.Columns().AdjustToContents();
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    public async Task<byte[]> ExportRecoveryExcelAsync()
    {
        var data = await _db.RecoveryCases.AsNoTracking().Include(r => r.Property).Where(r => !r.IsDeleted).OrderByDescending(r => r.CreatedAt).ToListAsync();
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("वसुली अहवाल");
        string[] headers = { "अ.क्र.", "मालमत्ता", "थकीत महिने", "थकबाकी रक्कम", "टप्पा", "नोटीस क्र.", "नोटीस तारीख", "वसूल रक्कम", "सील तारीख", "शेरा" };
        for (int i = 0; i < headers.Length; i++) ws.Cell(1, i + 1).Value = headers[i];
        ws.Row(1).Style.Font.Bold = true;
        int row = 2;
        foreach (var r in data)
        {
            ws.Cell(row, 1).Value = row - 1;
            ws.Cell(row, 2).Value = r.Property?.Name;
            ws.Cell(row, 3).Value = r.MonthsOverdue;
            ws.Cell(row, 4).Value = (double)r.OutstandingAmount;
            ws.Cell(row, 5).Value = r.Stage.ToString();
            ws.Cell(row, 6).Value = r.NoticeNumber;
            ws.Cell(row, 7).Value = r.NoticeDate?.ToString("dd-MM-yyyy");
            ws.Cell(row, 8).Value = (double)r.RecoveredAmount;
            ws.Cell(row, 9).Value = r.SealDate?.ToString("dd-MM-yyyy");
            ws.Cell(row, 10).Value = r.Shera;
            row++;
        }
        ws.Columns().AdjustToContents();
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    public async Task<byte[]> ExportAuditExcelAsync()
    {
        var data = await _db.AuditLogs.AsNoTracking().OrderByDescending(a => a.Timestamp).Take(5000).ToListAsync();
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Audit अहवाल");
        string[] headers = { "अ.क्र.", "User", "कृती", "Entity", "Entity Id", "Field", "जुनी value", "नवीन value", "तारीख-वेळ" };
        for (int i = 0; i < headers.Length; i++) ws.Cell(1, i + 1).Value = headers[i];
        ws.Row(1).Style.Font.Bold = true;
        int row = 2;
        foreach (var a in data)
        {
            ws.Cell(row, 1).Value = row - 1;
            ws.Cell(row, 2).Value = a.UserName;
            ws.Cell(row, 3).Value = a.Action;
            ws.Cell(row, 4).Value = a.EntityName;
            ws.Cell(row, 5).Value = a.EntityId;
            ws.Cell(row, 6).Value = a.FieldName;
            ws.Cell(row, 7).Value = a.OldValue;
            ws.Cell(row, 8).Value = a.NewValue;
            ws.Cell(row, 9).Value = a.Timestamp.ToString("dd-MM-yyyy HH:mm");
            row++;
        }
        ws.Columns().AdjustToContents();
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    public async Task<byte[]> ExportPropertiesPdfAsync()
    {
        var data = await _db.Properties.AsNoTracking().Where(p => !p.IsDeleted).OrderBy(p => p.Category).ToListAsync();
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(20);
                page.Header().Text("सोलापूर महानगरपालिका - मालमत्ता अहवाल").FontSize(16).Bold();
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.ConstantColumn(30); c.RelativeColumn(2); c.RelativeColumn(1.5f);
                        c.RelativeColumn(2); c.RelativeColumn(1.5f); c.RelativeColumn(1.5f); c.RelativeColumn(1.5f);
                    });
                    table.Header(h =>
                    {
                        foreach (var t in new[] { "क्र.", "विभाग", "मालमत्ता क्र.", "नाव", "वार्षिक मागणी", "स्थिती", "धारक" })
                            h.Cell().Background(Colors.Blue.Darken2).Padding(4).Text(t).FontColor(Colors.White).Bold();
                    });
                    int i = 1;
                    foreach (var p in data)
                    {
                        table.Cell().Padding(3).Text(i++.ToString());
                        table.Cell().Padding(3).Text(p.Category.ToString());
                        table.Cell().Padding(3).Text(p.PropertyCode);
                        table.Cell().Padding(3).Text(p.Name);
                        table.Cell().Padding(3).Text(p.AnnualDemand.ToString("N0"));
                        table.Cell().Padding(3).Text(p.Status.ToString());
                        table.Cell().Padding(3).Text(p.CurrentOccupant ?? "-");
                    }
                });
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("तयार दिनांक: ").FontSize(9);
                    x.Span(DateTime.Now.ToString("dd-MM-yyyy HH:mm")).FontSize(9);
                });
            });
        });
        return doc.GeneratePdf();
    }

    public async Task<byte[]> ExportRecoveryPdfAsync()
    {
        var data = await _db.RecoveryCases.AsNoTracking().Include(r => r.Property).Where(r => !r.IsDeleted).ToListAsync();
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(20);
                page.Header().Text("सोलापूर महानगरपालिका - वसुली अहवाल").FontSize(16).Bold();
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.ConstantColumn(30); c.RelativeColumn(2); c.RelativeColumn(1); c.RelativeColumn(1.5f);
                        c.RelativeColumn(1.5f); c.RelativeColumn(1.5f); c.RelativeColumn(1.5f);
                    });
                    table.Header(h =>
                    {
                        foreach (var t in new[] { "क्र.", "मालमत्ता", "थकीत म.", "थकबाकी", "टप्पा", "वसूल रक्कम", "नोटीस क्र." })
                            h.Cell().Background(Colors.Red.Darken2).Padding(4).Text(t).FontColor(Colors.White).Bold();
                    });
                    int i = 1;
                    foreach (var r in data)
                    {
                        table.Cell().Padding(3).Text(i++.ToString());
                        table.Cell().Padding(3).Text(r.Property?.Name ?? "-");
                        table.Cell().Padding(3).Text(r.MonthsOverdue.ToString());
                        table.Cell().Padding(3).Text(r.OutstandingAmount.ToString("N0"));
                        table.Cell().Padding(3).Text(r.Stage.ToString());
                        table.Cell().Padding(3).Text(r.RecoveredAmount.ToString("N0"));
                        table.Cell().Padding(3).Text(r.NoticeNumber ?? "-");
                    }
                });
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("तयार दिनांक: ").FontSize(9);
                    x.Span(DateTime.Now.ToString("dd-MM-yyyy HH:mm")).FontSize(9);
                });
            });
        });
        return doc.GeneratePdf();
    }
}
