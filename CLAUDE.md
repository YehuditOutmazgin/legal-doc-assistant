# LegalDoc Assistant — Project Context for Claude

## Project Overview
Legal document automation tool for lawyers. Enables creation, editing, management, and archiving of legal documents. Built as a Portfolio Project targeting a Full Stack Developer position at Ness Technologies.

## Tech Stack
| Layer | Technology | Notes |
|---|---|---|
| Frontend | HTML5, CSS3, Vanilla JavaScript | PWA, offline-capable |
| Backend | ASP.NET Core Web API | C# .NET 8 |
| Database | Oracle Database | Local XE for dev, AWS RDS for prod |
| Office Integration | VSTO Word Add-in | .NET Framework 4.8, Visual Studio 2022 |
| Office Automation | VBA Macros | Document template automation |
| Cloud Storage | Amazon S3 | Separate buckets per environment |
| Cloud Compute | AWS Lambda (.NET 8) | Word-to-PDF conversion |
| Cloud API | AWS API Gateway | REST, integrated with Lambda |
| Auth | JWT Bearer Tokens | ASP.NET Core Identity |

## Solution Structure
```
LegalDocAssistant.sln
├── LegalDoc.Core/           # Class Library .NET 8 — no external dependencies
│   ├── Models/
│   ├── Interfaces/
│   ├── DTOs/
│   └── Enums/
├── LegalDoc.Infrastructure/ # Class Library .NET 8
│   ├── Repositories/        # Oracle implementations
│   ├── Services/            # S3Service, AuditService
│   └── Configuration/
├── LegalDoc.API/            # ASP.NET Core 8 Web API
│   ├── Controllers/
│   ├── Middleware/
│   └── Configuration/
├── LegalDoc.WordAddin/      # VSTO — .NET Framework 4.8 (NOT .NET 8)
│   ├── Ribbon/
│   ├── TaskPane/
│   └── Services/
├── LegalDoc.Lambda/         # AWS Lambda .NET 8
├── LegalDoc.Tests/          # xUnit
└── frontend/                # PWA — plain folder, not a VS project
    ├── index.html
    ├── app.js
    ├── styles.css
    ├── sw.js
    └── manifest.json
```

## Architecture Principles
- **Pattern:** Repository Pattern with Interface segregation
- **Dependency direction:** API → Infrastructure → Core. Core has zero dependencies.
- **Database access:** ODP.NET with parameterized queries only. No Entity Framework.
- **All DB calls must be async** using `await` / `Task<T>`
- **No raw SQL string concatenation** — always use OracleParameter
- **S3 access** via AWSSDK.S3 NuGet package with pre-signed URLs for downloads

## Oracle Database Schema
Five main tables:
- `USERS` — system users (ADMIN / LAWYER / CLERK roles)
- `CLIENTS` — law firm clients (INDIVIDUAL / COMPANY)
- `TEMPLATES` — contract templates with JSON field definitions + S3 key for base .docx
- `CONTRACTS` — created contracts with status workflow: DRAFT → REVIEW → SIGNED → ARCHIVED
- `AUDIT_LOG` — full audit trail for every CREATE / UPDATE / DELETE / DOWNLOAD action

Primary keys use Oracle Sequences. All PKs are `NUMBER` type named `{TABLE}_ID`.

## Authentication & Authorization
- JWT Bearer Tokens, algorithm HS256
- Access Token TTL: 60 minutes
- Refresh Token TTL: 7 days, stored in HttpOnly Cookie
- Three roles: ADMIN, LAWYER, CLERK
- Every protected endpoint has two layers: Authentication check + Authorization policy check
- Passwords stored as bcrypt hash only — never plain text

## Authorization Policies
| Policy | Roles |
|---|---|
| RequireAdmin | ADMIN |
| RequireLawyer | LAWYER, ADMIN |
| RequireAnyUser | All roles |
| OwnerOrAdmin | ADMIN + resource owner |

## API Conventions
- Base path: `/api/`
- All responses use consistent JSON envelope
- Pagination on list endpoints: `page`, `pageSize` query params
- Soft delete only — no hard deletes except via ADMIN archive action
- All contract file operations return pre-signed S3 URLs, not raw file bytes

## AWS Infrastructure
- S3 bucket structure: `templates/`, `contracts/{year}/{month}/`, `temp/` (TTL 24h)
- Lambda trigger: S3 PUT event on `contracts/` prefix → converts .docx to PDF via LibreOffice headless
- Lambda runtime: .NET 8 on Amazon Linux 2, memory 1024 MB, timeout 5 minutes
- IAM: least-privilege role per service

## VSTO Add-in Rules
- Target framework: .NET Framework 4.8 — never .NET 8
- UI: Custom Ribbon XML + WinForms Task Pane
- API calls via `HttpClient` with JWT token stored in add-in session
- VBA macros handle Content Controls filling in Word documents
- C# calls VBA via `Word.Application` COM object

## Code Style
- Use `async/await` everywhere IO is involved
- Use `ILogger<T>` for all logging — no `Console.WriteLine` in production code
- DTOs for all API input/output — never expose domain models directly
- Validate input at Controller level using Data Annotations or FluentValidation
- All Oracle repository methods must catch `OracleException` and wrap in domain exceptions

## Key Files to Know
| File | Purpose |
|---|---|
| `LegalDoc.API/Program.cs` | DI registration, JWT config, middleware pipeline |
| `LegalDoc.Core/Interfaces/IContractRepository.cs` | Contract data access contract |
| `LegalDoc.Infrastructure/Repositories/OracleContractRepository.cs` | Main data access implementation |
| `LegalDoc.API/Controllers/AuthController.cs` | Login, logout, token refresh |
| `LegalDoc.WordAddin/ThisAddIn.cs` | VSTO entry point |
| `LegalDoc.Lambda/Function.cs` | S3 trigger → PDF conversion |
| `frontend/sw.js` | PWA service worker — caching strategy |

## Build Order (Dependency Chain)
1. `LegalDoc.Core` — no dependencies, build first
2. `LegalDoc.Infrastructure` — references Core
3. `LegalDoc.API` — references Core + Infrastructure
4. `LegalDoc.WordAddin` — references Core (via NuGet or project ref with binding redirects)
5. `LegalDoc.Lambda` — references Core + AWSSDK
6. `LegalDoc.Tests` — references all above
7. `frontend/` — standalone, no build step required

## What NOT to Do
- Do not use Entity Framework — ODP.NET only
- Do not target .NET 8 in the VSTO project — it must be .NET Framework 4.8
- Do not store AWS credentials in code or appsettings — use IAM roles or environment variables
- Do not return S3 file bytes directly from API — always use pre-signed URLs
- Do not skip the AUDIT_LOG insert on any data mutation
