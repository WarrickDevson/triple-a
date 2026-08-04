# KPW Companion — Brief Gap Backlog

Maps the client brief (KPW Companion pitch / UX blueprint) against the current MoveWell build and `project_plan.md`.

**Goal of this backlog:** close the gap between “core rehab product works” and “matches what the brief sells for an MVP demo / soft launch.”

---

## Snapshot

| Layer | Status |
|---|---|
| Phases 1–5 (`project_plan.md`) | Largely **done** — auth, pets, exercise engine, physio builder, video loop, tracking, AI chat, PDF reports |
| Phase 6 | Reports **done**; payments / tier gating **open** (intentionally deferred) |
| Brief extras not in plan | Education hub UI, owner progress polish, gamification, 3D USP, push/email reminders |

**Suggested sequencing for “brief-complete MVP”:** Phase A → B → C → D → then Phase 6 payments when ready to monetise. Future roadmap items stay out of MVP.

---

## Already delivered (do not re-build)

These brief items are covered end-to-end today:

- Owner + physio auth (JWT); physio portal login
- Pet profiles, species (Canine/Feline/Equine/…), medical history on create
- Rehab programmes + exercise library (steps, video URL, safety/mistakes)
- Owner step-by-step exercise engine + mark complete
- Daily tracking (pain, lameness, energy, appetite, mobility)
- Video upload → physio review/feedback; owner video feedback inbox
- Appointments CRUD (API + physio portal + owner app)
- Owner↔physio messaging (pet-scoped threads, optional video reference)
- In-app reminders for appointments and exercises due today
- Physio patient roster, create patient, programme builder, progress chart, PDF report
- AI wellness chat (RAG over education markdown)
- Multi-species filters on exercises

---

## Phase A — Close the owner↔clinic loop  
*Highest impact vs brief. Builds on Phase 5 video/AI work.*

**Brief promise:** therapist messaging, appointment scheduling, daily reminders, owner sees feedback.

| ID | Item | Status |
|---|---|---|
| A1 | **Appointments CRUD API** — create / list / update status / cancel for owner + physio | **Done** |
| A2 | **Physio portal — appointments** — schedule list with create & complete | **Done** |
| A3 | **Owner app — appointments** — view upcoming, request, cancel | **Done** |
| A4 | **Owner video feedback inbox** — list submissions + physio notes / reviewed state | **Done** |
| A5 | **Secure messaging** — threads between owner and assigned physio (text; optional attach video ref) | **Done** |
| A6 | **Reminders foundation** — exercise due + appointment reminders (start with in-app / email; push later) | **Done** |

### Phase A acceptance (A1–A4 done)

- [x] Owner can see upcoming appointments and request/cancel visits.
- [x] Physio can schedule and mark appointments without relying on seed data.
- [x] Owner can read physio feedback on uploaded videos without leaving the app.
- [x] Owner and physio can exchange messages tied to a pet/clinic (A5).
- [x] Exercise/appointment reminders (A6).

---

## Phase B — Education Hub & owner progress polish  
*Turns AI-only content + tracking into the dashboard the brief mockups show.*

**Brief promise:** Education Centre, progress ring, pain/mobility charts, streaks, recovery milestones.

| ID | Item | Surfaces | Depends on | Effort* |
|---|---|---|---|---|
| B1 | **Education Hub API** — list/get articles from existing `Education/*.md` (title, tags, body) | Backend | Existing MD corpus | S |
| B2 | **Owner Education Hub UI** — browse + read articles (AI chat remains secondary entry) | Flutter | B1 | M |
| B3 | **Owner progress dashboard** — charts for pain/mobility/weight/compliance (reuse progress API patterns from physio) | Flutter | Existing tracking/progress APIs | M |
| B4 | **Home dashboard polish** — today’s exercises X/Y, progress %, current pain score, deep links | Flutter | Programmes + tracking | S |
| B5 | **Streaks & simple milestones** — consecutive completion days + “first week complete” style badges (lightweight; not full gamification platform) | Backend + Flutter | Exercise sessions | M |
| B6 | **Owner registration UI** — register flow calling existing `POST /api/auth/register` | Flutter | Auth API | S |

### Phase B acceptance

- Owner home matches brief mockup intent (today’s programme, pain, progress cue).
- Education is browsable, not only via AI.
- Owner can see their own trends without opening the physio portal.
- New owners can self-register in the app.

---

## Phase C — Soft subscription gating (no payment yet)  
*Aligns Free / Premium / Professional from the brief without waiting on Payfast.*

Maps to **Phase 6** prep in `project_plan.md` (tier field exists; gating does not).

| ID | Item | Surfaces | Depends on | Effort* |
|---|---|---|---|---|
| C1 | **Tier policy matrix** — document which features are Free vs Premium vs Professional | Spec only | Brief tiers | S |
| C2 | **API entitlement checks** — gate premium endpoints (e.g. unlimited video, AI chat, messaging) with clear `402`/`403` | Backend | C1 | M |
| C3 | **Owner upgrade prompts** — soft paywall screens when Free hits limits | Flutter | C2 | M |
| C4 | **Physio Pro prompts** — soft gate on multi-patient / reports / builder if using Free clinic seed | Vue | C2 | S |
| C5 | **Admin tier override** — SysAdmin can set `SubscriptionTier` for demos | Backend + Vue (simple) | Users entity | S |

