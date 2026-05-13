import { AuthUtils } from '@/utils/auth.utils';
import { AuthApi } from '@/api/auth.api';
import '@/components/navbar';
import '@/components/mobile-nav';

// Require authentication
AuthUtils.requireAuth();

// Get user info
const user = AuthUtils.getUser();
if (user) {
  const userNameEl = document.getElementById('userName');
  if (userNameEl) {
    userNameEl.textContent = user.fullName;
  }
}

// Load stats
async function loadStats() {
  const statsGrid = document.getElementById('statsGrid');
  if (!statsGrid) return;

  // Mock data for now - will be replaced with real API calls
  const stats = [
    { title: 'Active Contracts', value: '24', color: 'var(--color-primary)' },
    { title: 'Clients', value: '18', color: 'var(--color-success)' },
    { title: 'Templates', value: '12', color: 'var(--color-warning)' },
    { title: 'Pending Signature', value: '5', color: 'var(--color-danger)' }
  ];

  statsGrid.innerHTML = stats.map(stat => `
    <div class="stat-card" style="background: linear-gradient(135deg, ${stat.color} 0%, ${stat.color}dd 100%);">
      <div style="font-size: var(--font-size-3xl); font-weight: var(--font-weight-bold); margin-bottom: 0.5rem;">
        ${stat.value}
      </div>
      <div style="font-size: var(--font-size-lg);">
        ${stat.title}
      </div>
    </div>
  `).join('');
}

// Load recent contracts
async function loadRecentContracts() {
  const recentContractsEl = document.getElementById('recentContracts');
  if (!recentContractsEl) return;

  // Mock data for now
  recentContractsEl.innerHTML = `
    <div style="padding: 1rem; text-align: center; color: var(--color-gray-500);">
      No recent contracts to display
    </div>
  `;
}

// Initialize
loadStats();
loadRecentContracts();
