# Fahad Hassan | PulseSend

PulseSend is a .NET 9 REST API for selecting a customer and message template, rendering an SMS, submitting it through a configurable SMS gateway, and persisting the request and provider response.

## Requirements

- .NET SDK 9.0
- macOS, Windows, or Linux
- Internet access for the first NuGet restore

SQLite is the default database. SQL Server can be configured when a reachable SQL Server instance is available.

## Run locally

From Terminal, change to the extracted project directory and run:

```bash
cd FahadHassan.PulseSend
dotnet restore FahadHassan.PulseSend.sln
dotnet build FahadHassan.PulseSend.sln
dotnet test FahadHassan.PulseSend.sln
dotnet run --project src/FahadHassan.PulseSend.Api
```

Open the HTTP URL printed by the application, normally `http://localhost:5080/swagger`. The database is created and seeded on first launch. The seed data is fictional and for local demonstration only.

If the local HTTPS developer certificate is missing, run `dotnet dev-certs https --trust` and restart the application.

## Demonstrate the API in mock mode

Mock mode is enabled by default and does not contact a provider or send a real SMS.

Send a reminder:

```bash
curl -X POST http://localhost:5080/api/sms/send \
  -H 'Content-Type: application/json' \
  -d '{"creditNumber":"CR-10001","templateName":"Erinnerung"}'
```

List templates:

```bash
curl http://localhost:5080/api/templates
```

Get a customer:

```bash
curl http://localhost:5080/api/customers/CR-10001
```

List SMS logs for a customer:

```bash
curl 'http://localhost:5080/api/sms?creditNumber=CR-10001'
```

Get a log by ID:

```bash
curl http://localhost:5080/api/sms/1
```

## Bird / MessageBird API integration

The provider client is configured for the Bird SMS API shown in the supplied Bird Message Studio screenshot:

- Endpoint: `POST https://eu1.platform.bird.com/v1/sms/messages`
- Authentication: `Authorization: Bearer <API key>`
- Content type: `application/json`
- JSON properties: `to`, `text`, `from`, and `category`
- Default category: `transactional`

Provider references:

- https://developers.messagebird.com/quickstarts/sms-overview/
- https://developers.messagebird.com/api/sms-messaging/
- https://docs.bird.com/

The application defaults to mock mode. To enable a real provider request, first verify that the Bird account has SMS API access, an approved sender, and permission to send to the intended destination. Live requests can incur charges and may send an actual message.

Set the configuration in your Terminal session. Do not paste API keys into source code, screenshots, recordings, or a shared ZIP:

```bash
export Sms__Mode=MessageBird
export MessageBird__ApiKey='YOUR_ROTATED_BIRD_API_KEY'
export MessageBird__Originator='YOUR_APPROVED_SENDER'
export MessageBird__Category='transactional'
export MessageBird__BaseUrl='https://eu1.platform.bird.com/'
dotnet run --project src/FahadHassan.PulseSend.Api
```

The `Originator` must be a sender identity approved for your Bird account and destination. Use an E.164-format recipient phone number. A successful API response means the provider accepted the request; it does not prove the message was delivered to the recipient.

The API key visible in the provided screenshot should be treated as exposed. Revoke or rotate it in Bird before use, even if you suspect it will not work.

## SQL Server configuration

SQLite is the easiest local option. To use SQL Server, configure a reachable SQL Server instance and set:

```bash
export Database__Provider=SqlServer
export ConnectionStrings__SqlServer='Server=localhost,1433;Database=PulseSend;User Id=sa;Password=YOUR_STRONG_PASSWORD;Encrypt=True;TrustServerCertificate=True'
dotnet run --project src/FahadHassan.PulseSend.Api
```

## API contract

### `POST /api/sms/send`

```json
{
  "creditNumber": "CR-10001",
  "templateName": "Erinnerung"
}
```

The credit number identifies the customer. The template name selects an active template. Supported placeholders are `{Creditnumber}`, `{Name}`, `{amount}`, and `{dueDate}`. Amount and date are formatted using German conventions.

### Other endpoints

- `GET /api/customers/{creditNumber}`
- `GET /api/templates`
- `GET /api/sms/{id}`
- `GET /api/sms?creditNumber=CR-10001`
- `GET /health`

## Data and operational notes

The customer table contains the phone number, name, credit number, amount, and due date. The template table contains the `Erinnerung`, `Mahnung`, and `Kündigung` examples. The SMS log stores the customer and template references, recipient, rendered content, status, provider message ID, provider response JSON, error details, and timestamps.

The mock gateway is useful for repeatable local testing. The Bird gateway is selected with `Sms__Mode=MessageBird`. The service saves a pending log before calling the provider and updates it with the result. Provider timeouts can leave delivery uncertain; do not blindly retry a timed-out request.

This is a take-home demonstration rather than a production-ready debt-collection service. Before production use, add authentication and authorization, rate limiting, idempotency, delivery-status webhook handling, data-retention controls, and approved legal wording. Do not use the sample customer data for real messages. Confirm the recipient, amount, due date, and wording before any live send.
