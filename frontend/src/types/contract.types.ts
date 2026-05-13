// Enums
export enum ContractStatus {
  DRAFT = 'DRAFT',
  REVIEW = 'REVIEW',
  SIGNED = 'SIGNED',
  ARCHIVED = 'ARCHIVED'
}

// DTOs
export interface ContractDto {
  id: number;
  title: string;
  content: string;
  status: ContractStatus;
  clientId: number;
  clientName: string;
  templateId?: number;
  templateName?: string;
  createdByUserId: number;
  createdByUserName: string;
  assignedToUserId?: number;
  assignedToUserName?: string;
  s3Key?: string;
  createdAt: string;
  updatedAt?: string;
  signedAt?: string;
  signedByName?: string;
  notes?: string;
}

export interface CreateContractDto {
  title: string;
  content: string;
  status?: ContractStatus;
  clientId: number;
  templateId?: number;
  assignedToUserId?: number;
  notes?: string;
}

export interface UpdateContractDto {
  title?: string;
  content?: string;
  status?: ContractStatus;
  assignedToUserId?: number;
  signedByName?: string;
  notes?: string;
}