### Phase C acceptance

- Free vs Premium vs Pro behaviour is enforceable in API and visible in UI.
- Demo accounts can be flipped without payment integration.
- Payfast / IAP remain Phase 6 proper.

---

## Phase D — Brief “nice-to-have” for launch polish  
*Do after A–C unless a client demo specifically needs one item.*

| ID | Item | Surfaces | Notes | Effort* |
|---|---|---|---|---|
| D1 | Push notifications (FCM) for A6 reminders | Flutter + Backend | Replace/augment local notifications | M |
| D2 | Before & after photo/video comparison | Backend + Flutter | New media pair entity or reuse submissions | M |
| D3 | Equipment library screen | Flutter (+ optional API) | Content-led; low clinical risk | S |
| D4 | Vaccination / basic care reminders | Backend + Flutter | Brief “standard pet app” parity; optional for physio MVP | M |
| D5 | Clinic team invite (additional Physio users under same `ClinicId`) | Backend + Vue | Partial clinic model already exists | M |
| D6 | Vet / Nurse read-only role | Backend + Vue | Brief “whole rehab team”; expands role enum | L |
| D7 | Owner PDF download of own report | Flutter | API already exists for physio | S |
| D8 | Treatment timeline / medical history CRUD UI | Flutter + Vue | History created with pet; editing thin | M |

---

## Phase 6 (existing plan) — Payments when ready to monetise

Unchanged from `project_plan.md`; start after Phase C soft gating is stable.

| ID | Item | Status |
|---|---|---|
| 6.1 | Payfast subscription API + webhooks | Open |
| 6.2 | Endpoint filters returning `402` on failed billing | Open (overlaps C2) |
| 6.3 | Payment / upgrade gate screens (Flutter + Vue) | Open |
| 6.4 | Apple / Google IAP (if store distribution) | Open / later |
| 6.5 | PDF reports + physio analytics charts | **Done** |

---

## Explicitly out of MVP (brief “Future Features”)

Do not schedule in Phases A–D unless the client re-prioritises:

- AI gait analysis  
- 3D anatomy visualisations / rotatable 3D exercise models (brief USP — treat as Phase E content/tech spike)  
- Voice-guided exercises  
- Behaviour monitoring  
- Insurance integration  
- Live tele-rehab video calls (async video loop already covers tele-rehab MVP)  
- Full revenue / practice management dashboard  

### Optional Phase E — Signature USP spike (post-MVP)

| ID | Item | Notes |
|---|---|---|
| E1 | 3D exercise viewer spike (one exercise: Sit-to-Stand, rotate + 0.5x) | High effort; content pipeline; only if client insists on USP from pitch |
| E2 | Richer gamification (achievements catalogue, celebrations) | Extends B5 |

---

## Recommended build order (next 4–6 sprints)

```
Sprint 1:  A1 A2 A3 A4          Appointments + owner feedback inbox  [DONE]
Sprint 2:  A5 A6                Messaging + reminder foundation  [DONE]
Sprint 3:  B1 B2 B4 B6          Education hub + home polish + register
Sprint 4:  B3 B5                Owner charts + streaks/milestones
Sprint 5:  C1 C2 C3 C4 C5       Soft tier gating for demos
Sprint 6:  D7 D1 (optional)     Owner PDF + push; then Phase 6 payments when commercial
```

---

## Traceability: brief → backlog

| Brief feature | Backlog |
|---|---|
| Step-by-step exercises / interactive programmes | Done (Phases 3–4) |
| Progress tracking / pain monitoring | Done; polish in B3–B4 |
| Video submission for therapist feedback | Done; owner inbox A4 **Done** |
| Appointment scheduling | A1–A3 **Done** |
| Daily reminders | A6 **Done** (in-app); push in D1 |
| Educational resources | B1–B2 (AI chat already Done) |
| Therapist messaging | A5 **Done** |
| Free / Premium / Professional | C1–C5 → Phase 6 payments |
| Physio patient mgmt / exercise builder / reports | Done |
| Team access | D5–D6 |
| Progress ring / streaks / celebrate | B4–B5, E2 |
| Before & after | D2 |
| 3D interactive models | E1 (future) |
| Vaccination reminders | D4 |
| Multi-species | Done |
| Tele-rehabilitation | Done (async); live calls = future |

---

## Definition of “brief-complete MVP”

Ready to demo against the client PDF when:

1. Phases A and B are done.  
2. Phase C soft gating works for demo accounts.  
3. Future Features (gait AI, 3D, insurance, etc.) are explicitly labelled roadmap.  
4. Phase 6 payments remain a commercial go-live track, not a demo blocker.
