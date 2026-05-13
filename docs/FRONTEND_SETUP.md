# Frontend Setup & Deployment Guide

## Quick Start

### 1. Install Dependencies

```bash
cd frontend
npm install
```

### 2. Development Server

```bash
npm run dev
```

Opens at `http://localhost:5173`

### 3. Build for Production

```bash
npm run build
```

Output: `frontend/dist/`

## Project Structure

```
frontend/
├── src/
│   ├── types/index.ts           # TypeScript interfaces
│   ├── services/
│   │   ├── auth.ts              # Authentication service
│   │   ├── api.ts               # API client
│   │   ├── ui.ts                # UI helpers
│   │   └── sw.ts                # Service Worker registration
│   ├── pages/
│   │   ├── login.ts             # Login page logic
│   │   ├── dashboard.ts         # Dashboard logic
│   │   ├── contracts.ts         # Contracts page logic
│   │   ├── clients.ts           # Clients page logic
│   │   └── templates.ts         # Templates page logic
│   ├── style.css                # Global styles
│   └── main.ts                  # Entry point
├── public/
│   ├── sw.js                    # Service Worker
│   └── manifest.json            # PWA manifest
├── index.html                   # Login page
├── dashboard.html               # Dashboard
├── contracts.html               # Contracts management
├── clients.html                 # Clients management
├── templates.html               # Templates management
└── package.json
```

## Configuration

### API Base URL

Edit `frontend/src/services/api.ts`:

```typescript
const API_BASE = 'https://localhost:7001/api';  // Change for production
```

### CORS on Backend

The API already has CORS configured in `LegalDoc.API/Program.cs`:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://127.0.0.1:5500")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();  // Required for HttpOnly cookies
    });
});
```

For production, update the origins:

```csharp
policy.WithOrigins("https://yourdomain.com")
```

## Authentication Flow

1. **Login**: User enters email/password
2. **Token Response**: API returns JWT token + user data
3. **Storage**: 
   - Access token → `sessionStorage` (cleared on tab close)
   - Refresh token → HttpOnly cookie (auto-sent by browser)
4. **Auto-Refresh**: On 401, frontend calls `/auth/refresh` to get new token
5. **Logout**: Clears both token and user data

## PWA Features

- **Offline Support**: Service Worker caches static assets
- **Manifest**: PWA manifest for installability
- **Icons**: Add 192x192 and 512x512 PNG icons to `public/icons/`

## Deployment

### Option 1: Serve with Node.js

```bash
npm install -g serve
serve -s dist -l 3000
```

### Option 2: Docker

```dockerfile
FROM node:18-alpine
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build
EXPOSE 3000
CMD ["npm", "run", "preview"]
```

### Option 3: Static Hosting (Vercel, Netlify, etc.)

1. Build: `npm run build`
2. Deploy `dist/` folder
3. Configure API URL for production

## Environment Variables

Create `.env` file (not committed):

```
VITE_API_BASE=https://api.yourdomain.com
```

Update `src/services/api.ts`:

```typescript
const API_BASE = import.meta.env.VITE_API_BASE || 'https://localhost:7001/api';
```

## Troubleshooting

### CORS Errors

- Ensure API has `AllowCredentials()` enabled
- Check that frontend origin is in CORS policy
- Verify `credentials: 'include'` in fetch calls

### 401 Unauthorized

- Check if token is being sent in Authorization header
- Verify JWT secret matches between frontend and API
- Check token expiration time

### Service Worker Not Registering

- Check browser console for errors
- Ensure `sw.js` is in `public/` folder
- Verify HTTPS in production (SW requires secure context)

## Development Tips

- Use `npm run dev` for hot reload
- Check browser DevTools → Application tab for Service Worker status
- Use `npm run build` to test production build locally
- Run `npm run preview` to serve production build

## TypeScript

- All code is fully typed
- Run `npm run build` to check for type errors
- No need to run `tsc` separately (Vite handles it)

## Next Steps

1. Add PWA icons to `public/icons/`
2. Update API URL for your environment
3. Test authentication flow
4. Deploy to production
