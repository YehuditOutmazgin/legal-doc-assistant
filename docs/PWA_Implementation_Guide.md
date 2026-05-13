# PWA Frontend Implementation Guide
## LegalDoc Assistant — Vanilla JS + HTML5 + CSS3

---

## Architecture Overview

```
frontend/
├── index.html          # Entry point — login page
├── dashboard.html      # Main dashboard after login
├── contracts.html      # Contracts list + management
├── clients.html        # Clients list + management
├── templates.html      # Templates list + management
├── app.js              # Core: API calls, auth, routing
├── styles.css          # Global styles + CSS variables
├── sw.js               # Service Worker — offline + caching
└── manifest.json       # PWA manifest
```

**Key principle:** No framework, no build step.
Vanilla JS with fetch API. All API calls go through one central `api.js` module.
The Service Worker handles offline capability and caching.

---

## Step 1 — manifest.json

```json
{
  "name": "LegalDoc Assistant",
  "short_name": "LegalDoc",
  "description": "Legal document management system",
  "start_url": "/",
  "display": "standalone",
  "background_color": "#0f172a",
  "theme_color": "#1e293b",
  "orientation": "portrait-primary",
  "icons": [
    {
      "src": "icons/icon-192.png",
      "sizes": "192x192",
      "type": "image/png",
      "purpose": "any maskable"
    },
    {
      "src": "icons/icon-512.png",
      "sizes": "512x512",
      "type": "image/png",
      "purpose": "any maskable"
    }
  ]
}
```

---

## Step 2 — sw.js (Service Worker)

```javascript
const CACHE_NAME = 'legaldoc-v1';
const STATIC_ASSETS = [
  '/',
  '/index.html',
  '/dashboard.html',
  '/contracts.html',
  '/clients.html',
  '/templates.html',
  '/styles.css',
  '/app.js'
];

// Install — cache static assets
self.addEventListener('install', event => {
  event.waitUntil(
    caches.open(CACHE_NAME)
      .then(cache => cache.addAll(STATIC_ASSETS))
      .then(() => self.skipWaiting())
  );
});

// Activate — clean old caches
self.addEventListener('activate', event => {
  event.waitUntil(
    caches.keys().then(keys =>
      Promise.all(keys
        .filter(key => key !== CACHE_NAME)
        .map(key => caches.delete(key))
      )
    ).then(() => self.clients.claim())
  );
});

// Fetch strategy:
// - Static assets: Cache First
// - API calls: Network First (never cache auth/data)
self.addEventListener('fetch', event => {
  const url = new URL(event.request.url);

  // Never cache API calls
  if (url.pathname.startsWith('/api/')) {
    event.respondWith(fetch(event.request));
    return;
  }

  // Cache first for static assets
  event.respondWith(
    caches.match(event.request)
      .then(cached => cached || fetch(event.request))
  );
});
```

---

## Step 3 — app.js (Core Module)

