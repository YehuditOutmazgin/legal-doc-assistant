# AWS S3 Setup & Testing Guide

## Prerequisites
- AWS Account
- AWS CLI installed

## Step 1: Install AWS CLI

### Windows:
Download from: https://aws.amazon.com/cli/

Or using winget:
```cmd
winget install Amazon.AWSCLI
```

### Verify installation:
```cmd
aws --version
```

## Step 2: Configure AWS Credentials

Run the following command and enter your credentials:
```cmd
aws configure
```

You'll be prompted for:
- **AWS Access Key ID**: Your access key from AWS Console
- **AWS Secret Access Key**: Your secret key from AWS Console
- **Default region name**: `us-east-1`
- **Default output format**: `json`

### How to get AWS credentials:
1. Go to AWS Console: https://console.aws.amazon.com/
2. Click your username (top right) → Security credentials
3. Scroll to "Access keys" → Create access key
4. Save both Access Key ID and Secret Access Key

## Step 3: Create S3 Bucket

```cmd
aws s3 mb s3://legaldoc-dev --region us-east-1
```

### Verify bucket was created:
```cmd
aws s3 ls
```

You should see `legaldoc-dev` in the list.

## Step 4: Configure Bucket Security

### Block public access (IMPORTANT for security):
```cmd
aws s3api put-public-access-block --bucket legaldoc-dev --public-access-block-configuration "BlockPublicAcls=true,IgnorePublicAcls=true,BlockPublicPolicy=true,RestrictPublicBuckets=true"
```

### Enable encryption:
```cmd
aws s3api put-bucket-encryption --bucket legaldoc-dev --server-side-encryption-configuration "{\"Rules\":[{\"ApplyServerSideEncryptionByDefault\":{\"SSEAlgorithm\":\"AES256\"}}]}"
```

## Step 5: Test Connection

### Test 1: Upload a test file
```cmd
echo "Test file" > test.txt
aws s3 cp test.txt s3://legaldoc-dev/test.txt
```

### Test 2: List files in bucket
```cmd
aws s3 ls s3://legaldoc-dev/
```

### Test 3: Download the file
```cmd
aws s3 cp s3://legaldoc-dev/test.txt downloaded-test.txt
```

### Test 4: Delete test file
```cmd
aws s3 rm s3://legaldoc-dev/test.txt
del test.txt
del downloaded-test.txt
```

## Step 6: Test API Integration

### 1. Restore NuGet packages:
```cmd
dotnet restore
```

### 2. Build the project:
```cmd
dotnet build
```

### 3. Run the API:
```cmd
cd LegalDoc.API
dotnet run
```

### 4. Test S3 endpoints using Swagger:
- Open browser: `https://localhost:7XXX/swagger` (check console for actual port)
- Login to get JWT token
- Test the new endpoints:
  - `POST /api/contracts/{id}/upload-url` - Get upload URL
  - `GET /api/contracts/{id}/download/docx` - Get download URL
  - `POST /api/contracts/{id}/from-template/{templateId}` - Copy template

## Troubleshooting

### Error: "Unable to find credentials"
- Run `aws configure` again
- Make sure credentials are saved in `C:\Users\YourUsername\.aws\credentials`

### Error: "Bucket does not exist"
- Verify bucket name in appsettings.json matches the created bucket
- Check region is correct (us-east-1)

### Error: "Access Denied"
- Make sure your AWS user has S3 permissions
- Check IAM policies in AWS Console

### Error: "The security token included in the request is invalid"
- Your AWS credentials might be expired or incorrect
- Run `aws configure` again with valid credentials

## Bucket Structure

After setup, your bucket will have this structure:
```
legaldoc-dev/
├── templates/              # Base .docx templates
│   └── {templateId}.docx
├── contracts/              # Contract files organized by date
│   └── {year}/
│       └── {month}/
│           ├── {contractId}.docx
│           └── {contractId}.pdf
└── temp/                   # Temporary uploads (24h TTL)
    └── {guid}.docx
```

## Security Notes

✅ **DO:**
- Keep AWS credentials secure and never commit them to git
- Use IAM roles in production (no keys needed)
- Enable bucket encryption
- Block all public access
- Use pre-signed URLs with expiration

❌ **DON'T:**
- Never put credentials in appsettings.json (keep them empty)
- Never make bucket public
- Never commit .aws/credentials file
- Never share your access keys

## Production Deployment

For production (Lambda/EC2):
1. Create IAM Role with S3 permissions
2. Attach role to Lambda/EC2 instance
3. Remove AccessKey/SecretKey from appsettings.json
4. AWS SDK will automatically use the IAM role

No credentials needed in production! 🎉
