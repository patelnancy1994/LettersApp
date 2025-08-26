# LettersApp (.NET 8 • MVC + Web API)

Generate a single downloadable HTML file with one letter per printed page from a CSV of addressees. Uses a very small custom `$Token` template engine (no external packages).

## Quick start

```bash
# Requires .NET SDK 8.x
cd src
dotnet run
# Open http://localhost:5000  (or printed URL)
```

Or build/publish:
```bash
dotnet publish -c Release -o out
./out/LettersApp
```

## How it works

- **Frontend (MVC)**: simple form at `/` to upload a CSV and (optionally) a custom HTML template.
- **API**: `POST /api/letters/generate` accepts `multipart/form-data` with fields:
  - `Csv` (required): the addressee data
  - `TemplateHtml` (optional): custom HTML template using `$Placeholders`
- The API personalizes the template for each row and returns a single HTML with sections separated by `page-break-after` for printing.

### CSV columns

```
ContactPerson,StreetAddress,Suburb,State,PostCode,FirstName,CardNumber,ExpireDate
```

Dates are treated as plain text and not parsed.

### Placeholders

Use `$FieldName` inside your template. Supported fields:
- `$ContactPerson`, `$StreetAddress`, `$Suburb`, `$State`, `$PostCode`
- `$FirstName`, `$CardNumber`, `$ExpireDate`
- `$MyCompanyPhoneNumber` (from `appsettings.json`)

> Heads-up: the default template also recognizes `$CardNunber` (typo) and maps it to `$CardNumber`.

### Fixed sender values

Configured in `appsettings.json` under `Sender`. Update PhoneNumber to change `$MyCompanyPhoneNumber`.

### Notes

- The default template is included at `wwwroot/templates/SixtyDaysLetterPrompt.html`.
- Output HTML adds `<section class="page">...</section>` per addressee and a `@media print` rule to ensure one page per letter.
- No DB or queues required; everything is in-memory per request.

## Endpoints

- `GET /` — upload form
- `POST /Upload/Generate` — submits to the API and streams back the HTML download
- `POST /api/letters/generate` — API endpoint

## Sample CSV

```
ContactPerson,StreetAddress,Suburb,State,PostCode,FirstName,CardNumber,ExpireDate
Alex Smith,101 Example St,Westside,CA,90210,Alex,123456,2025-07-01
Jamie Lee,55 Harbor Rd,Santa Cruz,CA,95060,Jamie,987654,2025-06-15
```

## Print

Open the downloaded HTML file in a browser and print. Each section is forced to a new page when printing.