```javascript
// ============================================================
// CONFIG
// ============================================================
const API_BASE = 'https://localhost:7001/api'; // Change for production

// ============================================================
// AUTH MODULE
// ============================================================
const Auth = {
  getToken: () => sessionStorage.getItem('accessToken'),
  
  setToken: (token) => sessionStorage.setItem('accessToken', token),
  
  clearToken: () => sessionStorage.removeItem('accessToken'),
  
  getUser: () => {
    const raw = sessionStorage.getItem('currentUser');
    return raw ? JSON.parse(raw) : null;
  },
  
  setUser: (user) => sessionStorage.setItem('currentUser', JSON.stringify(user)),
  
  isAuthenticated: () => !!sessionStorage.getItem('accessToken'),
  
  hasRole: (role) => {
    const user = Auth.getUser();
    return user?.role === role;
  },

  requireAuth: () => {
    if (!Auth.isAuthenticated()) {
      window.location.href = '/index.html';
      return false;
    }
    return true;
  }
};

// ============================================================
// API MODULE — all HTTP calls go through here
// ============================================================
const Api = {
  async request(method, endpoint, body = null) {
    const headers = { 'Content-Type': 'application/json' };
    const token = Auth.getToken();
    if (token) headers['Authorization'] = `Bearer ${token}`;

    const options = { method, headers, credentials: 'include' }; // include for cookie
    if (body) options.body = JSON.stringify(body);

    try {
      const res = await fetch(`${API_BASE}${endpoint}`, options);

      // Token expired — try refresh
      if (res.status === 401) {
        const refreshed = await Api.refresh();
        if (refreshed) return Api.request(method, endpoint, body);
        Auth.clearToken();
        window.location.href = '/index.html';
        return null;
      }

      if (!res.ok) {
        const error = await res.json().catch(() => ({ message: 'Unknown error' }));
        throw new Error(error.message || `HTTP ${res.status}`);
      }

      if (res.status === 204) return null; // No content
      return await res.json();

    } catch (err) {
      if (err.message !== 'Failed to fetch') throw err;
      UI.showToast('No internet connection. Working offline.', 'warning');
      return null;
    }
  },

  get: (endpoint) => Api.request('GET', endpoint),
  post: (endpoint, body) => Api.request('POST', endpoint, body),
  put: (endpoint, body) => Api.request('PUT', endpoint, body),
  delete: (endpoint) => Api.request('DELETE', endpoint),

  async refresh() {
    try {
      const res = await fetch(`${API_BASE}/auth/refresh`, {
        method: 'POST',
        credentials: 'include' // sends HttpOnly cookie
      });
      if (!res.ok) return false;
      const data = await res.json();
      Auth.setToken(data.token);
      return true;
    } catch {
      return false;
    }
  },

  // Auth endpoints
  auth: {
    login: (email, password) => Api.post('/auth/login', { email, password }),
    logout: () => Api.post('/auth/logout'),
    me: () => Api.get('/auth/me')
  },

  // Contracts endpoints
  contracts: {
    getAll: () => Api.get('/contracts'),
    getById: (id) => Api.get(`/contracts/${id}`),
    create: (data) => Api.post('/contracts', data),
    update: (id, data) => Api.put(`/contracts/${id}`, data),
    delete: (id) => Api.delete(`/contracts/${id}`),
    getDownloadUrl: (id, type) => Api.post(`/contracts/${id}/download/${type}`),
    getUploadUrl: (id) => Api.post(`/contracts/${id}/upload-url`)
  },

  // Clients endpoints
  clients: {
    getAll: () => Api.get('/clients'),
    getById: (id) => Api.get(`/clients/${id}`),
    create: (data) => Api.post('/clients', data),
    update: (id, data) => Api.put(`/clients/${id}`, data),
    delete: (id) => Api.delete(`/clients/${id}`)
  },

  // Templates endpoints
  templates: {
    getAll: () => Api.get('/templates'),
    getById: (id) => Api.get(`/templates/${id}`),
    create: (data) => Api.post('/templates', data),
    update: (id, data) => Api.put(`/templates/${id}`, data),
    delete: (id) => Api.delete(`/templates/${id}`)
  }
};

// ============================================================
// UI HELPERS
// ============================================================
const UI = {
  showToast(message, type = 'info') {
    const toast = document.createElement('div');
    toast.className = `toast toast--${type}`;
    toast.textContent = message;
    document.body.appendChild(toast);
    setTimeout(() => toast.classList.add('toast--visible'), 10);
    setTimeout(() => {
      toast.classList.remove('toast--visible');
      setTimeout(() => toast.remove(), 300);
    }, 3500);
  },

  showLoading(container) {
    container.innerHTML = `
      <div class="loading">
        <div class="loading__spinner"></div>
        <p>Loading...</p>
      </div>`;
  },

  showError(container, message) {
    container.innerHTML = `
      <div class="error-state">
        <p>⚠️ ${message}</p>
        <button onclick="location.reload()">Try again</button>
      </div>`;
  },

  showEmpty(container, message) {
    container.innerHTML = `
      <div class="empty-state">
        <p>${message}</p>
      </div>`;
  },

  formatDate(dateStr) {
    return new Date(dateStr).toLocaleDateString('he-IL', {
      year: 'numeric', month: 'short', day: 'numeric'
    });
  },

  statusBadge(status) {
    const map = {
      DRAFT: { label: 'Draft', class: 'badge--draft' },
      REVIEW: { label: 'In Review', class: 'badge--review' },
      SIGNED: { label: 'Signed', class: 'badge--signed' },
      ARCHIVED: { label: 'Archived', class: 'badge--archived' }
    };
    const s = map[status] || { label: status, class: '' };
    return `<span class="badge ${s.class}">${s.label}</span>`;
  }
};

// ============================================================
// SERVICE WORKER REGISTRATION
// ============================================================
if ('serviceWorker' in navigator) {
  window.addEventListener('load', () => {
    navigator.serviceWorker.register('/sw.js')
      .catch(err => console.warn('SW registration failed:', err));
  });
}
```

