import { HttpClient } from './http.client';
import type {
  TemplateDto,
  CreateTemplateDto,
  UpdateTemplateDto
} from '@/types/template.types';

export const TemplatesApi = {
  async getAll(): Promise<TemplateDto[]> {
    return HttpClient.get<TemplateDto[]>('/templates');
  },

  async getById(id: number): Promise<TemplateDto> {
    return HttpClient.get<TemplateDto>(`/templates/${id}`);
  },

  async getByCategory(category: string): Promise<TemplateDto[]> {
    return HttpClient.get<TemplateDto[]>(`/templates/category/${category}`);
  },

  async create(data: CreateTemplateDto): Promise<TemplateDto> {
    return HttpClient.post<TemplateDto>('/templates', data);
  },

  async update(id: number, data: UpdateTemplateDto): Promise<TemplateDto> {
    return HttpClient.put<TemplateDto>(`/templates/${id}`, data);
  },

  async delete(id: number): Promise<void> {
    return HttpClient.delete<void>(`/templates/${id}`);
  }
};
