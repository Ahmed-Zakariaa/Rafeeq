<div align="center">

# Rafeeq · رفيق

**Intercity carpooling for Egypt 🇪🇬 & Saudi Arabia 🇸🇦 — share the road, split the cost.**

[![License: MIT](https://img.shields.io/badge/License-MIT-117C6F.svg)](LICENSE)
![.NET](https://img.shields.io/badge/.NET-10-512BD4)
![Angular](https://img.shields.io/badge/Angular-17-DD0031)
![RTL](https://img.shields.io/badge/i18n-Arabic%20%2F%20English%20(RTL)-F4A62A)

</div>

Rafeeq connects a **driver already making an intercity trip** with **passengers heading the same way**, so empty seats get filled. It is **carpooling — cost-sharing, not ride-hailing**: the price a driver may charge per seat is **capped** so a trip only shares fuel cost and never turns a profit. Arabic-first, right-to-left, built for two countries from day one.

## Features

- **Auth & trust** — register / login (JWT), mandatory **email OTP** verification, forgot / reset password.
- **Trips** — post a trip (origin → destination, seats, price within the cost-share cap, gender preference, pickup point), search & filter, mark completed, cancel.
- **Bookings** — request a seat → driver accepts / rejects → seats adjust automatically; contact revealed only after acceptance.
- **Vehicles** — drivers register their cars.
- **Ratings & reports** — two-sided ratings after a trip; report a user for trust & safety.
- **Document verification** — upload ID / license → admin review → **Verified** badge.
- **Notifications** — in-app bell **and** email on booking / trip events.
- **Admin portal** — manage users, drivers, passengers and admins; a dashboard; handle reports; review documents; **custom roles** with a fine-grained permission matrix.
- **Bilingual** — Arabic (default, RTL) & English throughout.

## Tech stack

| | |
|---|---|
| **Backend** | .NET 10 Web API · EF Core · SQL Server · JWT · FluentValidation · Swagger |
| **Frontend** | Angular 17 (standalone) · PrimeNG · Tailwind CSS · ngx-translate |
| **Architecture** | Tier-2 layered, one-way dependencies: `Api → Application → Infrastructure → Domain` |

## Getting started

**Prerequisites:** .NET 10 SDK · SQL Server (LocalDB / Express) · Node.js 20+.

**Backend** — from the repo root:
```bash
dotnet run --project Rafeeq.BE/Rafeeq.Api --launch-profile http
```
Runs on **http://localhost:5200** (Swagger at `/swagger`). Migrations and reference data (roles, countries, cities) are applied automatically on first start.

**Frontend:**
```bash
cd Rafeeq.FE/rafeeq-web
npm install
npm start
```
Runs on **http://localhost:4200** (already CORS-allowed by the API).

## Configuration

Secrets are read from **.NET user-secrets** in development (never committed) and environment variables in production. Email uses any SMTP provider:
```bash
cd Rafeeq.BE/Rafeeq.Api
dotnet user-secrets set "Email:User"     "you@example.com"
dotnet user-secrets set "Email:Password" "your-smtp-app-password"
dotnet user-secrets set "Email:FromEmail" "you@example.com"
```
Without SMTP configured, emails are logged to the console so local dev needs no credentials.

## License

Released under the [MIT License](LICENSE).
