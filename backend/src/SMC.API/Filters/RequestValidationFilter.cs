using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SMC.Application.DTOs;

namespace SMC.API.Filters;

public sealed class RequestValidationFilter : IActionFilter
{
    private static readonly HashSet<string> Required = new(StringComparer.OrdinalIgnoreCase)
    {
        "PropertyId", "Category", "PropertyCode", "Name", "Status", "LesseeName", "DeedNumber",
        "DeedDate", "DurationType", "StartDate", "EndDate", "Method", "PublishDate", "PeriodMonths",
        "CalculatedAmount", "TotalAmount", "CalculationDate", "MonthsOverdue", "OutstandingAmount",
        "Stage", "SchemeType", "ApplicantName", "Username", "Password", "FullName", "Role"
    };

    public void OnActionExecuting(ActionExecutingContext context)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null || !IsWriteDto(argument.GetType())) continue;
            Validate(argument, context.ModelState);
        }

        if (context.ModelState.IsValid) return;
        var errors = context.ModelState.Values.SelectMany(value => value.Errors)
            .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage) ? "कृपया वैध माहिती भरा." : error.ErrorMessage)
            .Distinct().ToList();
        context.Result = new BadRequestObjectResult(new { success = false, messageMr = "कृपया दाखवलेल्या त्रुटी दुरुस्त करा.", errors });
    }

    public void OnActionExecuted(ActionExecutedContext context) { }

    private static bool IsWriteDto(Type type) => type.Namespace == typeof(CreatePropertyDto).Namespace
        && (type.Name.StartsWith("Create", StringComparison.Ordinal) || type.Name.StartsWith("Update", StringComparison.Ordinal));

    private static void Validate(object dto, ModelStateDictionary modelState)
    {
        var properties = dto.GetType().GetProperties();
        foreach (var property in properties)
        {
            var value = property.GetValue(dto);
            var name = property.Name;
            if (Required.Contains(name) && IsEmpty(value))
                Add(modelState, name, "हे क्षेत्र आवश्यक आहे.");

            if (value is string text && text.Length > MaxLength(name))
                Add(modelState, name, $"कमाल {MaxLength(name)} अक्षरे परवानगी आहेत.");
            if (value is string stringValue && !string.IsNullOrWhiteSpace(stringValue))
            {
                if (name.Contains("Mobile", StringComparison.OrdinalIgnoreCase) && !System.Text.RegularExpressions.Regex.IsMatch(stringValue, "^\\d{10}$"))
                    Add(modelState, name, "भ्रमणध्वनी क्रमांक 10 अंकी असावा.");
                if (name.Equals("Email", StringComparison.OrdinalIgnoreCase) && !System.Text.RegularExpressions.Regex.IsMatch(stringValue, "^[^\\s@]+@[^\\s@]+\\.[^\\s@]+$"))
                    Add(modelState, name, "कृपया वैध ई-मेल भरा.");
                if (IsIdentifier(name) && !System.Text.RegularExpressions.Regex.IsMatch(stringValue, "^[A-Za-z0-9][A-Za-z0-9./_ -]*$"))
                    Add(modelState, name, "क्रमांकात अवैध अक्षरे आहेत.");
                if (IsEnumField(name) && !IsKnownEnum(dto, name, stringValue))
                    Add(modelState, name, "कृपया वैध पर्याय निवडा.");
            }
            if (value is decimal number && (number < 0 || number > 1_000_000_000_000m))
                Add(modelState, name, "ऋण किंवा मर्यादेपेक्षा जास्त संख्या मान्य नाही.");
            if (value is int integer && IsNumericField(name) && integer < 0)
                Add(modelState, name, "ऋण संख्या मान्य नाही.");
            if (value is DateTime date && (date.Year < 1900 || date.Year > 2099))
                Add(modelState, name, "कृपया वैध तारीख भरा.");
        }

        var dates = properties.Where(property => property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?))
            .ToDictionary(property => property.Name, property => property.GetValue(dto) as DateTime?);
        CheckOrder(dates, "StartDate", "EndDate", "समाप्ती तारीख सुरुवातीच्या तारखेनंतर असावी.", modelState);
        CheckOrder(dates, "PublishDate", "LastDateToApply", "अंतिम तारीख प्रसिद्धी तारखेनंतर असावी.", modelState);
        CheckOrder(dates, "ApplicationDate", "DecisionDate", "निर्णय तारीख अर्ज तारखेपूर्वी असू शकत नाही.", modelState);
    }

    private static void CheckOrder(Dictionary<string, DateTime?> dates, string first, string second, string message, ModelStateDictionary state)
    {
        if (dates.TryGetValue(first, out var start) && dates.TryGetValue(second, out var end) && start.HasValue && end.HasValue && end < start)
            Add(state, second, message);
    }

    private static bool IsEmpty(object? value) => value is null || value is string text && string.IsNullOrWhiteSpace(text) || value is int integer && integer <= 0;
    private static void Add(ModelStateDictionary state, string key, string message) => state.AddModelError(key, message);
    private static bool IsNumericField(string name) => name.Contains("Amount", StringComparison.OrdinalIgnoreCase) || name.Contains("Months", StringComparison.OrdinalIgnoreCase) || name.Contains("Area", StringComparison.OrdinalIgnoreCase) || name.Contains("Rent", StringComparison.OrdinalIgnoreCase) || name.Contains("Deposit", StringComparison.OrdinalIgnoreCase);
    private static bool IsIdentifier(string name) => name.Contains("Number", StringComparison.OrdinalIgnoreCase) || name.Contains("Code", StringComparison.OrdinalIgnoreCase) || name.Contains("Deed", StringComparison.OrdinalIgnoreCase) || name.Contains("Notice", StringComparison.OrdinalIgnoreCase) || name.Contains("Survey", StringComparison.OrdinalIgnoreCase) || name.Contains("Tp", StringComparison.OrdinalIgnoreCase);
    private static bool IsEnumField(string name) => name is "Category" or "Status" or "DurationType" or "Method" or "Stage" or "SchemeType" or "Role";
    private static int MaxLength(string name) => name.Contains("Shera", StringComparison.OrdinalIgnoreCase) || name.Contains("Address", StringComparison.OrdinalIgnoreCase) ? 2000 : name.Contains("Name", StringComparison.OrdinalIgnoreCase) ? 200 : 300;

    private static bool IsKnownEnum(object dto, string name, string value)
    {
        var enumName = (dto, name) switch
        {
            (CreatePropertyDto, "Category") or (UpdatePropertyDto, "Category") => "PropertyCategory",
            (CreatePropertyDto, "Status") or (UpdatePropertyDto, "Status") => "PropertyStatus",
            (CreateLeaseDto, "DurationType") or (UpdateLeaseDto, "DurationType") => "LeaseDurationType",
            (CreateLeaseDto, "Status") or (UpdateLeaseDto, "Status") => "LeaseStatus",
            (CreateAllocationProcessDto, "Method") or (UpdateAllocationProcessDto, "Method") => "AllocationMethod",
            (CreateAllocationProcessDto, "Status") or (UpdateAllocationProcessDto, "Status") => "AllocationStatus",
            (CreateCalculationDto, "Status") or (UpdateCalculationDto, "Status") => "CalculationStatus",
            (CreateRecoveryCaseDto, "Stage") or (UpdateRecoveryCaseDto, "Stage") => "RecoveryStage",
            (CreateSchemeApplicationDto, "SchemeType") or (UpdateSchemeApplicationDto, "SchemeType") => "SchemeType",
            (CreateSchemeApplicationDto, "Status") or (UpdateSchemeApplicationDto, "Status") => "SchemeStatus",
            (CreateUserDto, "Role") or (UpdateUserDto, "Role") => "UserRole",
            _ => null
        };
        if (enumName is null) return true;
        var enumType = typeof(SMC.Domain.Enums.PropertyCategory).Assembly.GetTypes().FirstOrDefault(type => type.Name == enumName && type.IsEnum);
        return enumType is not null && Enum.TryParse(enumType, value, ignoreCase: false, out _);
    }
}