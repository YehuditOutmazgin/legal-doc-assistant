# LegalDoc Frontend

מערכת ניהול מסמכים משפטיים - ממשק משתמש

## 🚀 התחלה מהירה

### 1. התקנה
```bash
cd frontend
npm install
```

### 2. הגדרת סביבה
הקובץ `.env` כבר מוגדר עם:
```
VITE_API_BASE_URL=https://localhost:7261/api
```

### 3. הרצה
```bash
npm run dev
```

האפליקציה תרוץ על: http://localhost:3000

### 4. אישור HTTPS Certificate
**חשוב!** לפני הלוגין הראשון:
1. פתח בדפדפן: `https://localhost:7261/swagger`
2. אשר את ה-certificate (Advanced → Proceed to localhost)
3. חזור לפרונטאנד

### 5. התחברות
```
Email: admin@legaldoc.com
Password: Admin123!
```

## ⚠️ בעיות נפוצות

### "Failed to fetch"
ראה [QUICK_FIX.md](./QUICK_FIX.md) לפתרון מהיר.

### בעיות נוספות
ראה [TROUBLESHOOTING.md](./TROUBLESHOOTING.md) למדריך מלא.

## 📁 מבנה הפרויקט

```
frontend/
├── public/              # קבצים סטטיים
│   ├── manifest.json   # PWA manifest
│   ├── sw.js           # Service Worker
│   └── icons/          # אייקונים
├── src/
│   ├── api/            # API clients
│   │   ├── http.client.ts    # HTTP client עם JWT + refresh
│   │   └── auth.api.ts       # Auth endpoints
│   ├── components/     # קומפוננטות משותפות
│   │   ├── navbar.ts         # Desktop sidebar
│   │   └── mobile-nav.ts     # Mobile bottom bar
│   ├── pages/          # עמודים
│   │   ├── login/            # עמוד התחברות
│   │   └── dashboard/        # לוח בקרה
│   ├── styles/         # סגנונות
│   │   ├── variables.css     # CSS custom properties
│   │   ├── reset.css         # CSS reset
│   │   ├── global.css        # סגנונות בסיס
│   │   ├── components.css    # קומפוננטות
│   │   ├── pages.css         # עמודים
│   │   └── responsive.css    # מובייל
│   ├── types/          # TypeScript types
│   │   ├── api.types.ts      # Generic API types
│   │   ├── auth.types.ts     # Auth DTOs
│   │   ├── contract.types.ts # Contract DTOs
│   │   ├── client.types.ts   # Client DTOs
│   │   └── template.types.ts # Template DTOs
│   └── utils/          # פונקציות עזר
│       ├── auth.utils.ts     # requireAuth, getUser, hasRole
│       ├── validation.utils.ts # Email, password validation
│       └── date.utils.ts     # Date formatting
├── index.html          # נקודת כניסה
├── vite.config.ts      # הגדרות Vite
├── tsconfig.json       # הגדרות TypeScript
└── package.json
```

## ✅ מה הושלם

### שלב 1: מבנה בסיסי
- [x] מבנה תיקיות מלא
- [x] Vite + TypeScript
- [x] CSS Variables + Components
- [x] PWA Support (manifest, service worker)

### שלב 2: Authentication
- [x] Types (התאמה מלאה לבקאנד)
- [x] HTTP Client עם JWT + Refresh Token
- [x] Login Page מלא
- [x] Auth Utils (requireAuth, getUser, hasRole)
- [x] Dashboard בסיסי
- [x] Navigation (Desktop + Mobile)

## 📋 הבא בתור

### שלב 3: Contracts
- [ ] contracts.html + contracts.ts
- [ ] contract-detail.html + contract-detail.ts
- [ ] contracts.api.ts

### שלב 4: Clients
- [ ] clients.html + clients.ts
- [ ] client-detail.html + client-detail.ts
- [ ] clients.api.ts

### שלב 5: Templates
- [ ] templates.html + templates.ts
- [ ] template-detail.html + template-detail.ts
- [ ] templates.api.ts

## 🎯 תכונות מרכזיות

### אימות (Authentication)
✅ **עובד מצוין!**
- JWT tokens עם automatic refresh
- Cookies (HttpOnly, Secure, SameSite)
- המשתמש נשאר מחובר בין רענונים
- Redirect אוטומטי ללוגין

### Responsive Design
✅ **מוכן!**
- Desktop: Sidebar בצד ימין (250px)
- Mobile: Bottom navigation bar (60px)
- Breakpoint: 768px

### התאמה לבקאנד
✅ **התאמה מלאה!**
- Types זהים ל-DTOs
- Enums זהים
- API endpoints תואמים

## 🛠️ פקודות

```bash
# פיתוח
npm run dev

# בנייה
npm run build

# תצוגה מקדימה
npm run preview

# בדיקת Types
npm run type-check
```

## 📚 מסמכים נוספים

- [SETUP.md](./SETUP.md) - הוראות התקנה מפורטות
- [PROGRESS.md](./PROGRESS.md) - התקדמות הפיתוח
- [AUTHENTICATION_FLOW.md](./AUTHENTICATION_FLOW.md) - הסבר על זרימת האימות
- [TROUBLESHOOTING.md](./TROUBLESHOOTING.md) - פתרון בעיות
- [QUICK_FIX.md](./QUICK_FIX.md) - תיקון מהיר ל-"Failed to fetch"

## 🎨 עיצוב

### צבעים
- Primary: #2563eb (כחול)
- Success: #10b981 (ירוק)
- Warning: #f59e0b (כתום)
- Danger: #ef4444 (אדום)

### Typography
- Font: System fonts
- Sizes: xs(12px), sm(14px), base(16px), lg(18px), xl(20px), 2xl(24px), 3xl(30px)

### Spacing
- xs: 4px, sm: 8px, md: 16px, lg: 24px, xl: 32px, 2xl: 48px

## 🔒 אבטחה

- JWT tokens ב-Cookies (HttpOnly, Secure, SameSite=Strict)
- Automatic token refresh
- CORS מוגדר נכון
- Input validation
- XSS protection

## 📱 PWA

- Service Worker לעבודה offline
- Manifest.json להתקנה כאפליקציה
- Icons מותאמים (צריך להוסיף תמונות)

## 🤝 תרומה

1. צור branch חדש
2. בצע שינויים
3. הרץ `npm run type-check`
4. צור Pull Request

## 📄 רישיון

MIT

