# KPW Companion (MoveWell) — Integration Status

**Last updated:** July 2026  
**Audience:** Kruger’s Pet Wellness — product / clinical stakeholders  
**Legend:** 🟢 Live API · 🟡 Partial (API + demo/placeholder UI) · 🔴 Mock / not built · ⚪ API ready, UI not wired

---

## Summary

| App | Core workflows on API | UI still demo / placeholder |
|-----|------------------------|-----------------------------|
| **Owner app (Flutter)** | ~90% | ~10–15% |
| **Physio portal (Vue)** | ~55–65% | ~40–50% |
| **Overall project** | ~70–75% | Remaining polish & scaffold pages |

The **owner rehab loop** (login → pets → exercises → tracking → videos → chat) is largely production-shaped. The **physio portal** has strong API coverage on clinical pages but still uses demo data for dashboard polish, billing, documents, tasks, and settings.

---

## Owner app (Flutter)

| Feature | Status | Notes |
|---------|--------|--------|
| Login / session refresh | 🟢 | JWT via `POST /api/auth/login`, refresh token |
| Owner sign-up (invite code) | 🟢 | `POST /api/auth/register` with clinic invite code |
| Forgot / reset password | 🟢 | API + Flutter UI; reset link logged to API console (no SMTP yet) |
| Change password | 🟢 | `PUT /api/auth/change-password` from More screen |
| My pets (list, add) | 🟢 | `GET/POST /api/pets` |
| Pet detail — profile & medical history | 🟢 | From pet API payload |
| Pet detail — weekly progress ring | 🟡 | Placeholder % (not from progress API) |
| Pet detail — pain / mobility / energy summary | 🟡 | Static labels (“Low”, “Improving”, “Good”) |
| Exercise program & routine | 🟢 | `GET /api/rehab-programs/pet/{id}` |
| Exercise session logging | 🟢 | `POST /api/pets/{id}/exercise-sessions` |
| Daily tracking (submit) | 🟢 | `POST /api/pets/{id}/tracking` |
| Daily tracking (history / charts) | 🔴 | Submit works; no read-back UI yet |
| Video upload | 🟢 | `POST /api/pets/{id}/videos` |
| Video inbox | 🟢 | `GET /api/pets/{id}/videos` |
| Wellness Assistant (AI chat) | 🟢 | `POST /api/ai/chat` (Vertex Gemini) |
| Messages | 🟢 | `GET/POST /api/pets/{id}/messages` |
| Appointments | 🟢 | Full CRUD + status updates |
| Reminders | 🟢 | `GET /api/reminders` |
| Education hub | 🔴 | Not built (planned) |
| Pop-up wellness facts | 🔴 | Not built (planned) |

---

## Physio portal (Vue)