---

## Step 4 — index.html (Login Page)

```html
<!DOCTYPE html>
<html lang="he" dir="rtl">
<head>
  <meta charset="UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <meta name="theme-color" content="#1e293b">
  <title>LegalDoc Assistant — Login</title>
  <link rel="stylesheet" href="styles.css">
  <link rel="manifest" href="manifest.json">
</head>
<body class="login-page">

  <div class="login-container">
    <div class="login-card">
      <div class="login-logo">
        <span class="logo-icon">⚖️</span>
        <h1>LegalDoc</h1>
        <p>Legal Document Management System</p>
      </div>

      <form id="loginForm" class="login-form" novalidate>
        <div class="form-group">
          <label for="email">Email</label>
          <input type="email" id="email" name="email"
                 placeholder="your@email.com" required autocomplete="email">
        </div>

        <div class="form-group">
          <label for="password">Password</label>
          <input type="password" id="password" name="password"
                 placeholder="••••••••" required autocomplete="current-password">
        </div>

        <div id="loginError" class="form-error" hidden></div>

        <button type="submit" class="btn btn--primary btn--full" id="loginBtn">
          Sign In
        </button>
      </form>
    </div>
  </div>

  <script src="app.js"></script>
  <script>
    // Redirect if already logged in
    if (Auth.isAuthenticated()) {
      window.location.href = '/dashboard.html';
    }

    document.getElementById('loginForm').addEventListener('submit', async (e) => {
      e.preventDefault();
      const btn = document.getElementById('loginBtn');
      const errorEl = document.getElementById('loginError');

      btn.disabled = true;
      btn.textContent = 'Signing in...';
      errorEl.hidden = true;

      try {
        const email = document.getElementById('email').value;
        const password = document.getElementById('password').value;

        const result = await Api.auth.login(email, password);
        if (!result) return;

        Auth.setToken(result.token);
        Auth.setUser(result.user);
        window.location.href = '/dashboard.html';

      } catch (err) {
        errorEl.textContent = err.message || 'Login failed. Please try again.';
        errorEl.hidden = false;
      } finally {
        btn.disabled = false;
        btn.textContent = 'Sign In';
      }
    });
  </script>
</body>
</html>
```

---

## Step 5 — dashboard.html (Main Dashboard)

