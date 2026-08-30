using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMC.Application.Interfaces;
using SMC.Application.Services;

namespace SMC.API.Controllers;

/// <summary>Excel/PDF अहवाल निर्यात.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AllStaff")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _service;

    public ReportsController(IReportService service)
    {
        _service = service;
    }

    private const string XlsxType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    [HttpGet("properties/excel")]
    [AllowAnonymous]
    public async Task<IActionResult> PropertiesExcel() =>
        File(await _service.ExportPropertiesExcelAsync(), XlsxType, "मालमत्ता-अहवाल.xlsx");

    [HttpGet("leases/excel")]
    [AllowAnonymous]
    public async Task<IActionResult> LeasesExcel() =>
        File(await _service.ExportLeasesExcelAsync(), XlsxType, "हस्तांतरण-अहवाल.xlsx");

    [HttpGet("recovery/excel")]
    [AllowAnonymous]
    public async Task<IActionResult> RecoveryExcel() =>
        File(await _service.ExportRecoveryExcelAsync(), XlsxType, "वसुली-अहवाल.xlsx");

    [HttpGet("audit/excel")]
    [AllowAnonymous]
    public async Task<IActionResult> AuditExcel() =>
        File(await _service.ExportAuditExcelAsync(), XlsxType, "audit-अहवाल.xlsx");

    [HttpGet("properties/pdf")]
    [AllowAnonymous]
    public async Task<IActionResult> PropertiesPdf() =>
        File(await _service.ExportPropertiesPdfAsync(), "application/pdf", "मालमत्ता-अहवाल.pdf");

    [HttpGet("recovery/pdf")]
    [AllowAnonymous]
    public async Task<IActionResult> RecoveryPdf() =>
        File(await _service.ExportRecoveryPdfAsync(), "application/pdf", "वसुली-अहवाल.pdf");
}
