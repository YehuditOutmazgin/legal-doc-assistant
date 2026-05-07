# LegalDoc.WordAddin — Project Context

## Critical Framework Rule
This project targets **.NET Framework 4.8** — never .NET 8 or .NET Core.
If asked to add a NuGet package, verify it supports .NET Framework 4.8 before installing.

## Project Type
VSTO Word Add-in. Entry point is `ThisAddIn.cs`, not `Program.cs`.

## Structure
```
LegalDoc.WordAddin/
├── Ribbon/
│   ├── LegalRibbon.cs       # Event handlers for ribbon buttons
│   └── LegalRibbon.xml      # Custom ribbon UI definition
├── TaskPane/
│   └── ContractPane.cs      # WinForms custom task pane
├── Services/
│   └── ApiClient.cs         # HttpClient wrapper for API calls
├── ThisAddIn.cs             # VSTO entry point, add-in lifecycle
└── VbaMacros.bas            # Exported to Word template
```

## Key Rules
- UI is WinForms only — no WPF, no XAML
- Ribbon defined in XML (`LegalRibbon.xml`), not designer
- Task pane inherits from `UserControl`
- JWT token stored in memory (add-in session) — never on disk
- All API calls go through `ApiClient.cs` — no direct HttpClient usage elsewhere
- VBA macros fill Word Content Controls — C# calls VBA via `Word.Application` COM object
- Use `Globals.ThisAddIn.Application` to access the Word COM object

## API Communication
- Base URL loaded from add-in settings (`app.config`)
- All requests include `Authorization: Bearer {token}` header
- Deserialize responses with `System.Text.Json` or `Newtonsoft.Json`

## Common Pitfalls
- Do not use `async/await` on ribbon button click handlers — VSTO COM threading will deadlock. Use `Task.Run` carefully or synchronous calls.
- Do not reference .NET 8 projects directly — use a shared NuGet package or duplicate the DTOs
- Word COM objects must be released with `Marshal.ReleaseComObject` to avoid memory leaks
