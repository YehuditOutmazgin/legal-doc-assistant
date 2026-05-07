# LegalDoc.Lambda — Project Context

## Project Type
AWS Lambda function, .NET 8, triggered by S3 PUT events on the `contracts/` prefix.
Entry point is `Function.cs`, method `FunctionHandler`.

## Structure
```
LegalDoc.Lambda/
├── Function.cs                      # Lambda handler — S3 trigger logic
└── aws-lambda-tools-defaults.json   # Deploy configuration
```

## What This Function Does
1. Receives S3 event (new .docx uploaded to `contracts/`)
2. Downloads the .docx from S3 to `/tmp/`
3. Converts to PDF using LibreOffice headless
4. Uploads PDF back to S3 under the same path with `.pdf` extension
5. Calls back to the API (or SNS) to update `PDF_S3_KEY` in Oracle

## Key Rules
- Runtime: .NET 8 on Amazon Linux 2
- Memory: 1024 MB minimum (LibreOffice requires it)
- Timeout: 5 minutes
- Temp storage: `/tmp/` only — Lambda has no other writable filesystem
- LibreOffice is provided as a Lambda Layer — do not bundle it in the deployment package
- Use `AWSSDK.S3` NuGet for all S3 operations
- Use `Amazon.Lambda.S3Events` NuGet for the event model

## Environment Variables
| Variable | Purpose |
|---|---|
| `API_BASE_URL` | URL to notify after PDF is ready |
| `S3_BUCKET_NAME` | Target bucket name |

## Deployment
```bash
dotnet lambda deploy-function LegalDocPdfConverter
```
Configuration lives in `aws-lambda-tools-defaults.json` — do not hardcode region or role ARN in code.

## Common Pitfalls
- `/tmp/` is limited to 512 MB — clean up files after conversion
- LibreOffice layer must match the Lambda runtime architecture (x86_64)
- S3 event may batch multiple records — always loop over `s3Event.Records`, never assume one record
- Do not use `Console.WriteLine` — use `ILambdaContext.Logger` for CloudWatch logs
