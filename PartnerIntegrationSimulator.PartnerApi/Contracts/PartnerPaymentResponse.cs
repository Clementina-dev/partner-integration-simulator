namespace PartnerIntegrationSimulator.PartnerApi.Contracts;

public sealed record PartnerPaymentResponse(
    string Reference,
    string Status,
    string PartnerTransactionId
);