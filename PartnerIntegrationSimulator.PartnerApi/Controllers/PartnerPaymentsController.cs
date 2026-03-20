using Microsoft.AspNetCore.Mvc;
using PartnerIntegrationSimulator.PartnerApi.Contracts;

namespace PartnerIntegrationSimulator.PartnerApi.Controllers;

[ApiController]
[Route("api/partner/payments")]
public sealed class PartnerPaymentsController : ControllerBase
{
    private static readonly Random Rng = new();

    [HttpPost]
    public async Task<ActionResult<PartnerPaymentResponse>> Create([FromBody] PartnerPaymentRequest request, CancellationToken ct)
    {
        // Simulate random partner behavior:
        //  - 60% success
        //  - 15% 400 (bad request)
        //  - 15% 500 (partner error)
        //  - 10% slow response (forces client timeouts)
        var roll = Rng.Next(1, 101);

        if (roll <= 10)
        {
            await Task.Delay(TimeSpan.FromSeconds(6), ct); // slow
        }

        if (roll <= 15)
        {
            return BadRequest(new { error = "Partner validation failed (simulated)." });
        }

        if (roll <= 30)
        {
            return StatusCode(500, new { error = "Partner internal error (simulated)." });
        }

        return Ok(new PartnerPaymentResponse(
            Reference: request.Reference,
            Status: "ACCEPTED",
            PartnerTransactionId: $"PTX-{Guid.NewGuid():N}"
        ));
    }
}