import { AuthUtils } from '@/utils/auth.utils';
import { ClientsApi } from '@/api/clients.api';
import { Toast } from '@/components/toast';
import { Modal } from '@/components/modal';
import type { ClientDto, ClientType } from '@/types/client.types';
import '@/components/navbar';
import '@/components/mobile-nav';

AuthUtils.requireAuth();

let allClients: ClientDto[] = [];
let filteredClients: ClientDto[] = [];

const clientsBody = document.getElementById('clientsBody') as HTMLTableSectionElement;
const searchInput = document.getElementById('searchInput') as HTMLInputElement;
const typeFilter = document.getElementById('typeFilter') as HTMLSelectElement;
const createBtn = document.getElementById('createBtn') as HTMLButtonElement;

// No translation - use enum values directly
function getTypeBadge(type: ClientType): string {
  return type === 'INDIVIDUAL' 
    ? '<span class="badge badge-draft">INDIVIDUAL</span>'
    : '<span class="badge badge-pending">COMPANY</span>';
}

function renderClients() {
  if (filteredClients.length === 0) {
    clientsBody.innerHTML = `
      <tr>
        <td colspan="5" style="text-align: center; padding: 2rem; color: var(--color-gray-500);">
          No clients to display
        </td>
      </tr>
    `;
    return;
  }

  clientsBody.innerHTML = filteredClients.map(client => `
    <tr>
      <td>
        <a href="/src/pages/clients/client-detail.html?id=${client.id}" style="color: var(--color-primary); font-weight: var(--font-weight-medium);">
          ${client.name}
        </a>
      </td>
      <td>${getTypeBadge(client.type)}</td>
      <td>${client.email}</td>
      <td>${client.phone}</td>
      <td>
        <div style="display: flex; gap: 0.5rem;">
          <button class="btn btn-sm btn-secondary" onclick="window.location.href='/src/pages/clients/client-detail.html?id=${client.id}'">
            View
          </button>
          <button class="btn btn-sm btn-danger delete-btn" data-id="${client.id}">
            Delete
          </button>
        </div>
      </td>
    </tr>
  `).join('');

  document.querySelectorAll('.delete-btn').forEach(btn => {
    btn.addEventListener('click', async (e) => {
      const id = parseInt((e.target as HTMLElement).closest('button')?.dataset.id || '0');
      await handleDelete(id);
    });
  });
}

function filterClients() {
  const searchTerm = searchInput.value.toLowerCase();
  const typeValue = typeFilter.value;

  filteredClients = allClients.filter(client => {
    const matchesSearch = client.name.toLowerCase().includes(searchTerm) ||
                         client.email.toLowerCase().includes(searchTerm);
    const matchesType = !typeValue || client.type === typeValue;
    
    return matchesSearch && matchesType;
  });

  renderClients();
}

async function loadClients() {
  try {
    allClients = await ClientsApi.getAll();
    filteredClients = [...allClients];
    renderClients();
  } catch (error: any) {
    console.error('Error loading clients:', error);
    Toast.error('Error loading clients');
    clientsBody.innerHTML = `
      <tr>
        <td colspan="5" style="text-align: center; padding: 2rem; color: var(--color-danger);">
          Error loading clients. Please try again.
        </td>
      </tr>
    `;
  }
}

async function handleDelete(id: number) {
  const confirmed = await Modal.confirm(
    'Delete Client',
    'Are you sure you want to delete this client? This action cannot be undone.'
  );

  if (!confirmed) return;

  try {
    await ClientsApi.delete(id);
    Toast.success('Client deleted successfully');
    await loadClients();
  } catch (error: any) {
    console.error('Error deleting client:', error);
    Toast.error('Error deleting client');
  }
}

createBtn.addEventListener('click', () => {
  window.location.href = '/src/pages/clients/client-detail.html?new=true';
});

searchInput.addEventListener('input', filterClients);
typeFilter.addEventListener('change', filterClients);

loadClients();
