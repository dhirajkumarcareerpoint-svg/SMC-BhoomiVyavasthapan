namespace SMC.Application.Interfaces;

public interface ISmsService
{
    Task SendAsync(string mobile, string message, string? dltTemplateId = null, CancellationToken cancellationToken = default);
}
