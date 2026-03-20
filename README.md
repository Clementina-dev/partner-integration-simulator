# Partner Integration Simulator

A two-service .NET 10 solution that simulates realistic partner payment API integration, demonstrating resilience patterns, structured logging, and correlation tracking.

## Architecture

```
┌─────────────────────┐         HTTP          ┌─────────────────────┐
│     ClientApi        │ ───────────────────►  │     PartnerApi      │
│  (your application)  │   Polly resilience    │ (simulated partner) │
│  localhost:5186      │   + correlation IDs   │  localhost:5046     │
└─────────────────────┘                        └─────────────────────┘
```

| Project | Description |
|---------|-------------|
| **ClientApi** | Client-facing REST API that accepts payment requests and forwards them to the partner, wrapped in Polly resilience policies. |
| **PartnerApi** | Simulated third-party partner that randomly returns successes, errors, and slow responses. |

## Features

- **Polly resilience policies** — retry (exponential back-off), circuit breaker, and timeout applied to outbound HTTP calls
- **Correlation IDs** — `X-Correlation-Id` header propagated across services via custom middleware
- **Structured logging** — Serilog with console sink and request logging
- **Simulated failure modes** — the partner API randomly produces:
  - ✅ 60% — `200 OK` (accepted)
  - ⚠️ 15% — `400 Bad Request` (validation failure)
  - 💥 15% — `500 Internal Server Error`
  - 🐢 10% — 6-second delay (triggers client timeout)

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### Run both services

Open two terminals from the repository root:

```bash
# Terminal 1 — start the partner (simulated third-party)
dotnet run --project PartnerIntegrationSimulator.PartnerApi

# Terminal 2 — start the client API
dotnet run --project PartnerIntegrationSimulator.ClientApi
```

### Send a test payment

```bash
curl -X POST http://localhost:5186/api/payments/send \
  -H "Content-Type: application/json" \
  -d '{"amount": 150.00, "currency": "USD", "reference": "INV-001"}'
```

The response will include a status (`SENT`, `PARTNER_REJECTED`, or `TIMEOUT`) and a `correlationId` for tracing across both services.

### Swagger UI

Both services expose Swagger for interactive exploration:

| Service | URL |
|---------|-----|
| ClientApi | http://localhost:5186/swagger |
| PartnerApi | http://localhost:5046/swagger |

## API Reference

### ClientApi — `POST /api/payments/send`

**Request body:**

```json
{
  "amount": 150.00,
  "currency": "USD",
  "reference": "INV-001"
}
```

**Response:**

```json
{
  "reference": "INV-001",
  "status": "SENT",
  "partnerTransactionId": "PTX-a1b2c3...",
  "correlationId": "d4e5f6..."
}
```

| Status | Meaning |
|--------|---------|
| `SENT` | Partner accepted the payment |
| `PARTNER_REJECTED` | Partner returned a non-success status code |
| `TIMEOUT` | Partner did not respond within the timeout window |

## Configuration

The ClientApi reads the partner base URL from configuration:

```json
{
  "PartnerApi": {
    "BaseUrl": "http://localhost:5071"
  }
}
```

Override via environment variable: `PartnerApi__BaseUrl`.

## Project Structure

```
├── PartnerIntegrationSimulator.ClientApi/
│   ├── Controllers/
│   │   └── PaymentsController.cs        # Payment endpoint
│   ├── Contracts/
│   │   ├── SendPaymentRequest.cs        # Inbound request DTO
│   │   └── SendPaymentResponse.cs       # Outbound response DTO
│   ├── Infrastructure/
│   │   └── CorrelationIdMiddleware.cs   # X-Correlation-Id propagation
│   ├── Services/
│   │   └── PartnerPaymentsClient.cs     # Typed HttpClient for partner calls
│   └── Program.cs                       # Host setup, Polly policies, Serilog
│
├── PartnerIntegrationSimulator.PartnerApi/
│   ├── Controllers/
│   │   └── PartnerPaymentsController.cs # Simulated partner endpoint
│   ├── Contracts/
│   │   ├── PartnerPaymentRequest.cs     # Partner request DTO
│   │   └── PartnerPaymentResponse.cs    # Partner response DTO
│   └── Program.cs                       # Host setup, Serilog
│
└── README.md
```

## License

This project is provided as-is for demonstration and learning purposes.
