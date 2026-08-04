# MoveWell (KPW Companion)

Veterinary physiotherapy and rehabilitation platform for **Kruger’s Pet Wellness** — helping pet owners follow home rehab programmes and giving physiotherapists tools to prescribe, monitor, and support recovery.

> Better compliance. Better outcomes. Stronger bonds.

## What’s in this repo

| Path | Role |
|------|------|
| `backend-api-dot-net/` | .NET 9 Web API (Clean Architecture) + SQL Server |
| `owner-app-flutter/` | Flutter owner mobile app |
| `physio-portal-vue3/` | Vue 3 + TypeScript physio web portal |
| `site-landing/` | Marketing / landing site |
| `docs/` | Integration status and project docs |
| `project_plan.md` | Technical specification & blueprint |
| `gap_backlog.md` | Brief vs build gap backlog |

## Stack

- **API:** .NET 9, C#, Clean Architecture, JWT auth
- **Database:** Microsoft SQL Server
- **Physio portal:** Vue 3, TypeScript, Pinia, Tailwind CSS, Vite
- **Owner app:** Flutter (Dart), Riverpod
- **Cloud (optional):** Google Cloud Storage, Transcoder, Vertex AI (Gemini)
- **Compliance:** Designed with POPIA (South Africa) in mind for owner PII and clinic data

## Core capabilities

- Owner auth (invite-code sign-up), pets, rehab programmes, exercise sessions
- Daily tracking (pain, mobility, energy, appetite, lameness)
- Video upload for therapist feedback
- Appointments, reminders, and owner↔physio messaging
- Physio portal: patients, exercise library, programmes, progress, PDF reports
- AI wellness assistant (RAG over education content)

See [docs/INTEGRATION_STATUS.md](docs/INTEGRATION_STATUS.md) for live vs placeholder coverage.

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (LTS) + npm
- [Flutter](https://docs.flutter.dev/get-started/install) (SDK ^3.10)
- SQL Server (local or remote) with a connection string in API config

## Getting started

### 1. Backend API

```powershell
cd backend-api-dot-net\KPW.Api
dotnet restore
dotnet run --launch-profile http
```

- API: `http://localhost:5057`
- OpenAPI / Scalar: `http://localhost:5057/scalar/v1`

Configure SQL and providers in `KPW.Api/appsettings.json` / `appsettings.Development.json`.  
For Google video/AI setup, see [backend-api-dot-net/docs/GCP_SETUP.md](backend-api-dot-net/docs/GCP_SETUP.md). Staging/IIS notes: [backend-api-dot-net/docs/IIS_STAGING.md](backend-api-dot-net/docs/IIS_STAGING.md).

### 2. Physio portal

```powershell
cd physio-portal-vue3
npm install
npm run dev
```

Point the portal at the running API (see portal env / API base URL config in that package).

### 3. Owner app

```powershell
cd owner-app-flutter
flutter pub get
flutter run
```

Ensure the app’s API base URL targets your local or staging API.

## Documentation

| Doc | Purpose |
|-----|---------|
| [project_plan.md](project_plan.md) | Full technical blueprint (schema, phases, design tokens) |
| [gap_backlog.md](gap_backlog.md) | Brief gap backlog for MVP / soft launch |
| [docs/INTEGRATION_STATUS.md](docs/INTEGRATION_STATUS.md) | Feature integration status by app |

## Licence / ownership

Private project for Kruger’s Pet Wellness (KPW) Veterinary Physiotherapy. Not open source unless explicitly licensed otherwise.
