namespace SMC.Application.Interfaces;

public interface IRazorpayGateway
{
    string KeyId { get; }
    Task<RazorpayOrderResult> CreateOrderAsync(string receipt, decimal amount, CancellationToken cancellationToken = default);
    bool VerifyPaymentSignature(string orderId, string paymentId, string signature);
}

public sealed record RazorpayOrderResult(string OrderId, int Amount, string Currency);