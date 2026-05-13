# Quick Start Guide

## 🚀 Start the Full Stack

### Terminal 1: Database (if needed)
```bash
cd Database
./run_all.sql  # Or run_setup.bat on Windows
```

### Terminal 2: Backend API
```bash
cd LegalDoc.API
dotnet run
```

API runs at: `https://localhost:7001`

### Terminal 3: Frontend
```bash
cd frontend
npm install  # First time only
npm run dev
```

Frontend runs at: `http://localhost:5173`

## 🔐 Login Credentials

Use credentials from `Database/03_seed_data.sql`:

```
Email: admin@legaldoc.com
Password: Admin@123
```

Or:

```
Email: lawyer@legaldoc.com
Password: Lawyer@123
```

## ✅ Verify Everything Works

1. **Frontend loads**: http://localhost:5173
2. **Login page appears**: Dark theme with LegalDoc logo
3. **Login succeeds**: Redirects to dashboard
4. **Dashboard loads**: Shows stats and recent contracts
5. **Navigation works**: Can click between pages
6. **API calls work**: Data loads from backend

## 📦 Build for Production

```bash
cd frontend
npm run build
```

Output: `frontend/dist/`

## 🐛 Troubleshooting

### CORS Error
- Ensure API is running on `https://localhost:7001`
- Check `LegalDoc.API/Program.cs` has CORS configured

### 401 Unauthorized
- Check login credentials
- Verify JWT secret in `appsettings.json`
- Check token expiration

### Frontend won't load
- Ensure Node.js is installed: `node --version`
- Clear node_modules: `rm -r node_modules && npm install`
- Check port 5173 is available

### Service Worker issues
- Check browser DevTools → Application → Service Workers
- Clear cache: DevTools → Application → Clear storage

## 📚 Documentation

- `FRONTEND_SETUP.md` — Detailed frontend setup
- `IMPLEMENTATION_SUMMARY.md` — What was implemented
- `frontend/README.md` — Frontend project info
- `LegalDoc.API/README.md` — Backend API info
- `Database/README.md` — Database setup

## 🎯 Next Steps

1. Test all CRUD operations (Create, Read, Update, Delete)
2. Test authentication flow (login, logout, token refresh)
3. Test offline mode (DevTools → Network → Offline)
4. Add PWA icons to `frontend/public/icons/`
5. Deploy to production

---

**Everything is ready to go!** 🎉
