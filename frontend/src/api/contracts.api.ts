import { HttpClient } from './http.client';
import type {
  ContractDto,
  CreateContractDto,
  UpdateContractDto
} from '@/types/contract.types';

export const ContractsApi = {
  async getAll(): Promise<ContractDto[]> {
    return HttpClient.get<ContractDto[]>('/contracts');
  },

  async getById(id: number): Promise<ContractDto> {
    return HttpClient.get<ContractDto>(`/contracts/${id}`);
  },

  async getByClient(clientId: number): Promise<ContractDto[]> {
    return HttpClient.get<ContractDto[]>(`/contracts/client/${clientId}`);
  },

  async create(data: CreateContractDto): Promise<ContractDto> {
    return HttpClient.post<ContractDto>('/contracts', data);
  },

  async update(id: number, data: UpdateContractDto): Promise<ContractDto> {
    return HttpClient.put<ContractDto>(`/contracts/${id}`, data);
  },

  async delete(id: number): Promise<void> {
    return HttpClient.delete<void>(`/contracts/${id}`);
  }
};
