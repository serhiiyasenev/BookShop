# BookShop

[![Build and test](https://github.com/serhiiyasenev/BookShop/actions/workflows/test.yml/badge.svg)](https://github.com/serhiiyasenev/BookShop/actions/workflows/test.yml)

A book catalog and reservation application with an ASP.NET Core API and an MVC interface. It demonstrates the workflow from maintaining book records to reserving available items, recording delivery details, and updating booking status.

**Project status:** Portfolio application built on .NET 7. The repository contains working application code and automated tests; it is not a complete commercial storefront. There is no payment processing or inventory quantity model.

## Demo

| Scenario | What to show |
|---|---|
| Maintain a catalog | Create a book, search by name, page through results, edit its details and price |
| Reserve books | Create a booking from existing product IDs, with a customer email and delivery date/address |
| Handle availability | A product already attached to a booking cannot be added to another booking through the service |
| Follow a booking | Retrieve a booking and update its status through the API |

Start with the catalog demo below; it does not require an email provider. The booking demo requires a configured SendGrid account and a verified sender.

## Run locally

Prerequisites: .NET 7 SDK, a running SQL Server instance, and a local HTTPS development certificate. Commands below use Bash and run from the repository root. .NET 7 is out of support; upgrade it before considering a production deployment.

```bash
git clone --branch develop https://github.com/serhiiyasenev/BookShop.git
cd BookShop
dotnet restore BookShop.sln
dotnet dev-certs https

# Replace the password with your local SQL Server password.
export ConnectionStrings__Default='Server=localhost,1433;Database=BookShop;User Id=sa;Password=<your-local-password>;TrustServerCertificate=True'

# Install once; the tool is isolated from other projects' EF versions.
dotnet tool install --tool-path ./.tools dotnet-ef --version 7.0.3
./.tools/dotnet-ef database update --project src/DataAccessLayer --startup-project src/Api

dotnet run --project src/Api --launch-profile BookShopAPI
```

Open [Swagger](https://localhost:5001/swagger). On supported platforms, `dotnet dev-certs https --trust` trusts the certificate locally. Otherwise, follow your platform's development-certificate setup.

To run the MVC catalog, open a second terminal, set the same `ConnectionStrings__Default`, then:

```bash
dotnet run --project src/WebUI --launch-profile WebUI
```

Open [the product list](https://localhost:7247/Products). Both entry points use the shared business/data layers and the same database; MVC does not call the API over HTTP. No initial products are seeded, so create a book first.

## Five-minute catalog demo

1. In Swagger, call `POST /Product` with:

   ```json
   {
     "name": "The Three Musketeers",
     "description": "A historical adventure novel for the catalog demo.",
     "author": "Alexandre Dumas",
     "price": 19.49,
     "imageUrl": "https://example.com/book.jpg"
   }
   ```

   The image URL is a placeholder; an image download is not needed for this demo. Expect `201` and copy the returned `id`.
2. Call `GET /Product/{id}` and confirm the returned price is `19.49`.
3. Call `GET /Product?Name=Musketeers&Page=1&PageSize=10`, then find the same record in the MVC product list.
4. Edit the price in the MVC form, reload the API record, and show that both views use the same storage.
5. Submit a negative price or a price with a fractional cent (such as `19.499`) to `POST /Product`; expect validation to reject it with `400`.

For the booking flow, configure the following environment variables in the API terminal using your own SendGrid settings, then restart the API:

| Variable | Purpose |
|---|---|
| `SEND_GRID_API_KEY` | SendGrid API credential |
| `SEND_GRID_EMAIL_FROM` | Verified sender email address |
| `SendGridSettings__SenderNameFrom` | Display name of the sender |

The development configuration stores the **names** of the credential/sender environment variables in `SendGridSettings.ApiKey` and `SenderEmailFromKey`; the email sender resolves their values from the environment. Keep these option values as variable names if you override the configuration.

Use `POST /Booking` in Swagger with a new booking name, an email you control, a delivery address, a delivery date of today or later, and the product ID in `products`. Read the booking back and try reserving the same product again to demonstrate the conflict response. The booking email is sent after persistence: an email failure can leave a saved booking even when the HTTP request returns an error. Check existing bookings before retrying.

## Architecture and tradeoffs

| Component | Responsibility | Why it is separate |
|---|---|---|
| `src/Api` | HTTP API, validation and Swagger | Allows external clients to use the catalog and booking workflow |
| `src/WebUI` | MVC pages for product management | Provides a browser demo over the same business logic |
| `src/BusinessLayer` | Catalog/booking services and DTO mappings | Keeps domain operations reusable across both entry points |
| `src/DataAccessLayer` | EF Core repositories, SQL Server model and migrations | Centralizes persistence and schema evolution |
| `src/InfrastructureLayer` | SendGrid integration | Isolates the external email provider |

A shared layered application is sufficient for this scope. It is straightforward to debug and test, at the cost of both hosts being coupled to the same model and database. Separate API and MVC hosts are alternative entry points, not independently owned microservices.

## Tests and price migration

```bash
dotnet test BookShop.sln --configuration Release
```

Unit tests cover controllers, mappings, price/date validation, local file writes, and SendGrid message construction, tracking settings and provider/transport failures. SendGrid tests inject `ISendGridClient`; no real email or provider credentials are required.

HTTP/MVC integration tests exercise catalog validation, booking creation/update/status, conflicts, pagination, rendered pages, and deletion with anti-forgery protection. The new scenario fixtures use a separate EF Core InMemory database per test and record outgoing email in memory. CI additionally verifies price migrations and the complete booking persistence flow against a real SQL Server container, including reusing existing products and saving newly added booking links. To run the SQL Server tests locally, set `BOOKSHOP_TEST_SQLSERVER` to a local test-server connection with permission to create/drop temporary databases; otherwise those tests are explicitly skipped.

CI merges unit and integration coverage into the **CoverageReport** workflow artifact and posts a per-assembly table on the PR. The table's Health indicator is based on line coverage: below 50% is red, 50–74% is intermediate, and 75% or more is green. Branch coverage is reported separately; a green Health indicator does not mean every path is tested. To collect coverage locally:

```bash
dotnet test BookShop.sln --configuration Release --collect:"XPlat Code Coverage" --results-directory coverage
```

## Known limitations

- Authentication/authorization is not a complete access-control boundary; use a local demo environment.
- A product record acts as one reservable item. There are no stock quantities, payments, taxes, or refunds.
- Reservation checks are application-level; concurrent booking requests need a separate database-concurrency review.
- Booking persistence and email sending are separate operations. Email delivery is not transactional.
- Image upload uses local storage and additional `AllowedExtensions`/`ImageStorageSettings` configuration; the catalog demo above uses a URL only.
- The MVC interface focuses on products; demonstrate bookings through Swagger.
- CI includes a SonarCloud integration that requires a valid `SONAR_TOKEN` with analysis permission for this project.

[`BookShop-2`](https://github.com/serhiiyasenev/BookShop-2) is a related variant, not a separate portfolio case. This repository is the featured version.