| Feature | Status | Notes |
|---------|--------|--------|
| Login / profile | 🟢 | `POST /api/auth/login`, `GET /api/auth/me` |
| Forgot / reset password | 🟢 | Portal guest routes + API; reset link in API logs until SMTP |
| Change password | 🟢 | Settings → Security |
| Clinic invite code (share with owners) | 🟢 | Shown on Profile / Clinic settings from `/api/auth/me` |
| Dashboard — today’s schedule | 🟢 | From `GET /api/dashboard/physio` |
| Dashboard — appointment / patient counts | 🟢 | From dashboard API |
| Dashboard — progress overview chart | 🟡 | Demo stats (`dashboardDemo.ts`) |
| Dashboard — recent patient updates | 🟡 | Demo list (`dashboardDemo.ts`) |
| Dashboard — species breakdown | 🟡 | Demo percentages |
| Dashboard — tasks / reminders card | 🟡 | Local demo tasks (`taskDemo.ts`) |
| Patients — list & create | 🟢 | `GET /api/pets/clinic`, `POST /api/pets` + **Add patient** modal (new owner + pet) |
| Patients — detail (programs, progress, videos, appointments) | 🟢 | Multiple API calls per patient |
| Patients — status badges / phase labels | 🟡 | Demo metadata (`patientDemo.ts`) |
| Patients — outcome measures panel | 🟡 | Static demo content |
| Appointments — calendar & list | 🟢 | `GET/POST/PUT /api/appointments` |
| Appointments — type / location labels | 🟡 | Demo labels (`appointmentDemo.ts`) |
| Treatment plans — program load / create | 🟢 | Rehab program API |
| Treatment plans — phase UI (sidebar, progress) | 🟡 | Demo phases (`planDemo.ts`) |
| Exercise library | 🟢 | `GET /api/exercises` |
| Exercise library — thumbnails / category polish | 🟡 | Demo assets (`exerciseDemo.ts`) |
| Progress — per-pet charts & latest video | 🟢 | Progress + videos API |
| Progress — clinic summary strip | 🟡 | Demo stats |
| Messages — threads & send | 🟢 | Messages API |
| Messages — starred threads | 🟡 | Browser localStorage |
| Reports — PDF download | 🟢 | `GET /api/reports/pet/{id}/download` |
| Reports — history & report types | 🟡 | Demo (`reportsDemo.ts`) |
| Video approvals (review pending uploads) | ⚪ | API + store exist; **no portal page yet** |
| Documents | 🔴 | 100% demo (`documentsDemo.ts`) |
| Tasks | 🔴 | 100% demo / localStorage (`taskDemo.ts`) |
| Billing | 🔴 | 100% demo (`billingDemo.ts`) |
| Settings — clinic / notifications | 🟡 | Local demo preferences (`settingsDemo.ts`) |
| Settings — security (password) | 🟢 | Change password via API |
| SOAP notes (private clinical notes) | 🔴 | Not built (planned) |
| Education hub CMS | 🔴 | Not built (planned; backend has static education files for AI) |

---

## Backend API (reference)

Endpoints in active use by at least one client today:

| Domain | Endpoints | Used by |
|--------|-----------|---------|
| Auth | login, refresh, me, register, forgot-password, reset-password, change-password | Both apps |
| Pets | clinic list, owner list, create, update | Both apps |
| Rehab programs | list by pet, create | Both apps |
| Exercise sessions | log completion | Owner app |
| Exercises | library list | Portal |
| Tracking | upsert daily log | Owner app |
| Videos | upload, list, pending, review | Owner upload; review API unused in portal UI |
| Progress | pet summary | Portal |
| Dashboard | physio summary | Portal |
| Appointments | list, create, status | Both apps |
| Messages | threads, per-pet messages, read | Both apps |
| Reminders | list | Owner app |
| AI chat | chat | Owner app |
| Reports | pet PDF download | Portal |

---

## Recommended next wiring (priority)

1. **🟡 → 🟢** Owner app: real progress % and tracking metrics on pet overview (read `GET /api/pets/{id}/tracking` + progress).
2. **⚪ → 🟢** Physio portal: Video Approvals page using existing pending/review API.
3. **🟡 → 🟢** Portal dashboard widgets: replace demo charts with aggregated API data.
4. **🔴** SOAP notes, Education hub, pop-up facts — new backend + UI (client roadmap).

---

## Environments

| Environment | Owner app API | Portal API |
|-------------|---------------|------------|
| Local dev | `https://localhost:7112` | `https://localhost:7112` (or `VITE_API_BASE_URL`) |
| Flutter Web | `http://localhost:8068` (CORS allowlisted) | — |
| Portal dev | — | `http://localhost:5287` |

**Seed clinic invite codes (after migration):** `KWPDEMO1` (demo clinic), `KPWNORTH2` (north branch). Password reset emails are written to API logs until SMTP is configured.

---

*This document reflects codebase state as of the integration audit in July 2026. Update when features move from 🟡/🔴/⚪ to 🟢.*
