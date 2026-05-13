import { AuthApi } from '@/api/auth.api';
import { AuthUtils } from '@/utils/auth.utils';

const navbarHTML = `
  <nav class="navbar hide-mobile" style="
    position: fixed;
    left: 0;
    top: 0;
    height: 100vh;
    width: var(--navbar-width);
    background-color: var(--color-white);
    box-shadow: var(--shadow-md);
    padding: var(--spacing-lg);
    display: flex;
    flex-direction: column;
    z-index: var(--z-dropdown);
  ">
    <div style="margin-bottom: 2rem;">
      <h2 style="color: var(--color-primary); font-size: var(--font-size-2xl);">LegalDoc</h2>
    </div>

    <div style="flex: 1; display: flex; flex-direction: column; gap: var(--spacing-sm);">
      <a href="/src/pages/dashboard/dashboard.html" class="nav-link" data-page="dashboard">
        <svg width="20" height="20" fill="currentColor" viewBox="0 0 20 20">
          <path d="M10.707 2.293a1 1 0 00-1.414 0l-7 7a1 1 0 001.414 1.414L4 10.414V17a1 1 0 001 1h2a1 1 0 001-1v-2a1 1 0 011-1h2a1 1 0 011 1v2a1 1 0 001 1h2a1 1 0 001-1v-6.586l.293.293a1 1 0 001.414-1.414l-7-7z"/>
        </svg>
        <span>Dashboard</span>
      </a>

      <a href="/src/pages/contracts/contracts.html" class="nav-link" data-page="contracts">
        <svg width="20" height="20" fill="currentColor" viewBox="0 0 20 20">
          <path fill-rule="evenodd" d="M4 4a2 2 0 012-2h4.586A2 2 0 0112 2.586L15.414 6A2 2 0 0116 7.414V16a2 2 0 01-2 2H6a2 2 0 01-2-2V4z" clip-rule="evenodd"/>
        </svg>
        <span>Contracts</span>
      </a>

      <a href="/src/pages/clients/clients.html" class="nav-link" data-page="clients">
        <svg width="20" height="20" fill="currentColor" viewBox="0 0 20 20">
          <path d="M9 6a3 3 0 11-6 0 3 3 0 016 0zM17 6a3 3 0 11-6 0 3 3 0 016 0zM12.93 17c.046-.327.07-.66.07-1a6.97 6.97 0 00-1.5-4.33A5 5 0 0119 16v1h-6.07zM6 11a5 5 0 015 5v1H1v-1a5 5 0 015-5z"/>
        </svg>
        <span>Clients</span>
      </a>

      <a href="/src/pages/templates/templates.html" class="nav-link" data-page="templates">
        <svg width="20" height="20" fill="currentColor" viewBox="0 0 20 20">
          <path fill-rule="evenodd" d="M3 4a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1zm0 4a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1zm0 4a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1zm0 4a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1z" clip-rule="evenodd"/>
        </svg>
        <span>Templates</span>
      </a>
    </div>

    <div style="border-top: 1px solid var(--color-gray-200); padding-top: var(--spacing-lg); margin-top: var(--spacing-lg);">
      <div style="margin-bottom: var(--spacing-md); padding: var(--spacing-sm); background-color: var(--color-gray-50); border-radius: var(--radius-md);">
        <div style="font-weight: var(--font-weight-medium); margin-bottom: 0.25rem;" id="navUserName"></div>
        <div style="font-size: var(--font-size-sm); color: var(--color-gray-600);" id="navUserEmail"></div>
      </div>
      <button id="logoutBtn" class="btn btn-secondary" style="width: 100%;">
        <svg width="20" height="20" fill="currentColor" viewBox="0 0 20 20">
          <path fill-rule="evenodd" d="M3 3a1 1 0 00-1 1v12a1 1 0 102 0V4a1 1 0 00-1-1zm10.293 9.293a1 1 0 001.414 1.414l3-3a1 1 0 000-1.414l-3-3a1 1 0 10-1.414 1.414L14.586 9H7a1 1 0 100 2h7.586l-1.293 1.293z" clip-rule="evenodd"/>
        </svg>
        <span>Logout</span>
      </button>
    </div>
  </nav>
`;

// Inject navbar
const navbarContainer = document.getElementById('navbar');
if (navbarContainer) {
  navbarContainer.innerHTML = navbarHTML;

  // Set user info
  const user = AuthUtils.getUser();
  if (user) {
    const navUserName = document.getElementById('navUserName');
    const navUserEmail = document.getElementById('navUserEmail');
    if (navUserName) navUserName.textContent = user.fullName;
    if (navUserEmail) navUserEmail.textContent = user.email;
  }

  // Highlight active page
  const currentPath = window.location.pathname;
  document.querySelectorAll('.nav-link').forEach(link => {
    const href = link.getAttribute('href');
    if (href && currentPath.includes(href)) {
      link.classList.add('active');
    }
  });

  // Logout handler
  const logoutBtn = document.getElementById('logoutBtn');
  if (logoutBtn) {
    logoutBtn.addEventListener('click', async () => {
      if (confirm('Are you sure you want to logout?')) {
        await AuthApi.logout();
      }
    });
  }
}

// Add navbar styles
const style = document.createElement('style');
style.textContent = `
  .nav-link {
    display: flex;
    align-items: center;
    gap: var(--spacing-md);
    padding: var(--spacing-md);
    border-radius: var(--radius-md);
    color: var(--color-gray-700);
    transition: all var(--transition-base);
    font-weight: var(--font-weight-medium);
  }

  .nav-link:hover {
    background-color: var(--color-gray-100);
    color: var(--color-primary);
  }

  .nav-link.active {
    background-color: var(--color-primary);
    color: var(--color-white);
  }
`;
document.head.appendChild(style);
