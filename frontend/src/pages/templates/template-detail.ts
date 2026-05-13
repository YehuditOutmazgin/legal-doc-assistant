import { AuthUtils } from '@/utils/auth.utils';
import { ValidationUtils } from '@/utils/validation.utils';
import { TemplatesApi } from '@/api/templates.api';
import { Toast } from '@/components/toast';
import { Loader } from '@/components/loader';
import type { CreateTemplateDto, UpdateTemplateDto } from '@/types/template.types';
import '@/components/navbar';
import '@/components/mobile-nav';

AuthUtils.requireAuth();

const urlParams = new URLSearchParams(window.location.search);
const templateId = urlParams.get('id') ? parseInt(urlParams.get('id')!) : null;
const isNew = urlParams.get('new') === 'true';

const pageTitle = document.getElementById('pageTitle') as HTMLHeadingElement;
const templateForm = document.getElementById('templateForm') as HTMLFormElement;
const nameInput = document.getElementById('name') as HTMLInputElement;
const categoryInput = document.getElementById('category') as HTMLInputElement;
const descriptionTextarea = document.getElementById('description') as HTMLTextAreaElement;
const contentTextarea = document.getElementById('content') as HTMLTextAreaElement;
const saveBtn = document.getElementById('saveBtn') as HTMLButtonElement;
const saveBtnText = document.getElementById('saveBtnText') as HTMLSpanElement;
const saveBtnLoader = document.getElementById('saveBtnLoader') as HTMLSpanElement;
const cancelBtn = document.getElementById('cancelBtn') as HTMLButtonElement;

const nameError = document.getElementById('nameError') as HTMLDivElement;
const categoryError = document.getElementById('categoryError') as HTMLDivElement;
const descriptionError = document.getElementById('descriptionError') as HTMLDivElement;
const contentError = document.getElementById('contentError') as HTMLDivElement;

async function loadTemplate() {
  if (!templateId) return;

  Loader.show();
  try {
    const template = await TemplatesApi.getById(templateId);
    
    pageTitle.textContent = template.name;
    nameInput.value = template.name;
    categoryInput.value = template.category;
    descriptionTextarea.value = template.description;
    contentTextarea.value = template.content;
  } catch (error: any) {
    console.error('Error loading template:', error);
    Toast.error('שגיאה בטעינת פרטי התבנית');
    setTimeout(() => {
      window.location.href = '/src/pages/templates/templates.html';
    }, 2000);
  } finally {
    Loader.hide();
  }
}

function validateForm(): boolean {
  let isValid = true;

  nameError.classList.add('hidden');
  categoryError.classList.add('hidden');
  descriptionError.classList.add('hidden');
  contentError.classList.add('hidden');

  if (!ValidationUtils.isRequired(nameInput.value)) {
    nameError.textContent = ValidationUtils.getErrorMessage('שם', 'required');
    nameError.classList.remove('hidden');
    isValid = false;
  }

  if (!ValidationUtils.isRequired(categoryInput.value)) {
    categoryError.textContent = ValidationUtils.getErrorMessage('קטגוריה', 'required');
    categoryError.classList.remove('hidden');
    isValid = false;
  }

  if (!ValidationUtils.isRequired(descriptionTextarea.value)) {
    descriptionError.textContent = ValidationUtils.getErrorMessage('תיאור', 'required');
    descriptionError.classList.remove('hidden');
    isValid = false;
  }

  if (!ValidationUtils.isRequired(contentTextarea.value)) {
    contentError.textContent = ValidationUtils.getErrorMessage('תוכן', 'required');
    contentError.classList.remove('hidden');
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

templateForm.addEventListener('submit', async (e) => {
  e.preventDefault();

  if (!validateForm()) return;

  setLoading(true);

  try {
    if (isNew || !templateId) {
      const createData: CreateTemplateDto = {
        name: nameInput.value.trim(),
        category: categoryInput.value.trim(),
        description: descriptionTextarea.value.trim(),
        content: contentTextarea.value.trim()
      };

      await TemplatesApi.create(createData);
      Toast.success('התבנית נוצרה בהצלחה');
    } else {
      const updateData: UpdateTemplateDto = {
        name: nameInput.value.trim(),
        category: categoryInput.value.trim(),
        description: descriptionTextarea.value.trim(),
        content: contentTextarea.value.trim()
      };

      await TemplatesApi.update(templateId, updateData);
      Toast.success('התבנית עודכנה בהצלחה');
    }

    setTimeout(() => {
      window.location.href = '/src/pages/templates/templates.html';
    }, 1000);
  } catch (error: any) {
    console.error('Error saving template:', error);
    Toast.error(error.message || 'שגיאה בשמירת התבנית');
  } finally {
    setLoading(false);
  }
});

cancelBtn.addEventListener('click', () => {
  window.location.href = '/src/pages/templates/templates.html';
});

nameInput.addEventListener('input', () => nameError.classList.add('hidden'));
categoryInput.addEventListener('input', () => categoryError.classList.add('hidden'));
descriptionTextarea.addEventListener('input', () => descriptionError.classList.add('hidden'));
contentTextarea.addEventListener('input', () => contentError.classList.add('hidden'));

if (templateId && !isNew) {
  loadTemplate();
}
