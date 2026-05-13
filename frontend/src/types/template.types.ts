// DTOs
export interface TemplateDto {
  id: number;
  name: string;
  description: string;
  content: string;
  category: string;
  createdByUserId: number;
  createdByUserName: string;
  createdAt: string;
  updatedAt?: string;
  isActive: boolean;
}

export interface CreateTemplateDto {
  name: string;
  description: string;
  content: string;
  category: string;
}

export interface UpdateTemplateDto {
  name?: string;
  description?: string;
  content?: string;
  category?: string;
  isActive?: boolean;
}
