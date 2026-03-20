namespace PartnerIntegrationSimulator.PartnerApi.Contracts
{
    public sealed record PartnerPaymentRequest(
    decimal Amount,
    string Currency,
    string Reference
    );
}
