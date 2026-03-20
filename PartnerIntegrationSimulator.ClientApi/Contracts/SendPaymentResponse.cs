namespace PartnerIntegrationSimulator.ClientApi.Contracts;

public sealed record SendPaymentResponse(
    string Reference,
    string Status,
    string? PartnerTransactionId,
    string CorrelationId
);