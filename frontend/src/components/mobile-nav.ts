const mobileNavHTML = `
  <nav class="mobile-nav hide-desktop" style="
    position: fixed;
    bottom: 0;
    left: 0;
    right: 0;
    height: var(--mobile-nav-height);
    background-color: var(--color-white);
    box-shadow: 0 -2px 10px rgba(0, 0, 0, 0.1);
    display: flex;
    justify-content: space-around;
    align-items: center;
    padding: 0 var(--spacing-md);
    z-index: var(--z-dropdown);
  ">
    <a href="/src/pages/dashboard/dashboard.html" class="mobile-nav-link" data-page="dashboard">
      <svg width="24" height="24" fill="currentColor" viewBox="0 0 20 20">
        <path d="M10.707 2.293a1 1 0 00-1.414 0l-7 7a1 1 0 001.414 1.414L4 10.414V17a1 1 0 001 1h2a1 1 0 001-1v-2a1 1 0 011-1h2a1 1 0 011 1v2a1 1 0 001 1h2a1 1 0 001-1v-6.586l.293.293a1 1 0 001.414-1.414l-7-7z"/>
      </svg>
      <span>Dashboard</span>
    </a>

    <a href="/src/pages/contracts/contracts.html" class="mobile-nav-link" data-page="contracts">
      <svg width="24" height="24" fill="currentColor" viewBox="0 0 20 20">
        <path fill-rule="evenodd" d="M4 4a2 2 0 012-2h4.586A2 2 0 0112 2.586L15.414 6A2 2 0 0116 7.414V16a2 2 0 01-2 2H6a2 2 0 01-2-2V4z" clip-rule="evenodd"/>
      </svg>
      <span>Contracts</span>
    </a>

    <a href="/src/pages/clients/clients.html" class="mobile-nav-link" data-page="clients">
      <svg width="24" height="24" fill="currentColor" viewBox="0 0 20 20">
        <path d="M9 6a3 3 0 11-6 0 3 3 0 016 0zM17 6a3 3 0 11-6 0 3 3 0 016 0zM12.93 17c.046-.327.07-.66.07-1a6.97 6.97 0 00-1.5-4.33A5 5 0 0119 16v1h-6.07zM6 11a5 5 0 015 5v1H1v-1a5 5 0 015-5z"/>
      </svg>
      <span>Clients</span>
    </a>

    <a href="/src/pages/templates/templates.html" class="mobile-nav-link" data-page="templates">
      <svg width="24" height="24" fill="currentColor" viewBox="0 0 20 20">
        <path fill-rule="evenodd" d="M3 4a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1zm0 4a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1zm0 4a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1zm0 4a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1z" clip-rule="evenodd"/>
      </svg>
      <span>Templates</span>
    </a>
  </nav>
`;

// Inject mobile nav
const mobileNavContainer = document.getElementById('mobileNav');
if (mobileNavContainer) {
  mobileNavContainer.innerHTML = mobileNavHTML;

  // Highlight active page
  const currentPath = window.location.pathname;
  document.querySelectorAll('.mobile-nav-link').forEach(link => {
    const href = link.getAttribute('href');
    if (href && currentPath.includes(href)) {
      link.classList.add('active');
    }
  });
}

// Add mobile nav styles
const style = document.createElement('style');
style.textContent = `
  .mobile-nav-link {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 0.25rem;
    padding: var(--spacing-xs);
    color: var(--color-gray-600);
    font-size: var(--font-size-xs);
    transition: color var(--transition-base);
    min-width: 60px;
  }

  .mobile-nav-link:hover {
    color: var(--color-primary);
  }

  .mobile-nav-link.active {
    color: var(--color-primary);
  }

  .mobile-nav-link svg {
    width: 24px;
    height: 24px;
  }
`;
document.head.appendChild(style);