```html
<!DOCTYPE html>
<html lang="he" dir="rtl">
<head>
  <meta charset="UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <title>LegalDoc — Dashboard</title>
  <link rel="stylesheet" href="styles.css">
  <link rel="manifest" href="manifest.json">
</head>
<body>

  <nav class="navbar">
    <div class="navbar__brand">⚖️ LegalDoc</div>
    <ul class="navbar__links">
      <li><a href="/dashboard.html" class="active">Dashboard</a></li>
      <li><a href="/contracts.html">Contracts</a></li>
      <li><a href="/clients.html">Clients</a></li>
      <li><a href="/templates.html">Templates</a></li>
    </ul>
    <div class="navbar__user">
      <span id="userName"></span>
      <button id="logoutBtn" class="btn btn--ghost btn--sm">Logout</button>
    </div>
  </nav>

  <main class="main-content">
    <div class="page-header">
      <h1>Dashboard</h1>
      <p>Welcome back, <span id="welcomeName"></span></p>
    </div>

    <div class="stats-grid" id="statsGrid">
      <div class="stat-card">
        <div class="stat-card__value" id="statContracts">—</div>
        <div class="stat-card__label">Total Contracts</div>
      </div>
      <div class="stat-card">
        <div class="stat-card__value" id="statDraft">—</div>
        <div class="stat-card__label">In Draft</div>
      </div>
      <div class="stat-card">
        <div class="stat-card__value" id="statReview">—</div>
        <div class="stat-card__label">Under Review</div>
      </div>
      <div class="stat-card">
        <div class="stat-card__value" id="statClients">—</div>
        <div class="stat-card__label">Active Clients</div>
      </div>
    </div>

    <div class="dashboard-grid">
      <section class="card">
        <div class="card__header">
          <h2>Recent Contracts</h2>
          <a href="/contracts.html" class="btn btn--ghost btn--sm">View all</a>
        </div>
        <div id="recentContracts"><div class="loading__spinner"></div></div>
      </section>
    </div>
  </main>

  <script src="app.js"></script>
  <script>
    if (!Auth.requireAuth()) throw new Error('Not authenticated');

    const user = Auth.getUser();
    document.getElementById('userName').textContent = user?.email || '';
    document.getElementById('welcomeName').textContent = user?.firstName || '';

    document.getElementById('logoutBtn').addEventListener('click', async () => {
      await Api.auth.logout();
      Auth.clearToken();
      window.location.href = '/index.html';
    });

    async function loadDashboard() {
      const [contracts, clients] = await Promise.all([
        Api.contracts.getAll(),
        Api.clients.getAll()
      ]);

      if (contracts) {
        document.getElementById('statContracts').textContent = contracts.length;
        document.getElementById('statDraft').textContent =
          contracts.filter(c => c.status === 'DRAFT').length;
        document.getElementById('statReview').textContent =
          contracts.filter(c => c.status === 'REVIEW').length;

        const recent = contracts.slice(0, 5);
        const container = document.getElementById('recentContracts');
        if (recent.length === 0) {
          UI.showEmpty(container, 'No contracts yet.');
        } else {
          container.innerHTML = recent.map(c => `
            <div class="list-item">
              <div class="list-item__main">
                <strong>${c.title}</strong>
                <small>${c.clientName}</small>
              </div>
              <div class="list-item__meta">
                ${UI.statusBadge(c.status)}
                <small>${UI.formatDate(c.createdAt)}</small>
              </div>
            </div>
          `).join('');
        }
      }

      if (clients) {
        document.getElementById('statClients').textContent =
          clients.filter(c => c.isActive).length;
      }
    }

    loadDashboard();
  </script>
</body>
</html>
```

---

## Step 6 — styles.css (Design System)

