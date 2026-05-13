// Enums
export enum ClientType {
  INDIVIDUAL = 'INDIVIDUAL',
  COMPANY = 'COMPANY'
}

// DTOs
export interface ClientDto {
  id: number;
  name: string;
  type: ClientType;
  email: string;
  phone: string;
  address: string;
  companyRegistrationNumber?: string;
  contactPersonName?: string;
  createdAt: string;
  updatedAt?: string;
  isActive: boolean;
}

export interface CreateClientDto {
  name: string;
  type: ClientType;
  email: string;
  phone: string;
  address: string;
  companyRegistrationNumber?: string;
  contactPersonName?: string;
}

export interface UpdateClientDto {
  name?: string;
  email?: string;
  phone?: string;
  address?: string;
  companyRegistrationNumber?: string;
  contactPersonName?: string;
  isActive?: boolean;
}
