using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SMC.Application.Interfaces;

namespace SMC.Infrastructure.Services;

public sealed class RazorpayOptions
{
    public bool Enabled { get; set; }
    public string KeyId { get; set; } = string.Empty;
    public string KeySecret { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.razorpay.com/v1";
}

public sealed class RazorpayGateway : IRazorpayGateway
{
    private readonly IHttpClientFactory _clients;
    private readonly RazorpayOptions _options;
    private readonly ILogger<RazorpayGateway> _logger;

    public RazorpayGateway(IHttpClientFactory clients, IOptions<RazorpayOptions> options, ILogger<RazorpayGateway> logger)
    {
        _clients = clients;
        _options = options.Value;
        _logger = logger;
    }

    public string KeyId => _options.KeyId;

    public async Task<RazorpayOrderResult> CreateOrderAsync(string receipt, decimal amount, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled || string.IsNullOrWhiteSpace(_options.KeyId) || string.IsNullOrWhiteSpace(_options.KeySecret))
            throw new InvalidOperationException("Razorpay is not configured.");

        var client = _clients.CreateClient("Razorpay");
        client.BaseAddress = new Uri(_options.BaseUrl.TrimEnd('/') + "/");
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(_options.KeyId + ":" + _options.KeySecret));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        var amountInPaise = checked((int)decimal.Round(amount * 100m, 0, MidpointRounding.AwayFromZero));
        using var content = new StringContent(JsonSerializer.Serialize(new { amount = amountInPaise, currency = "INR", receipt, payment_capture = 1 }), Encoding.UTF8, "application/json");
        using var response = await client.PostAsync("orders", content, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Razorpay order creation failed with HTTP {StatusCode}: {ResponseBody}", (int)response.StatusCode, body);
            throw new InvalidOperationException("Razorpay order creation failed.");
        }

        using var json = JsonDocument.Parse(body);
        var orderId = json.RootElement.GetProperty("id").GetString();
        if (string.IsNullOrWhiteSpace(orderId)) throw new InvalidOperationException("Razorpay returned an invalid order.");
        return new RazorpayOrderResult(orderId, amountInPaise, "INR");
    }

    public bool VerifyPaymentSignature(string orderId, string paymentId, string signature)
    {
        if (string.IsNullOrWhiteSpace(_options.KeySecret)) return false;
        var payload = Encoding.UTF8.GetBytes(orderId + "|" + paymentId);
        var secret = Encoding.UTF8.GetBytes(_options.KeySecret);
        var expected = Convert.ToHexString(HMACSHA256.HashData(secret, payload)).ToLowerInvariant();
        return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(expected), Encoding.UTF8.GetBytes(signature));
    }
}
