import { AuthUtils } from '@/utils/auth.utils';
import { DateUtils } from '@/utils/date.utils';
import { ContractsApi } from '@/api/contracts.api';
import { Toast } from '@/components/toast';
import { Modal } from '@/components/modal';
import type { ContractDto, ContractStatus } from '@/types/contract.types';
import '@/components/navbar';
import '@/components/mobile-nav';

// Require authentication
AuthUtils.requireAuth();

// State
let allContracts: ContractDto[] = [];
let filteredContracts: ContractDto[] = [];

// DOM Elements
const contractsBody = document.getElementById('contractsBody') as HTMLTableSectionElement;
const searchInput = document.getElementById('searchInput') as HTMLInputElement;
const statusFilter = document.getElementById('statusFilter') as HTMLSelectElement;
const createBtn = document.getElementById('createBtn') as HTMLButtonElement;

// Status badge helper - no translation, use enum values directly
function getStatusBadge(status: ContractStatus): string {
  const statusMap = {
    DRAFT: { class: 'badge-draft' },
    REVIEW: { class: 'badge-pending' },
    SIGNED: { class: 'badge-active' },
    ARCHIVED: { class: 'badge-completed' }
  };
  
  const config = statusMap[status] || { class: 'badge-draft' };
  return `<span class="badge ${config.class}">${status}</span>`;
}

// Render contracts table
function renderContracts() {
  if (filteredContracts.length === 0) {
    contractsBody.innerHTML = `
      <tr>
        <td colspan="5" style="text-align: center; padding: 2rem; color: var(--color-gray-500);">
          No contracts to display
        </td>
      </tr>
    `;
    return;
  }

  contractsBody.innerHTML = filteredContracts.map(contract => `
    <tr>
      <td>
        <a href="/src/pages/contracts/contract-detail.html?id=${contract.id}" style="color: var(--color-primary); font-weight: var(--font-weight-medium);">
          ${contract.title}
        </a>
      </td>
      <td>${contract.clientName || '-'}</td>
      <td>${getStatusBadge(contract.status)}</td>
      <td>${DateUtils.formatDateShort(contract.createdAt)}</td>
      <td>
        <div style="display: flex; gap: 0.5rem;">
          <button class="btn btn-sm btn-secondary" onclick="window.location.href='/src/pages/contracts/contract-detail.html?id=${contract.id}'">
            View
          </button>
          <button class="btn btn-sm btn-danger delete-btn" data-id="${contract.id}">
            Delete
          </button>
        </div>
      </td>
    </tr>
  `).join('');

  // Add delete event listeners
  document.querySelectorAll('.delete-btn').forEach(btn => {
    btn.addEventListener('click', async (e) => {
      const id = parseInt((e.target as HTMLElement).closest('button')?.dataset.id || '0');
      await handleDelete(id);
    });
  });
}

// Filter contracts
function filterContracts() {
  const searchTerm = searchInput.value.toLowerCase();
  const statusValue = statusFilter.value;

  filteredContracts = allContracts.filter(contract => {
    const matchesSearch = contract.title.toLowerCase().includes(searchTerm) ||
                         (contract.clientName?.toLowerCase().includes(searchTerm) || false);
    const matchesStatus = !statusValue || contract.status === statusValue;
    
    return matchesSearch && matchesStatus;
  });

  renderContracts();
}

// Load contracts
async function loadContracts() {
  try {
    allContracts = await ContractsApi.getAll();
    filteredContracts = [...allContracts];
    renderContracts();
  } catch (error: any) {
    console.error('Error loading contracts:', error);
    Toast.error('Error loading contracts');
    contractsBody.innerHTML = `
      <tr>
        <td colspan="5" style="text-align: center; padding: 2rem; color: var(--color-danger);">
          Error loading contracts. Please try again.
        </td>
      </tr>
    `;
  }
}

// Handle delete
async function handleDelete(id: number) {
  const confirmed = await Modal.confirm(
    'Delete Contract',
    'Are you sure you want to delete this contract? This action cannot be undone.'
  );

  if (!confirmed) return;

  try {
    await ContractsApi.delete(id);
    Toast.success('Contract deleted successfully');
    await loadContracts();
  } catch (error: any) {
    console.error('Error deleting contract:', error);
    Toast.error('Error deleting contract');
  }
}

// Handle create
createBtn.addEventListener('click', () => {
  window.location.href = '/src/pages/contracts/contract-detail.html?new=true';
});

// Search and filter
searchInput.addEventListener('input', filterContracts);
statusFilter.addEventListener('change', filterContracts);

// Initialize
loadContracts();