```css
/* ============================================================
   CSS VARIABLES — Design System
   ============================================================ */
:root {
  --color-bg: #0f172a;
  --color-surface: #1e293b;
  --color-surface-2: #263548;
  --color-border: #334155;
  --color-primary: #3b82f6;
  --color-primary-hover: #2563eb;
  --color-success: #22c55e;
  --color-warning: #f59e0b;
  --color-danger: #ef4444;
  --color-text: #f1f5f9;
  --color-text-muted: #94a3b8;

  --radius-sm: 6px;
  --radius-md: 10px;
  --radius-lg: 16px;

  --shadow-sm: 0 1px 3px rgba(0,0,0,0.3);
  --shadow-md: 0 4px 16px rgba(0,0,0,0.4);

  --font-sans: 'Segoe UI', system-ui, sans-serif;
}

/* ============================================================
   RESET + BASE
   ============================================================ */
*, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }

body {
  font-family: var(--font-sans);
  background: var(--color-bg);
  color: var(--color-text);
  min-height: 100vh;
  line-height: 1.6;
}

/* ============================================================
   NAVBAR
   ============================================================ */
.navbar {
  display: flex;
  align-items: center;
  gap: 2rem;
  padding: 0 2rem;
  height: 60px;
  background: var(--color-surface);
  border-bottom: 1px solid var(--color-border);
  position: sticky;
  top: 0;
  z-index: 100;
}
.navbar__brand { font-weight: 700; font-size: 1.1rem; }
.navbar__links { display: flex; gap: 1rem; list-style: none; }
.navbar__links a {
  color: var(--color-text-muted);
  text-decoration: none;
  padding: 0.25rem 0.5rem;
  border-radius: var(--radius-sm);
  transition: color 0.2s;
}
.navbar__links a:hover,
.navbar__links a.active { color: var(--color-text); }
.navbar__user { margin-right: auto; display: flex; align-items: center; gap: 1rem; }

/* ============================================================
   LAYOUT
   ============================================================ */
.main-content { max-width: 1200px; margin: 0 auto; padding: 2rem; }
.page-header { margin-bottom: 2rem; }
.page-header h1 { font-size: 1.75rem; font-weight: 700; }
.page-header p { color: var(--color-text-muted); margin-top: 0.25rem; }

/* ============================================================
   CARDS
   ============================================================ */
.card {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  padding: 1.5rem;
}
.card__header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.25rem;
}
.card__header h2 { font-size: 1rem; font-weight: 600; }

/* ============================================================
   STATS GRID
   ============================================================ */
.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(160px, 1fr));
  gap: 1rem;
  margin-bottom: 2rem;
}
.stat-card {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  padding: 1.5rem;
  text-align: center;
}
.stat-card__value { font-size: 2.5rem; font-weight: 700; color: var(--color-primary); }
.stat-card__label { color: var(--color-text-muted); font-size: 0.875rem; margin-top: 0.25rem; }

/* ============================================================
   LIST ITEMS
   ============================================================ */
.list-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0.875rem 0;
  border-bottom: 1px solid var(--color-border);
}
.list-item:last-child { border-bottom: none; }
.list-item__main { display: flex; flex-direction: column; gap: 0.2rem; }
.list-item__main small { color: var(--color-text-muted); font-size: 0.8rem; }
.list-item__meta { display: flex; align-items: center; gap: 0.75rem; }
.list-item__meta small { color: var(--color-text-muted); font-size: 0.8rem; }

/* ============================================================
   BADGES
   ============================================================ */
.badge {
  font-size: 0.75rem;
  font-weight: 600;
  padding: 0.2rem 0.6rem;
  border-radius: 999px;
  text-transform: uppercase;
  letter-spacing: 0.03em;
}
.badge--draft    { background: #1e3a5f; color: #93c5fd; }
.badge--review   { background: #3d2a00; color: #fcd34d; }
.badge--signed   { background: #14401f; color: #86efac; }
.badge--archived { background: #2d2d2d; color: #9ca3af; }

/* ============================================================
   BUTTONS
   ============================================================ */
.btn {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.6rem 1.25rem;
  border-radius: var(--radius-sm);
  font-size: 0.875rem;
  font-weight: 500;
  cursor: pointer;
  border: none;
  transition: background 0.2s, opacity 0.2s;
  text-decoration: none;
}
.btn--primary { background: var(--color-primary); color: #fff; }
.btn--primary:hover { background: var(--color-primary-hover); }
.btn--ghost {
  background: transparent;
  color: var(--color-text-muted);
  border: 1px solid var(--color-border);
}
.btn--ghost:hover { color: var(--color-text); border-color: var(--color-text-muted); }
.btn--danger { background: var(--color-danger); color: #fff; }
.btn--full { width: 100%; justify-content: center; }
.btn--sm { padding: 0.4rem 0.875rem; font-size: 0.8rem; }
.btn:disabled { opacity: 0.5; cursor: not-allowed; }

/* ============================================================
   FORMS
   ============================================================ */
.form-group { display: flex; flex-direction: column; gap: 0.4rem; margin-bottom: 1.25rem; }
.form-group label { font-size: 0.875rem; font-weight: 500; color: var(--color-text-muted); }
.form-group input,
.form-group select,
.form-group textarea {
  background: var(--color-bg);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-sm);
  padding: 0.625rem 0.875rem;
  color: var(--color-text);
  font-size: 0.9rem;
  transition: border-color 0.2s;
  width: 100%;
}
.form-group input:focus,
.form-group select:focus,
.form-group textarea:focus {
  outline: none;
  border-color: var(--color-primary);
}
.form-error {
  color: var(--color-danger);
  font-size: 0.875rem;
  margin-bottom: 1rem;
}

/* ============================================================
   LOGIN PAGE
   ============================================================ */
.login-page {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 100vh;
}
.login-container { width: 100%; max-width: 420px; padding: 1.5rem; }
.login-card {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  padding: 2.5rem;
  box-shadow: var(--shadow-md);
}
.login-logo { text-align: center; margin-bottom: 2rem; }
.logo-icon { font-size: 2.5rem; }
.login-logo h1 { font-size: 1.5rem; font-weight: 700; margin-top: 0.5rem; }
.login-logo p { color: var(--color-text-muted); font-size: 0.875rem; margin-top: 0.25rem; }

/* ============================================================
   TOAST NOTIFICATIONS
   ============================================================ */
.toast {
  position: fixed;
  bottom: 1.5rem;
  left: 50%;
  transform: translateX(-50%) translateY(1rem);
  background: var(--color-surface-2);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  padding: 0.875rem 1.5rem;
  font-size: 0.9rem;
  opacity: 0;
  transition: opacity 0.3s, transform 0.3s;
  z-index: 9999;
  white-space: nowrap;
}
.toast--visible { opacity: 1; transform: translateX(-50%) translateY(0); }
.toast--warning { border-color: var(--color-warning); }
.toast--error { border-color: var(--color-danger); }
.toast--success { border-color: var(--color-success); }

/* ============================================================
   LOADING + EMPTY STATES
   ============================================================ */
.loading { display: flex; flex-direction: column; align-items: center; gap: 1rem; padding: 2rem; }
.loading__spinner {
  width: 32px; height: 32px;
  border: 3px solid var(--color-border);
  border-top-color: var(--color-primary);
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}
@keyframes spin { to { transform: rotate(360deg); } }
.empty-state, .error-state {
  text-align: center;
  padding: 3rem 1rem;
  color: var(--color-text-muted);
}

/* ============================================================
   DASHBOARD GRID
   ============================================================ */
.dashboard-grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: 1.5rem;
}

/* ============================================================
   RESPONSIVE
   ============================================================ */
@media (max-width: 768px) {
  .navbar__links { display: none; }
  .stats-grid { grid-template-columns: repeat(2, 1fr); }
  .main-content { padding: 1rem; }
}
```

