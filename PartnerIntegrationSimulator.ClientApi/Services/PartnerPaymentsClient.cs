using System.Net.Http.Json;
using PartnerIntegrationSimulator.ClientApi.Contracts;

namespace PartnerIntegrationSimulator.ClientApi.Services;

public sealed class PartnerPaymentsClient
{
    private readonly HttpClient _http;

    public PartnerPaymentsClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<HttpResponseMessage> SendAsync(SendPaymentRequest req, string correlationId, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/partner/payments")
        {
            Content = JsonContent.Create(new
            {
                amount = req.Amount,
                currency = req.Currency,
                reference = req.Reference
            })
        };

        request.Headers.TryAddWithoutValidation("X-Correlation-Id", correlationId);
        return await _http.SendAsync(request, ct);
    }
}