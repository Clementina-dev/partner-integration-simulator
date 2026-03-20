namespace PartnerIntegrationSimulator.ClientApi.Contracts;

public sealed record SendPaymentRequest(
    decimal Amount,
    string Currency,
    string Reference
);