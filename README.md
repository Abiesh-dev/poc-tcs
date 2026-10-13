# User Directory

A small full-stack app for keeping a list of people. You can **view all users** and **add a new
user**.

- **Frontend:** React web app with two pages — List and Add
- **Backend:** .NET 8 Web API with full CRUD
- **Database:** SQLite (a single file, no setup needed)

---

## What it does

**List page** — shows every user in a table: Name, Age, City, State, Pincode. You can sort any
column and search the table. It shows a spinner while loading, a friendly message if the list is
empty, and an error with a Retry button if the API is down.

**Add page** — a form to create a user. It checks your input as you submit, shows the error under
each field, and on success sends you back to the List page with a green confirmation message.

---

## Technology used

| Part | What was used |
| ---- | ------------- |
| Frontend | React 18, TypeScript, Vite, React Router, PrimeReact (UI components), Poppins font |
| Backend | .NET 8 Web API (C#), Swagger |
| Database | SQLite, accessed with Entity Framework Core 8 |
| Backend tests | xUnit |
| Frontend tests | Vitest, React Testing Library |
| Containers | Docker and docker compose |

---

## How to run it

### Option 1 — on your machine

You need **.NET 8 SDK** and **Node.js 20+** installed.

**Step 1.** Start the backend. Open a terminal:

```bash
cd backend
dotnet run --project src/UserDirectory.Api
```

The API now runs at **http://localhost:5080**.
You can explore it at **http://localhost:5080/swagger**.

**Step 2.** Start the frontend. Open a *second* terminal:

```bash
cd frontend
npm install
npm run dev
```

**Step 3.** Open **http://localhost:5173** in your browser.

The database file and three sample users are created automatically the first time you run it. You do
not need to create a database or run any SQL by hand.

### Option 2 — with Docker

```bash
docker compose up --build
```

Then open **http://localhost:5173**.

---

## How to run the tests

```bash
cd backend   &&  dotnet test     # 74 tests
cd frontend  &&  npm test        # 51 tests
```

---

## Folder structure

```
tcs-poc/
├── docker-compose.yml        runs the API and the web app together
│
├── backend/
│   └── src/
│       ├── UserDirectory.Domain/          the User itself and its rules
│       ├── UserDirectory.Application/     what the app does (add, update, delete...)
│       ├── UserDirectory.Infrastructure/  database code (EF Core + SQLite)
│       └── UserDirectory.Api/             the web endpoints
│   └── tests/
│       ├── UserDirectory.UnitTests/        tests for small pieces in isolation
│       └── UserDirectory.IntegrationTests/ tests that call the real endpoints
│
└── frontend/
    └── src/
        ├── api/          talks to the backend
        ├── components/   reusable pieces (navbar, spinner, form field, toast)
        ├── pages/        the List page and the Add page
        └── validation/   the form rules
```

**How a request travels** (adding a user):

```
Browser  →  UsersController  →  validation  →  UserService  →  UserRepository  →  SQLite
                                                   ↑
                                      the User entity checks its own rules
```

The controller stays tiny — it never contains business logic. Anything that goes wrong is turned
into a clean error response by one piece of middleware.

---

## The database

SQLite is just **one file**. There is **no server, no username and no password** to configure.

| Where you run it | Where the file is |
| ---------------- | ----------------- |
| `dotnet run` | `backend/src/UserDirectory.Api/bin/Debug/net8.0/data/app.db` |
| Docker | `/data/app.db` inside the container |

One table, `Users`:

| Column | Type | Notes |
| ------ | ---- | ----- |
| Id | number | auto-generated |
| Name | text | required, max 100 |
| Age | number | required, must be 0–120 |
| City | text | required |
| State | text | required |
| Pincode | text | required, max 10 |
| CreatedAtUtc / UpdatedAtUtc | date | set automatically |

**No duplicates:** the same *Name + Pincode* cannot be added twice. Capitalisation is ignored, so
"AARAV SHARMA" with pincode 600001 is treated as the same person as "Aarav Sharma" with 600001. The
same name under a *different* pincode is allowed. This is enforced by a unique index in the
database, so it holds even if someone bypasses the app.

The table is created automatically on first run using EF Core migrations. A plain SQL version is in
`backend/scripts/schema.sql` if you prefer to run it by hand.

---

## The API

Base address: `http://localhost:5080`. Full interactive documentation: `/swagger`.

| Method | URL | What it does | Success |
| ------ | --- | ------------ | ------- |
| GET | `/api/users` | list all users | 200 |
| GET | `/api/users/{id}` | get one user | 200 |
| POST | `/api/users` | create a user | 201 |
| PUT | `/api/users/{id}` | update a user | 200 |
| DELETE | `/api/users/{id}` | delete a user | 204 |
| GET | `/health` | is the API alive? | 200 |

Errors come back in a consistent format:

| Code | Meaning |
| ---- | ------- |
| 400 | something in the form is invalid (the response says which field) |
| 404 | that user does not exist |
| 409 | that Name + Pincode already exists |
| 500 | unexpected error (details are never leaked to the caller) |

**Example — creating a user**

```http
POST /api/users
{ "name": "Aarav Sharma", "age": 32, "city": "Chennai", "state": "Tamil Nadu", "pincode": "600001" }
```

```http
201 Created
{ "id": 1, "name": "Aarav Sharma", "age": 32, "city": "Chennai",
  "state": "Tamil Nadu", "pincode": "600001",
  "createdAtUtc": "2026-10-09T17:14:38.26Z", "updatedAtUtc": "2026-10-09T17:14:38.26Z" }
```

**Example — invalid input**

```http
400 Bad Request
{ "title": "One or more validation errors occurred.",
  "errors": { "Name": ["Name must be between 2 and 100 characters."],
              "Age":  ["Age must be between 0 and 120."] } }
```

### Validation rules

| Field | Rule |
| ----- | ---- |
| name | required, 2–100 characters |
| age | required, whole number, 0–120 |
| city | required |
| state | required |
| pincode | required, 4–10 characters |

These are checked in the browser (instant feedback), again in the API (so curl cannot bypass them),
and once more by the database. Spaces around the edges are trimmed before anything is saved.

---

## Test results

Run on Windows 11 with .NET SDK 8.0.425 and Node 20.20.0. **All 125 tests pass.**

```
backend  $ dotnet test
Passed!  - Failed: 0, Passed: 59   (UserDirectory.UnitTests)
Passed!  - Failed: 0, Passed: 15   (UserDirectory.IntegrationTests)

frontend $ npm test
Test Files  5 passed (5)
     Tests  51 passed (51)
```

What they cover:

- **Unit tests** — the User rules and their edge values (age 0 and 120, 2- and 100-character names),
  the service logic with a stand-in database, the validation rules, and the repository against a
  real in-memory SQLite database.
- **Integration tests** — every endpoint called over real HTTP, checking status codes, the duplicate
  rule, missing users, and malformed JSON.
- **Frontend tests** — the form rules, the API error handling, all four states of the List page
  (loading / has data / empty / error), searching and sorting, and the full add-a-user journey.

The running app was also checked by hand: all five endpoints, the 400/404/409 responses shown above,
the case-insensitive duplicate rule, and Swagger.

---

## Technology and AI tool disclosure

As requested in the brief:

**AI tools used:** I built this project with the guidence of **Claude Code** (Anthropic's command-line coding
assistant, running the **Claude Opus 5** model) as a pair-programming assistant throughout.

I owned the project end to end. I set the requirements, chose the architecture (the four-project
backend split, SQLite with EF Core, PrimeReact for the UI), decided what was in and out of scope,
and reviewed and tested everything that went into the repository. Claude Code was used to accelerate
the typing — scaffolding projects, drafting implementations and tests from my direction, generating
the EF Core migration, and helping debug failures.

Nothing was accepted without being run and checked. Working that way caught several real defects
before they reached the final code, including an entity that could be left half-updated when
validation failed, and SQLite silently dropping the UTC marker from timestamps. Where I disagreed
with a suggested approach I changed it — for example, dropping the OAuth2 bonus entirely so the
project runs with no external accounts, and rewriting this README to be readable by a non-specialist.

I can explain any file in this repository, why it is structured the way it is, and what every test
is proving.

**Third-party packages:** all listed in the *Technology used* table above, plus their standard
dependencies. Everything is open-source and installed from NuGet and npm. No paid services, no
external APIs and no accounts are required to run this project.

---

## What is not included

Honest list of the gaps:

- **No login / authentication.** Every endpoint is open. This was a bonus item in the brief and was
  deliberately left out so the project runs with nothing but this repository — no external accounts
  and no credentials to share.
- **No edit or delete buttons in the UI.** The brief asked for two pages (Add and List), so update
  and delete are available through the API and Swagger only.
- **No paging.** The list returns every user. Fine at this size; a real directory would need paging.
- **The Docker images were not built.** Docker was not installed on the machine used, so
  `docker-compose.yml` and the Dockerfiles are written but untested. The commands inside them
  (`dotnet publish`, `npm run build`) were both run successfully on their own.
- **No automated browser test.** The pages are covered by component tests with a stand-in API; there
  is no test that drives a real browser end to end.
