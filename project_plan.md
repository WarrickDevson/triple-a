This document serves as the **Comprehensive Technical Specification & Blueprint** for **KPW Companion (MoveWell)**. 

---

# Technical Stack Overview
*   **Database:** Microsoft SQL Server (MSSQL)
*   **Backend:** .NET 9 (C# Web API with Clean Architecture)
*   **Physio Web Portal:** Vue 3 (Composition API, TypeScript, Pinia, Tailwind CSS)
*   **Owner Mobile App:** Flutter (Dart, Bloc or Riverpod for State Management)
*   **Local Compliance:** Protection of Personal Information Act (POPIA) of South Africa. Secure encryption of owner PII (Personally Identifiable Information) and clinic data.

---

# Section 1: Styling Brief & Design Tokens
*Prepared by UX & UI Experts for the Kruger’s Pet Wellness Brand Identity.*

### 1. Brand Essence & UI Tone
*   **Tone:** Clinical-yet-compassionate, warm, clean, accessible, and structured. 
*   **Visual Direction:** High legibility for veterinarians and pet owners under varying stress levels. Clear visual cues, spacious layouts, and soft transitions to prevent cognitive overload.

### 2. Color Palette (Design Tokens)
```css
/* Color Palette */
--color-primary-dark:      #0C3C54; /* Deep Navy (Trust, clinical authority) */
--color-primary-light:     #1E6E8E; /* Ocean Teal (Active, encouraging) */
--color-accent-amber:      #E28743; /* Warm Amber (Warnings, active streaks, highlights) */
--color-success-green:     #2D8B57; /* Forest Green (Completed exercises, recovery goals) */
--color-neutral-dark:      #212529; /* Off-Black (High contrast text) */
--color-neutral-light:     #F8F9FA; /* Off-White (Screen backgrounds) */
--color-neutral-grey:      #E9ECEF; /* Soft Grey (Borders, inactive states) */
--color-alert-red:         #D90429; /* Stop/Pain Warning (Excessive pain alert) */
```

### 3. Typography Guide
*   **Primary Font Family (Web & Mobile):** Inter or Roboto (highly legible on small mobile screens).
*   **Headings Font Family (Optional):** Merriweather (used sparingly for editorial feel in the Education Hub).
*   **Hierarchy Scales:**
    *   **H1 (App Titles/Headers):** 24px Bold (Tracking: -0.5px)
    *   **H2 (Section Titles):** 20px Semi-Bold (Tracking: -0.2px)
    *   **Body Text (Primary):** 16px Regular (Line-height: 1.5)
    *   **Body Text (Secondary/Metadata):** 14px Regular (Line-height: 1.4)
    *   **Caption/Button Text:** 12px Bold (Uppercase, tracking: +0.5px)

### 4. Interactive Elements & Component Rules
*   **Buttons:**
    *   *Primary:* Rounded corners (`border-radius: 8px`). Deep Navy background, white text. Min height: `48px` (Touch target safety).
    *   *Secondary:* Outlined Deep Navy, transparent background.
    *   *Danger/Alert:* Forest Green or Amber based on severity thresholds.
*   **Forms & Inputs:**
    *   Text inputs must have explicit visual borders (no underline-only fields) to assist accessibility.
    *   Active/Focus state: Highlighted with a `2px` Teal border (`--color-primary-light`).
*   **Accessibility (WCAG 2.1 AA Compliance):**
    *   Text on backgrounds must maintain a minimum contrast ratio of 4.5:1.
    *   Touch targets on the Flutter mobile app must be a minimum of $48 \times 48 \text{ dp}$.

---

# Section 2: Database Design (MSSQL Schema)
Every table includes audit/system fields to enforce tracking, historical consistency, and soft-delete capabilities:
*   `CreatedDate` (DATETIME2, Default: `SYSUTCDATETIME()`)
*   `CreatedUserId` (INT, Nullable for system actions)
*   `ModifiedDate` (DATETIME2, Default: `SYSUTCDATETIME()`)
*   `ModifiedUserId` (INT, Nullable)
*   `IsActive` (BIT, Default: `1` for Soft Deletes)

### Entity-Relationship Schema Map (Simplified DDL)

```sql
-- 1. CLINICTABLE
CREATE TABLE Clinics (
    ClinicId INT IDENTITY(1,1) PRIMARY KEY,
    ClinicName NVARCHAR(150) NOT NULL,
    VatNumber NVARCHAR(50) NULL, -- South African VAT No.
    PhysicalAddress NVARCHAR(500) NOT NULL,
    ContactNumber NVARCHAR(20) NOT NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedUserId INT NULL,
    ModifiedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    ModifiedUserId INT NULL,
    IsActive BIT NOT NULL DEFAULT 1
);

-- 2. USERS TABLE (Covers Admins, Physios, and Owners)
CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    ClinicId INT NULL FOREIGN KEY REFERENCES Clinics(ClinicId),
    Email NVARCHAR(256) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(500) NOT NULL,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    PhoneNumber NVARCHAR(20) NULL,
    UserRole NVARCHAR(50) NOT NULL, -- 'SysAdmin', 'Physio', 'Owner'
    SubscriptionTier NVARCHAR(50) NOT NULL DEFAULT 'Free', -- 'Free', 'Premium', 'Pro'
    CreatedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedUserId INT NULL,
    ModifiedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    ModifiedUserId INT NULL,
    IsActive BIT NOT NULL DEFAULT 1
);

-- 3. PETS TABLE
CREATE TABLE Pets (
    PetId INT IDENTITY(1,1) PRIMARY KEY,
    OwnerId INT NOT NULL FOREIGN KEY REFERENCES Users(UserId),
    PetName NVARCHAR(100) NOT NULL,
    Species NVARCHAR(50) NOT NULL, -- 'Dog', 'Cat', 'Equine', etc.
    Breed NVARCHAR(100) NULL,
    BirthDate DATE NULL,
    WeightKg DECIMAL(5,2) NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedUserId INT NULL,
    ModifiedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    ModifiedUserId INT NULL,
    IsActive BIT NOT NULL DEFAULT 1
);

-- 4. MEDICAL HISTORY TABLE
CREATE TABLE MedicalHistories (
    MedicalHistoryId INT IDENTITY(1,1) PRIMARY KEY,
    PetId INT NOT NULL FOREIGN KEY REFERENCES Pets(PetId),
    Diagnosis NVARCHAR(250) NOT NULL,
    InjuryOrCondition NVARCHAR(MAX) NULL,
    SurgeryDate DATE NULL,
    ClinicianNotes NVARCHAR(MAX) NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedUserId INT NULL,
    ModifiedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    ModifiedUserId INT NULL,
    IsActive BIT NOT NULL DEFAULT 1
);

-- 5. EXERCISE LIBRARY (Base Templates)
CREATE TABLE Exercises (
    ExerciseId INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(150) NOT NULL,
    ShortDescription NVARCHAR(500) NULL,
    TargetedMuscles NVARCHAR(250) NULL,
    ClinicalPurpose NVARCHAR(500) NULL,
    SafetyNotes NVARCHAR(MAX) NULL,
    CommonMistakes NVARCHAR(MAX) NULL,
    VideoUrl NVARCHAR(500) NULL, -- Demo video storage reference
    DifficultyLevel INT NOT NULL DEFAULT 1, -- 1 (Easy) to 5 (Hard)
    CreatedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedUserId INT NULL,
    ModifiedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    ModifiedUserId INT NULL,
    IsActive BIT NOT NULL DEFAULT 1
);

-- 6. EXERCISE STEPS TABLE
CREATE TABLE ExerciseSteps (
    ExerciseStepId INT IDENTITY(1,1) PRIMARY KEY,
    ExerciseId INT NOT NULL FOREIGN KEY REFERENCES Exercises(ExerciseId),
    StepNumber INT NOT NULL,
    StepInstruction NVARCHAR(1000) NOT NULL,
    ImageUrl NVARCHAR(500) NULL, -- Illustrative graphic file path
    CreatedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedUserId INT NULL,
    ModifiedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    ModifiedUserId INT NULL,
    IsActive BIT NOT NULL DEFAULT 1
);

-- 7. REHABILITATION PROGRAMS
CREATE TABLE RehabPrograms (
    RehabProgramId INT IDENTITY(1,1) PRIMARY KEY,
    PhysioId INT NOT NULL FOREIGN KEY REFERENCES Users(UserId),
    PetId INT NOT NULL FOREIGN KEY REFERENCES Pets(PetId),
    ProgramTitle NVARCHAR(150) NOT NULL,
    StartDate DATE NOT NULL,
    EndDate DATE NULL,
    Notes NVARCHAR(MAX) NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedUserId INT NULL,
    ModifiedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    ModifiedUserId INT NULL,
    IsActive BIT NOT NULL DEFAULT 1
);

-- 8. REHAB PROGRAM EXERCISES (Junction table with assignment rules)
CREATE TABLE RehabProgramExercises (
    RehabProgramExerciseId INT IDENTITY(1,1) PRIMARY KEY,
    RehabProgramId INT NOT NULL FOREIGN KEY REFERENCES RehabPrograms(RehabProgramId),
    ExerciseId INT NOT NULL FOREIGN KEY REFERENCES Exercises(ExerciseId),
    Repetitions INT NOT NULL DEFAULT 10,
    Sets INT NOT NULL DEFAULT 3,
    FrequencyPerDay INT NOT NULL DEFAULT 1,
    CreatedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedUserId INT NULL,
    ModifiedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    ModifiedUserId INT NULL,
    IsActive BIT NOT NULL DEFAULT 1
);

-- 9. DAILY TRACKING LOGS
CREATE TABLE DailyTrackingLogs (
    LogId INT IDENTITY(1,1) PRIMARY KEY,
    PetId INT NOT NULL FOREIGN KEY REFERENCES Pets(PetId),
    LogDate DATE NOT NULL DEFAULT (CAST(SYSUTCDATETIME() AS DATE)),
    PainScore INT NULL, -- Slider value (1 to 10)
    LamenessScore INT NULL, -- Slider value (1 to 10)
    EnergyScore INT NULL, -- Slider value (1 to 10)
    AppetiteScore INT NULL, -- Slider value (1 to 10)
    MobilityScore INT NULL, -- Slider value (1 to 10)
    WeightKg DECIMAL(5,2) NULL,
    IsCompleted BIT NOT NULL DEFAULT 0,
    CreatedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedUserId INT NULL,
    ModifiedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    ModifiedUserId INT NULL,
    IsActive BIT NOT NULL DEFAULT 1
);

-- 10. CLIENT VIDEO UPLOADS FOR REVIEW
CREATE TABLE VideoSubmissions (
    VideoSubmissionId INT IDENTITY(1,1) PRIMARY KEY,
    PetId INT NOT NULL FOREIGN KEY REFERENCES Pets(PetId),
    ExerciseId INT NOT NULL FOREIGN KEY REFERENCES Exercises(ExerciseId),
    RawVideoStorageUrl NVARCHAR(500) NOT NULL,
    ProcessedVideoStreamingUrl NVARCHAR(500) NULL, -- HLS stream path
    PhysioFeedbackNotes NVARCHAR(MAX) NULL,
    IsReviewed BIT NOT NULL DEFAULT 0,
    CreatedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedUserId INT NULL,
    ModifiedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    ModifiedUserId INT NULL,
    IsActive BIT NOT NULL DEFAULT 1
);

-- 11. APPOINTMENTS
CREATE TABLE Appointments (
    AppointmentId INT IDENTITY(1,1) PRIMARY KEY,
    PhysioId INT NOT NULL FOREIGN KEY REFERENCES Users(UserId),
    OwnerId INT NOT NULL FOREIGN KEY REFERENCES Users(UserId),
    PetId INT NOT NULL FOREIGN KEY REFERENCES Pets(PetId),
    ScheduledDateTime DATETIME2 NOT NULL,
    AppointmentStatus NVARCHAR(50) NOT NULL DEFAULT 'Scheduled', -- 'Scheduled', 'Completed', 'Cancelled'
    ClientNotes NVARCHAR(500) NULL,
    ClinicianNotes NVARCHAR(MAX) NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedUserId INT NULL,
    ModifiedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    ModifiedUserId INT NULL,
    IsActive BIT NOT NULL DEFAULT 1
);
```

---

# Section 3: Repository File Structure

A clean, modular structure split into three primary folders under a single source code monorepo repository.

```
kpw-companion-platform/
│
├── backend-api-dot-net/             # ASP.NET Core 9 Web API
│   ├── KPW.Api/                     # Entry Point, Controllers, Routing, Config
│   │   ├── Controllers/
│   │   ├── Middleware/
│   │   └── Program.cs
│   ├── KPW.Application/             # Business Logic (CQRS - MediatR, DTOs, Mapping)
│   │   ├── Commands/
│   │   ├── Queries/
│   │   └── Interfaces/
│   ├── KPW.Domain/                  # Entities, Value Objects, Domain Exceptions
│   │   ├── Entities/
│   │   └── Common/ (System Base Entity for ModifiedUserId, IsActive etc)
│   └── KPW.Infrastructure/          # Persistence, MSSQL DbContext, External APIs
│       ├── Data/ (EF Core Migrations, ApplicationDbContext)
│       ├── Services/ (Video Transcoding, Local payment gateways e.g. Payfast)
│       └── Repositories/
│
├── physio-portal-vue3/              # Professional Dashboard Web Application
│   ├── public/
│   ├── src/
│   │   ├── assets/                  # CSS Styles, Brand SVGs, Images
│   │   ├── components/              # Shared Reusable Core UI Components
│   │   │   ├── BaseButton.vue
│   │   │   ├── BaseInput.vue
│   │   │   └── VideoPlayer.vue
│   │   ├── composables/             # Reusable UI hook structures (useAuth, useVideo)
│   │   ├── router/                  # Vue Router (Middleware & Role Guards)
│   │   ├── store/                   # Pinia state stores (auth, patients, exerciseBuilder)
│   │   ├── views/                   # Feature Screens
│   │   │   ├── Dashboard.vue
│   │   │   ├── PatientsList.vue
│   │   │   ├── ProgramBuilder.vue
│   │   │   └── VideoApprovals.vue
│   │   ├── App.vue
│   │   └── main.ts
│   ├── tailwind.config.js
│   └── vite.config.ts
│
└── owner-app-flutter/               # Native Mobile App (iOS / Android)
    ├── assets/                      # Icons, Splash Animations, System Assets
    ├── lib/
    │   ├── core/                    # Navigation, Design Tokens, Network Clients
    │   │   ├── theme/
    │   │   └── network/
    │   ├── features/                # Domain-Driven Feature Folders
    │   │   ├── auth/                # Login, Register, Profile Specs
    │   │   ├── dashboard/           # Owner Dashboard widget trees
    │   │   ├── exercises/           # Interactive state machine pages (Sit-To-Stand UI)
    │   │   ├── progress_tracker/    # Sliders, Analytics charts, logs
    │   │   └── appointments/        # Calendar interfaces
    │   ├── shared/                  # App-wide components
    │   └── main.dart
    └── pubspec.yaml
```

---

# Section 4: Phase-by-Phase Specification & Technical Checklists

---

## Phase 1: API Foundation, Database & Core Security

Establish the base server engine, configure safety checks, and deploy database migrations.

### Technical Scope & Implementation Rules
*   Setup .NET 9 Web API using Clean Architecture layers.
*   Configure EF Core to target the local/cloud MSSQL database instance.
*   Enforce system-wide POPIA compliance (Secure password hashing using Argon2/PBKDF2, SSL/TLS protocol mapping, secure database environments).
*   Add a Global Query Filter to automatically exclude entities where `IsActive = 0` (Soft Deletes).
*   Create an EF Core interceptor to auto-populate `CreatedDate`, `ModifiedDate`, `ModifiedUserId`, and `IsActive` fields on SaveChanges.

### DB Schema Fields To Populate
*   `Clinics`, `Users`.

### Developer Implementation Checklist

#### Backend & Database (.NET & MSSQL)
- [x] Initialize MSSQL Server schema and seed structural lookup roles (`SysAdmin`, `Physio`, `Owner`).
- [x] Set up global EF Core Interceptor to intercept entity updates and auto-inject `ModifiedUserId`, `ModifiedDate` (using UTC), and preserve base properties.
- [x] Configure global Soft-Delete Query Filter (`HasQueryFilter(e => e.IsActive)`) on all db tables.
- [x] Implement JWT Token Generation and validation filters.
- [x] Integrate ASP.NET Core identity system with custom extensions for POPIA compliance (automatic IP masking in logs, secure salt storage).

#### Owner App Setup (Flutter)
- [x] Setup base multi-environment scaffolding (Development, Staging, Production configuration blocks).
- [x] Create system styles matching the Styling Brief.
- [x] Configure `dio` network client with global interception to automatically attach JWT authorization headers and process refresh tokens on `401 Unauthorized`.

#### Physio Portal Setup (Vue 3)
- [x] Scaffold Vue 3 app using Vite and TypeScript.
- [x] Configure Tailwind CSS configuration mappings matching the design token variables.
- [x] Establish system routing pathways and build basic authentication guards.

---

## Phase 2: User Profiles & Pet Onboarding

Establish multi-tenant profile isolation across clinics, professional veterinary profiles, and patient configurations.

### Technical Scope & Implementation Rules
*   **Owner Side:** Register accounts, onboarding pets (species selection, breed, initial weight tracking, age, and existing condition categorization).
*   **Physio Side:** Clinic onboarding, inviting other physiotherapists, creating patient profiles manually from clinical records.

### DB Schema Fields To Populate
*   `Pets`, `MedicalHistories`.

### Developer Implementation Checklist

#### Backend API (.NET)
- [x] Implement endpoints for `POST /api/pets`, `GET /api/pets/owner/{id}`, and `PUT /api/pets/{id}`.
- [x] Ensure that whenever a pet's profile metadata is edited, audit fields (`ModifiedUserId`, `ModifiedDate`) update accordingly.
- [x] Implement validation models (e.g., validate that `Species` values map securely to defined options like Canine, Feline, Equine).
- [x] Build transactional save models so that creating a pet and logging an initial `MedicalHistory` block are executed inside a single SQL transaction.

#### Owner App Setup (Flutter)
- [x] Build the **"My Pets Profiles"** screen and the **"Add New Pet"** step-by-step form.
- [x] Add field validation logic (prevent negative weight inputs, prevent future birth dates).
- [x] Store basic pet credentials inside local state stores (BLoC/Riverpod) to avoid redundant backend API calls.

#### Physio Portal Setup (Vue 3)
- [x] Design high-fidelity dashboards to view a clinic's client list.
- [x] Build **"Create Patient Profile"** forms to allow therapists to onboard less tech-savvy clients.

---

## Phase 3: The Interactive Exercise Engine & Program Delivery

Build the cornerstone product differentiator—the Step-by-Step interactive exercise engine.

```
       [Start Routine]
              │
              ▼
    ┌──────────────────┐
    │  Exercise Step   │◄─────────────────┐
    │  - Play Video    │                  │
    │  - View Safety   │                  │
    │  - Count Sets    │                  │
    └────────┬─────────┘                  │
             │                            │
      [Next Step Pressed]                 │
             │                            │
             ▼                            │
     {Is Last Step?} ───(No: Next Step)───┘
             │
         (Yes: Done)
             │
             ▼
    ┌──────────────────┐
    │ Log Session to DB│
    └──────────────────┘
```

### Technical Scope & Implementation Rules
*   **The Engine:** A state machine on the Flutter client that guides owners through steps with visual media, safety triggers, rep counters, common mistakes, and feedback mechanisms.
*   **The Data:** Exercises must load dynamically from the base API library. This ensures that the clinical template rules designed by therapists map dynamically to screens without app store updates.

### DB Schema Fields To Populate
*   `Exercises`, `ExerciseSteps`, `RehabPrograms`, `RehabProgramExercises`.

### Developer Implementation Checklist

#### Backend API (.NET)
- [x] Build API endpoints to query exercises by category filters (`GET /api/exercises?species=Canine&condition=HipDysplasia`).
- [x] Expose program execution plans (`GET /api/rehab-programs/pet/{petId}`). This should return the program, the set of exercises, and individual step instructions.

#### Owner App Setup (Flutter)
- [x] Implement the dynamic **Exercise Step State Machine Engine**.
- [x] Integrate a video player component capable of streaming MP4 or HLS clips with play, pause, and replay triggers.
- [x] Develop the physical transition layout displaying: Step Title, Graphic, Instructions, and "Next Step / Mark Complete" buttons.
- [x] Store local exercise states in persistent cache layers so that if an app crashes mid-routine, the progress is retained.

---

## Phase 4: Physio Admin Portal & Custom Exercise Builder

Develop the high-density desktop workflow for therapists to manage clinics and prescribe routines.

### Technical Scope & Implementation Rules
*   A responsive, rich-text dashboard for therapists.
*   **The Drag-and-Drop Builder:** Allows therapists to search global library templates, change variables (such as repetitions, sets, frequencies), and assign them dynamically to a specific pet's active program.

### DB Schema Fields To Populate
*   `RehabPrograms`, `RehabProgramExercises`.

### Developer Implementation Checklist

#### Physio Portal Setup (Vue 3)
- [x] Build the interface for the **Physio Dashboard** (showing patient rosters, pending reviews, and today's schedule).
- [x] Build the **"Exercise Program Builder"** page.
- [x] Integrate input controls to configure individual exercise constraints (e.g., modifying "10 reps x 3 sets" to "5 reps x 2 sets" based on patient fatigue).
- [x] Render data visualizations displaying patient program progression over time.

#### Backend API (.NET)
- [x] Build transactional endpoints to assign rehabilitation routines: `POST /api/rehab-programs`.
- [x] Implement logic to soft-delete/deactivate existing programs if a therapist assigns a new, overlapping recovery plan.

---

## Phase 5: Messaging, AI Integration & Video Processing

Enable secure feedback loops via video uploads, automated transcription pipelines, and clinical-grade AI chatbots.

### Technical Scope & Implementation Rules
*   **Video Processing Pipeline:** Owners upload video files. The backend securely pushes them to file storage, triggers a compression pipeline, and updates the database with a web-streamable playback address.
*   **AI Chat (RAG Integration):** A retrieval pipeline that checks the internal Educational Database first. This ensures client answers are anchored strictly in approved veterinary clinical material [5].

```
Owner Video ──► .NET API ──► Google Cloud Storage ──► Google Cloud Transcoder API (HLS Stream) ──► Database Update ──► Physio Portal (Vue 3)
```

### DB Schema Fields To Populate
*   `VideoSubmissions`, `DailyTrackingLogs` (Pain/Mobility scores).

### Developer Implementation Checklist

#### Video Processing Pipeline (Backend)
- [x] Create secure file upload pipelines with format restrictions (allowing only `.mp4`, `.mov`, and `.hevc`).
- [x] Integrate an asynchronous background transcoder service (using local server utilities like `FFmpeg` or cloud transcoding APIs like Google Cloud Transcoder API) to produce lightweight, web-compatible stream paths.
- [x] Ensure that video upload records write tracking audit data (`ModifiedUserId`, `ModifiedDate`) to the database upon status change.

#### AI Chat Framework (.NET & Vertex AI)
- [x] Build a search module that converts user questions into vector queries to fetch relevant educational documents.
- [x] Design system instructions to restrict chatbot behavior: *"You are an assistant for Kruger's Pet Wellness. Answer only with information present in the retrieved texts. If you do not know, suggest booking a consultation with their physiotherapist."*
- [x] Expose chat query routes securely through `POST /api/ai/chat` (verifying active user credentials first).

#### Owner App Setup (Flutter)
- [x] Implement a video recording and upload interface with a visual progress bar.
- [x] Add the interactive sliders interface for owners to record tracking indicators (pain, energy, mobility, and appetite metrics).
- [x] Build the **AI Chat / Messaging** messaging bubble views.

---

## Phase 6: Subscriptions, Payments & Analytics Reports

> **Note:** Payments (Payfast / IAP) are deferred until the product is ready for monetization. Reporting is implemented first.

Scale monetization pathways and export clinical compliance progress metrics.

### Technical Scope & Implementation Rules
*   **Payment Gateways:** Integrate localized payment mechanisms suitable for the South African market (such as **Payfast** or **Peach Payments** for premium monthly billing, alongside Apple App Store / Google Play In-App Purchases).
*   **Analytics Engines:** Build automated PDF generators to print structured reports summarizing clinical exercises, pain trend histories, and overall recovery progression.

### DB Schema Fields To Populate
*   `Users` (SubscriptionTier status fields), `Appointments`.

### Developer Implementation Checklist

#### South African Payment Integrations (Backend & Web/Mobile)
- [ ] Integrate Payfast Subscription API to process recurring billing flows for Premium and Professional subscription tiers.
- [ ] Build secure webhook listeners (`POST /api/payments/payfast-webhook`) to automatically update a user's subscription status.
- [ ] Configure automatic subscription state verification filters across all high-tier backend endpoints (returning `402 Payment Required` when payments fail).

#### Reporting & Analytics Modules (.NET API)
- [x] Implement HTML-to-PDF compilation pipelines (using packages such as `QuestPDF` or `DinkToPdf`) to compile the tracking charts, pain trends, and compliance metrics into professional, ready-to-print PDF clinical reports.
- [x] Build endpoints to download the compiled PDF reports: `GET /api/reports/pet/{petId}/download`.

#### Client App Integrations (Flutter & Vue 3)
- [x] Design analytical interfaces (rendering line graphs, bar charts, and weight change curves).
- [ ] Build the payment gate screen to handle billing upgrade flows.

---

# Brief Gap Backlog (post–Phase 5/6)

Phases 1–5 deliver the core rehab loop. Remaining client-brief gaps (appointments, messaging, reminders, education hub UI, owner progress polish, soft tier gating, and optional polish) are tracked in:

**→ [`gap_backlog.md`](./gap_backlog.md)**

Recommended MVP close-out order in that doc: **Phase A → B → C**, then Phase 6 payments when monetising. Future Features from the pitch (3D models, AI gait, insurance, etc.) stay out of MVP.