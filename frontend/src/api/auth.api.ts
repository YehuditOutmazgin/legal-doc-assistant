import { HttpClient, TokenManager } from './http.client';
import type {
  LoginDto,
  RegisterDto,
  AuthResponseDto,
  UserDto,
  ChangePasswordDto
} from '@/types/auth.types';

export const AuthApi = {
  async login(credentials: LoginDto): Promise<AuthResponseDto> {
    const response = await HttpClient.post<AuthResponseDto>(
      '/auth/login',
      credentials,
      { skipAuth: true }
    );
    
    // Save access token (refresh token saved automatically in Cookie by backend)
    TokenManager.setTokens(response.token, '', response.expiresAt);
    
    // Save user data
    localStorage.setItem('user', JSON.stringify(response.user));
    
    return response;
  },

  async register(data: RegisterDto): Promise<AuthResponseDto> {
    const response = await HttpClient.post<AuthResponseDto>(
      '/auth/register',
      data,
      { skipAuth: true }
    );
    
    // Save access token (refresh token saved automatically in Cookie)
    TokenManager.setTokens(response.token, '', response.expiresAt);
    localStorage.setItem('user', JSON.stringify(response.user));
    
    return response;
  },

  async logout(): Promise<void> {
    try {
      await HttpClient.post('/auth/logout');
    } finally {
      TokenManager.clearTokens();
      localStorage.removeItem('user');
      window.location.href = '/src/pages/login/login.html';
    }
  },

  async getCurrentUser(): Promise<UserDto> {
    return HttpClient.get<UserDto>('/auth/me');
  },

  async changePassword(data: ChangePasswordDto): Promise<void> {
    return HttpClient.post('/auth/change-password', data);
  },

  isAuthenticated(): boolean {
    return TokenManager.getToken() !== null;
  },

  getStoredUser(): UserDto | null {
    const userStr = localStorage.getItem('user');
    return userStr ? JSON.parse(userStr) : null;
  }
};
