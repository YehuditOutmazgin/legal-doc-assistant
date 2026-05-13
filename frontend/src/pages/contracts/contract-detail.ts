import { AuthUtils } from '@/utils/auth.utils';
import { ValidationUtils } from '@/utils/validation.utils';
import { ContractsApi } from '@/api/contracts.api';
import { ClientsApi } from '@/api/clients.api';
import { TemplatesApi } from '@/api/templates.api';
import { Toast } from '@/components/toast';
import { Loader } from '@/components/loader';
import type { ContractDto, CreateContractDto, UpdateContractDto, ContractStatus } from '@/types/contract.types';
import '@/components/navbar';
import '@/components/mobile-nav';

// Require authentication
AuthUtils.requireAuth();

// Get contract ID from URL
const urlParams = new URLSearchParams(window.location.search);
const contractId = urlParams.get('id') ? parseInt(urlParams.get('id')!) : null;
const isNew = urlParams.get('new') === 'true';

// DOM Elements
const pageTitle = document.getElementById('pageTitle') as HTMLHeadingElement;
const contractForm = document.getElementById('contractForm') as HTMLFormElement;
const titleInput = document.getElementById('title') as HTMLInputElement;
const clientIdSelect = document.getElementById('clientId') as HTMLSelectElement;
const statusSelect = document.getElementById('status') as HTMLSelectElement;
const templateIdSelect = document.getElementById('templateId') as HTMLSelectElement;
const contentTextarea = document.getElementById('content') as HTMLTextAreaElement;
const notesTextarea = document.getElementById('notes') as HTMLTextAreaElement;
const saveBtn = document.getElementById('saveBtn') as HTMLButtonElement;
const saveBtnText = document.getElementById('saveBtnText') as HTMLSpanElement;
const saveBtnLoader = document.getElementById('saveBtnLoader') as HTMLSpanElement;
const cancelBtn = document.getElementById('cancelBtn') as HTMLButtonElement;

// Error elements
const titleError = document.getElementById('titleError') as HTMLDivElement;
const clientIdError = document.getElementById('clientIdError') as HTMLDivElement;
const contentError = document.getElementById('contentError') as HTMLDivElement;

// Load clients
async function loadClients() {
  try {
    const clients = await ClientsApi.getAll();
    clientIdSelect.innerHTML = '<option value="">Select client...</option>' +
      clients
        .filter(c => c.isActive)
        .map(client => `<option value="${client.id}">${client.name}</option>`)
        .join('');
  } catch (error) {
    console.error('Error loading clients:', error);
    Toast.error('Error loading clients list');
  }
}

// Load templates
async function loadTemplates() {
  try {
    const templates = await TemplatesApi.getAll();
    templateIdSelect.innerHTML = '<option value="">No template</option>' +
      templates
        .filter(t => t.isActive)
        .map(template => `<option value="${template.id}">${template.name}</option>`)
        .join('');
  } catch (error) {
    console.error('Error loading templates:', error);
    Toast.error('Error loading templates list');
  }
}

// Load contract data
async function loadContract() {
  if (!contractId) return;

  Loader.show();
  try {
    const contract = await ContractsApi.getById(contractId);
    
    pageTitle.textContent = contract.title;
    titleInput.value = contract.title;
    clientIdSelect.value = contract.clientId.toString();
    statusSelect.value = contract.status;
    templateIdSelect.value = contract.templateId?.toString() || '';
    contentTextarea.value = contract.content;
    notesTextarea.value = contract.notes || '';
  } catch (error: any) {
    console.error('Error loading contract:', error);
    Toast.error('Error loading contract details');
    setTimeout(() => {
      window.location.href = '/src/pages/contracts/contracts.html';
    }, 2000);
  } finally {
    Loader.hide();
  }
}

// Validation
function validateForm(): boolean {
  let isValid = true;

  // Reset errors
  titleError.classList.add('hidden');
  clientIdError.classList.add('hidden');
  contentError.classList.add('hidden');

  // Validate title
  if (!ValidationUtils.isRequired(titleInput.value)) {
    titleError.textContent = ValidationUtils.getErrorMessage('Title', 'required');
    titleError.classList.remove('hidden');
    isValid = false;
  }

  // Validate client
  if (!clientIdSelect.value) {
    clientIdError.textContent = 'Please select a client';
    clientIdError.classList.remove('hidden');
    isValid = false;
  }

  // Validate content
  if (!ValidationUtils.isRequired(contentTextarea.value)) {
    contentError.textContent = ValidationUtils.getErrorMessage('Content', 'required');
    contentError.classList.remove('hidden');
    isValid = false;
  }

  return isValid;
}

// Set loading state
function setLoading(loading: boolean) {
  saveBtn.disabled = loading;
  if (loading) {
    saveBtnText.classList.add('hidden');
    saveBtnLoader.classList.remove('hidden');
  } else {
    saveBtnText.classList.remove('hidden');
    saveBtnLoader.classList.add('hidden');
  }
}

// Handle form submission
contractForm.addEventListener('submit', async (e) => {
  e.preventDefault();

  if (!validateForm()) {
    return;
  }

  setLoading(true);

  try {
    if (isNew || !contractId) {
      // Create new contract
      const createData: CreateContractDto = {
        title: titleInput.value.trim(),
        clientId: parseInt(clientIdSelect.value),
        status: statusSelect.value as ContractStatus,
        templateId: templateIdSelect.value ? parseInt(templateIdSelect.value) : undefined,
        content: contentTextarea.value.trim(),
        notes: notesTextarea.value.trim() || undefined
      };

      await ContractsApi.create(createData);
      Toast.success('Contract created successfully');
    } else {
      // Update existing contract
      const updateData: UpdateContractDto = {
        title: titleInput.value.trim(),
        status: statusSelect.value as ContractStatus,
        content: contentTextarea.value.trim(),
        notes: notesTextarea.value.trim() || undefined
      };

      await ContractsApi.update(contractId, updateData);
      Toast.success('Contract updated successfully');
    }

    setTimeout(() => {
      window.location.href = '/src/pages/contracts/contracts.html';
    }, 1000);
  } catch (error: any) {
    console.error('Error saving contract:', error);
    Toast.error(error.message || 'Error saving contract');
  } finally {
    setLoading(false);
  }
});

// Cancel button
cancelBtn.addEventListener('click', () => {
  window.location.href = '/src/pages/contracts/contracts.html';
});

// Clear errors on input
titleInput.addEventListener('input', () => titleError.classList.add('hidden'));
clientIdSelect.addEventListener('change', () => clientIdError.classList.add('hidden'));
contentTextarea.addEventListener('input', () => contentError.classList.add('hidden'));

// Initialize
async function init() {
  await Promise.all([
    loadClients(),
    loadTemplates()
  ]);

  if (contractId && !isNew) {
    await loadContract();
  }
}

init();
