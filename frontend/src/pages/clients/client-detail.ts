import { AuthUtils } from '@/utils/auth.utils';
import { ValidationUtils } from '@/utils/validation.utils';
import { ClientsApi } from '@/api/clients.api';
import { Toast } from '@/components/toast';
import { Loader } from '@/components/loader';
import type { CreateClientDto, UpdateClientDto, ClientType } from '@/types/client.types';
import '@/components/navbar';
import '@/components/mobile-nav';

AuthUtils.requireAuth();

const urlParams = new URLSearchParams(window.location.search);
const clientId = urlParams.get('id') ? parseInt(urlParams.get('id')!) : null;
const isNew = urlParams.get('new') === 'true';

const pageTitle = document.getElementById('pageTitle') as HTMLHeadingElement;
const clientForm = document.getElementById('clientForm') as HTMLFormElement;
const nameInput = document.getElementById('name') as HTMLInputElement;
const typeSelect = document.getElementById('type') as HTMLSelectElement;
const emailInput = document.getElementById('email') as HTMLInputElement;
const phoneInput = document.getElementById('phone') as HTMLInputElement;
const addressInput = document.getElementById('address') as HTMLInputElement;
const companyRegistrationNumberInput = document.getElementById('companyRegistrationNumber') as HTMLInputElement;
const contactPersonNameInput = document.getElementById('contactPersonName') as HTMLInputElement;
const companyFields = document.getElementById('companyFields') as HTMLDivElement;
const saveBtn = document.getElementById('saveBtn') as HTMLButtonElement;
const saveBtnText = document.getElementById('saveBtnText') as HTMLSpanElement;
const saveBtnLoader = document.getElementById('saveBtnLoader') as HTMLSpanElement;
const cancelBtn = document.getElementById('cancelBtn') as HTMLButtonElement;

const nameError = document.getElementById('nameError') as HTMLDivElement;
const emailError = document.getElementById('emailError') as HTMLDivElement;
const phoneError = document.getElementById('phoneError') as HTMLDivElement;
const addressError = document.getElementById('addressError') as HTMLDivElement;

// Toggle company fields
typeSelect.addEventListener('change', () => {
  if (typeSelect.value === 'COMPANY') {
    companyFields.classList.remove('hidden');
  } else {
    companyFields.classList.add('hidden');
  }
});

async function loadClient() {
  if (!clientId) return;

  Loader.show();
  try {
    const client = await ClientsApi.getById(clientId);
    
    pageTitle.textContent = client.name;
    nameInput.value = client.name;
    typeSelect.value = client.type;
    emailInput.value = client.email;
    phoneInput.value = client.phone;
    addressInput.value = client.address;
    companyRegistrationNumberInput.value = client.companyRegistrationNumber || '';
    contactPersonNameInput.value = client.contactPersonName || '';

    if (client.type === 'COMPANY') {
      companyFields.classList.remove('hidden');
    }
  } catch (error: any) {
    console.error('Error loading client:', error);
    Toast.error('שגיאה בטעינת פרטי הלקוח');
    setTimeout(() => {
      window.location.href = '/src/pages/clients/clients.html';
    }, 2000);
  } finally {
    Loader.hide();
  }
}

function validateForm(): boolean {
  let isValid = true;

  nameError.classList.add('hidden');
  emailError.classList.add('hidden');
  phoneError.classList.add('hidden');
  addressError.classList.add('hidden');

  if (!ValidationUtils.isRequired(nameInput.value)) {
    nameError.textContent = ValidationUtils.getErrorMessage('שם', 'required');
    nameError.classList.remove('hidden');
    isValid = false;
  }

  if (!ValidationUtils.isRequired(emailInput.value)) {
    emailError.textContent = ValidationUtils.getErrorMessage('אימייל', 'required');
    emailError.classList.remove('hidden');
    isValid = false;
  } else if (!ValidationUtils.isValidEmail(emailInput.value)) {
    emailError.textContent = ValidationUtils.getErrorMessage('אימייל', 'email');
    emailError.classList.remove('hidden');
    isValid = false;
  }

  if (!ValidationUtils.isRequired(phoneInput.value)) {
    phoneError.textContent = ValidationUtils.getErrorMessage('טלפון', 'required');
    phoneError.classList.remove('hidden');
    isValid = false;
  }

  if (!ValidationUtils.isRequired(addressInput.value)) {
    addressError.textContent = ValidationUtils.getErrorMessage('כתובת', 'required');
    addressError.classList.remove('hidden');
    isValid = false;
  }

  return isValid;
}

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

clientForm.addEventListener('submit', async (e) => {
  e.preventDefault();

  if (!validateForm()) return;

  setLoading(true);

  try {
    if (isNew || !clientId) {
      const createData: CreateClientDto = {
        name: nameInput.value.trim(),
        type: typeSelect.value as ClientType,
        email: emailInput.value.trim(),
        phone: phoneInput.value.trim(),
        address: addressInput.value.trim(),
        companyRegistrationNumber: companyRegistrationNumberInput.value.trim() || undefined,
        contactPersonName: contactPersonNameInput.value.trim() || undefined
      };

      await ClientsApi.create(createData);
      Toast.success('הלקוח נוצר בהצלחה');
    } else {
      const updateData: UpdateClientDto = {
        name: nameInput.value.trim(),
        email: emailInput.value.trim(),
        phone: phoneInput.value.trim(),
        address: addressInput.value.trim(),
        companyRegistrationNumber: companyRegistrationNumberInput.value.trim() || undefined,
        contactPersonName: contactPersonNameInput.value.trim() || undefined
      };

      await ClientsApi.update(clientId, updateData);
      Toast.success('הלקוח עודכן בהצלחה');
    }

    setTimeout(() => {
      window.location.href = '/src/pages/clients/clients.html';
    }, 1000);
  } catch (error: any) {
    console.error('Error saving client:', error);
    Toast.error(error.message || 'שגיאה בשמירת הלקוח');
  } finally {
    setLoading(false);
  }
});

cancelBtn.addEventListener('click', () => {
  window.location.href = '/src/pages/clients/clients.html';
});

nameInput.addEventListener('input', () => nameError.classList.add('hidden'));
emailInput.addEventListener('input', () => emailError.classList.add('hidden'));
phoneInput.addEventListener('input', () => phoneError.classList.add('hidden'));
addressInput.addEventListener('input', () => addressError.classList.add('hidden'));

if (clientId && !isNew) {
  loadClient();
}
