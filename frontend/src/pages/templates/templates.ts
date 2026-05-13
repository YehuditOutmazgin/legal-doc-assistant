import { AuthUtils } from '@/utils/auth.utils';
import { DateUtils } from '@/utils/date.utils';
import { TemplatesApi } from '@/api/templates.api';
import { Toast } from '@/components/toast';
import { Modal } from '@/components/modal';
import type { TemplateDto } from '@/types/template.types';
import '@/components/navbar';
import '@/components/mobile-nav';

AuthUtils.requireAuth();

let allTemplates: TemplateDto[] = [];
let filteredTemplates: TemplateDto[] = [];
let categories: string[] = [];

const templatesBody = document.getElementById('templatesBody') as HTMLTableSectionElement;
const searchInput = document.getElementById('searchInput') as HTMLInputElement;
const categoryFilter = document.getElementById('categoryFilter') as HTMLSelectElement;
const createBtn = document.getElementById('createBtn') as HTMLButtonElement;

function renderTemplates() {
  if (filteredTemplates.length === 0) {
    templatesBody.innerHTML = `
      <tr>
        <td colspan="5" style="text-align: center; padding: 2rem; color: var(--color-gray-500);">
          No templates to display
        </td>
      </tr>
    `;
    return;
  }

  templatesBody.innerHTML = filteredTemplates.map(template => `
    <tr>
      <td>
        <a href="/src/pages/templates/template-detail.html?id=${template.id}" style="color: var(--color-primary); font-weight: var(--font-weight-medium);">
          ${template.name}
        </a>
      </td>
      <td><span class="badge badge-pending">${template.category}</span></td>
      <td style="max-width: 300px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap;">
        ${template.description}
      </td>
      <td>${DateUtils.formatDateShort(template.createdAt)}</td>
      <td>
        <div style="display: flex; gap: 0.5rem;">
          <button class="btn btn-sm btn-secondary" onclick="window.location.href='/src/pages/templates/template-detail.html?id=${template.id}'">
            View
          </button>
          <button class="btn btn-sm btn-danger delete-btn" data-id="${template.id}">
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

function filterTemplates() {
  const searchTerm = searchInput.value.toLowerCase();
  const categoryValue = categoryFilter.value;

  filteredTemplates = allTemplates.filter(template => {
    const matchesSearch = template.name.toLowerCase().includes(searchTerm) ||
                         template.description.toLowerCase().includes(searchTerm);
    const matchesCategory = !categoryValue || template.category === categoryValue;
    
    return matchesSearch && matchesCategory;
  });

  renderTemplates();
}

async function loadTemplates() {
  try {
    allTemplates = await TemplatesApi.getAll();
    filteredTemplates = [...allTemplates];
    
    // Extract unique categories
    categories = [...new Set(allTemplates.map(t => t.category))];
    categoryFilter.innerHTML = '<option value="">All</option>' +
      categories.map(cat => `<option value="${cat}">${cat}</option>`).join('');
    
    renderTemplates();
  } catch (error: any) {
    console.error('Error loading templates:', error);
    Toast.error('Error loading templates');
    templatesBody.innerHTML = `
      <tr>
        <td colspan="5" style="text-align: center; padding: 2rem; color: var(--color-danger);">
          Error loading templates. Please try again.
        </td>
      </tr>
    `;
  }
}

async function handleDelete(id: number) {
  const confirmed = await Modal.confirm(
    'Delete Template',
    'Are you sure you want to delete this template? This action cannot be undone.'
  );

  if (!confirmed) return;

  try {
    await TemplatesApi.delete(id);
    Toast.success('Template deleted successfully');
    await loadTemplates();
  } catch (error: any) {
    console.error('Error deleting template:', error);
    Toast.error('Error deleting template');
  }
}

createBtn.addEventListener('click', () => {
  window.location.href = '/src/pages/templates/template-detail.html?new=true';
});

searchInput.addEventListener('input', filterTemplates);
categoryFilter.addEventListener('change', filterTemplates);

loadTemplates();
