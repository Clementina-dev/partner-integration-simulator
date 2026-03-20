using Microsoft.AspNetCore.Mvc;
using PartnerIntegrationSimulator.ClientApi.Contracts;
using PartnerIntegrationSimulator.ClientApi.Infrastructure;
using PartnerIntegrationSimulator.ClientApi.Services;

namespace PartnerIntegrationSimulator.ClientApi.Controllers;

[ApiController]
[Route("api/payments")]
public sealed class PaymentsController : ControllerBase
{
    private readonly PartnerPaymentsClient _client;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(PartnerPaymentsClient client, ILogger<PaymentsController> logger)
    {
        _client = client;
        _logger = logger;
    }

    [HttpPost("send")]
    public async Task<ActionResult<SendPaymentResponse>> Send([FromBody] SendPaymentRequest request, CancellationToken ct)
    {
        var correlationId = HttpContext.Items[CorrelationIdMiddleware.HeaderName]?.ToString()
                            ?? Guid.NewGuid().ToString("N");

        try
        {
            var resp = await _client.SendAsync(request, correlationId, ct);

            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogWarning("Partner returned non-success: {StatusCode} for reference {Reference} (CorrelationId: {CorrelationId})",
                    (int)resp.StatusCode, request.Reference, correlationId);

                return StatusCode((int)resp.StatusCode, new SendPaymentResponse(
                    Reference: request.Reference,
                    Status: "PARTNER_REJECTED",
                    PartnerTransactionId: null,
                    CorrelationId: correlationId
                ));
            }

            // Try extract partnerTransactionId (optional)
            var body = await resp.Content.ReadAsStringAsync(ct);
            return Ok(new SendPaymentResponse(
                Reference: request.Reference,
                Status: "SENT",
                PartnerTransactionId: ExtractPartnerTransactionId(body),
                CorrelationId: correlationId
            ));
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            // Http timeout
            _logger.LogWarning("Partner call timed out for reference {Reference} (CorrelationId: {CorrelationId})",
                request.Reference, correlationId);

            return StatusCode(504, new SendPaymentResponse(
                Reference: request.Reference,
                Status: "TIMEOUT",
                PartnerTransactionId: null,
                CorrelationId: correlationId
            ));
        }
    }

    private static string? ExtractPartnerTransactionId(string body)
    {
        // Keep it simple for MVP; we just avoid failing if the response changes.
        const string marker = "partnerTransactionId";
        if (!body.Contains(marker, StringComparison.OrdinalIgnoreCase)) return null;
        return null;
    }
}