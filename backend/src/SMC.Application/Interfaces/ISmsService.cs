namespace SMC.Application.Interfaces;

public interface ISmsService
{
    Task SendAsync(string mobile, string eventType, IReadOnlyDictionary<string, string?> values, string? applicationNumber = null, CancellationToken cancellationToken = default);
}