---

## Step 7 — contracts.html (Template for List Pages)

The same pattern applies to `clients.html` and `templates.html`.
Cursor/Kiro should generate these using the same structure:

```
1. requireAuth() check at top of script
2. Load data with Api.contracts.getAll()
3. Render list with UI helpers
4. Modal for Create/Edit forms
5. Delete with confirmation
6. Status badge where applicable
```

Tell Cursor/Kiro:
> "Create contracts.html, clients.html, and templates.html following the exact same
> pattern as dashboard.html — same navbar, same CSS classes, same API module.
> Each page loads its data on DOMContentLoaded, renders a list, and has
> Create/Edit/Delete actions via a modal."

---

## Step 8 — CORS Configuration in API

The frontend on a different port needs CORS. Add to `Program.cs`:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://127.0.0.1:5500")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Required for HttpOnly cookie
    });
});

// In middleware section — before UseAuthentication:
app.UseCors("FrontendPolicy");
```

---

## Testing the PWA

1. Open `frontend/` with Live Server (VS Code extension) or:
   ```bash
   cd frontend
   npx serve .
   ```
2. Open `http://localhost:3000`
3. Login with seed data credentials
4. Open DevTools → Application tab → verify Service Worker is registered
5. In Network tab → set "Offline" → verify app still loads from cache

---

## PWA Checklist

- [x] manifest.json with icons
- [x] Service Worker registered
- [x] Cache First for static assets
- [x] Network First for API calls
- [x] Token stored in sessionStorage (cleared on tab close)
- [x] Refresh Token in HttpOnly cookie (auto-sent by browser)
- [x] 401 auto-refresh flow
- [x] Offline toast notification
- [x] Responsive layout
- [ ] Add icons (192x192 and 512x512 PNG) to `frontend/icons/`
