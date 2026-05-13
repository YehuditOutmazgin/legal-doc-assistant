import { HttpClient } from './http.client';
import type {
  ClientDto,
  CreateClientDto,
  UpdateClientDto
} from '@/types/client.types';

export const ClientsApi = {
  async getAll(): Promise<ClientDto[]> {
    return HttpClient.get<ClientDto[]>('/clients');
  },

  async getById(id: number): Promise<ClientDto> {
    return HttpClient.get<ClientDto>(`/clients/${id}`);
  },

  async create(data: CreateClientDto): Promise<ClientDto> {
    return HttpClient.post<ClientDto>('/clients', data);
  },

  async update(id: number, data: UpdateClientDto): Promise<ClientDto> {
    return HttpClient.put<ClientDto>(`/clients/${id}`, data);
  },

  async delete(id: number): Promise<void> {
    return HttpClient.delete<void>(`/clients/${id}`);
  }
};
