import Cookies from 'js-cookie';
import type { ApiError } from '@/types/api.types';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api';

// Token management
const TOKEN_KEY = 'auth_token';
const REFRESH_TOKEN_KEY = 'refresh_token';

export const TokenManager = {
  getToken: (): string | null => Cookies.get(TOKEN_KEY) || null,
  
  getRefreshToken: (): string | null => Cookies.get(REFRESH_TOKEN_KEY) || null,
  
  setTokens: (token: string, refreshToken: string, expiresAt: string) => {
    const expiresDate = new Date(expiresAt);
    Cookies.set(TOKEN_KEY, token, { expires: expiresDate, sameSite: 'strict' });
    Cookies.set(REFRESH_TOKEN_KEY, refreshToken, { expires: 7, sameSite: 'strict' });
  },
  
  clearTokens: () => {
    Cookies.remove(TOKEN_KEY);
    Cookies.remove(REFRESH_TOKEN_KEY);
  }
};

// Refresh token logic
let isRefreshing = false;
let refreshSubscribers: ((token: string) => void)[] = [];

function subscribeTokenRefresh(callback: (token: string) => void) {
  refreshSubscribers.push(callback);
}

function onTokenRefreshed(token: string) {
  refreshSubscribers.forEach(callback => callback(token));
  refreshSubscribers = [];
}

async function refreshAccessToken(): Promise<string> {
  // Backend sends refresh token in Cookie, no need to send it in request body
  const response = await fetch(`${API_BASE_URL}/auth/refresh`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    credentials: 'include' // Important! To send cookies
  });

  if (!response.ok) {
    TokenManager.clearTokens();
    window.location.href = '/src/pages/login/login.html';
    throw new Error('Failed to refresh token');
  }

  const data = await response.json();
  // Backend returns empty refreshToken, cookie is updated automatically
  TokenManager.setTokens(data.token, data.refreshToken || TokenManager.getRefreshToken() || '', data.expiresAt);
  return data.token;
}

// HTTP Client
interface RequestOptions extends RequestInit {
  skipAuth?: boolean;
}

export class HttpClient {
  private static async request<T>(
    endpoint: string,
    options: RequestOptions = {}
  ): Promise<T> {
    const { skipAuth = false, ...fetchOptions } = options;
    
    const headers: HeadersInit = {
      'Content-Type': 'application/json',
      ...fetchOptions.headers
    };

    // Add auth token if not skipped
    if (!skipAuth) {
      const token = TokenManager.getToken();
      if (token) {
        headers['Authorization'] = `Bearer ${token}`;
      }
    }

    const url = `${API_BASE_URL}${endpoint}`;
    
    try {
      let response = await fetch(url, {
        ...fetchOptions,
        headers,
        credentials: 'include' // Important! To send and receive cookies
      });

      // Handle 401 - try to refresh token
      if (response.status === 401 && !skipAuth) {
        if (!isRefreshing) {
          isRefreshing = true;
          try {
            const newToken = await refreshAccessToken();
            isRefreshing = false;
            onTokenRefreshed(newToken);
            
            // Retry original request with new token
            headers['Authorization'] = `Bearer ${newToken}`;
            response = await fetch(url, {
              ...fetchOptions,
              headers,
              credentials: 'include'
            });
          } catch (error) {
            isRefreshing = false;
            throw error;
          }
        } else {
          // Wait for token refresh
          const newToken = await new Promise<string>((resolve) => {
            subscribeTokenRefresh(resolve);
          });
          
          headers['Authorization'] = `Bearer ${newToken}`;
          response = await fetch(url, {
            ...fetchOptions,
            headers,
            credentials: 'include'
          });
        }
      }

      if (!response.ok) {
        const error: ApiError = await response.json().catch(() => ({
          message: 'An error occurred',
          statusCode: response.status
        }));
        throw error;
      }

      return await response.json();
    } catch (error) {
      console.error('HTTP Request failed:', error);
      throw error;
    }
  }

  static get<T>(endpoint: string, options?: RequestOptions): Promise<T> {
    return this.request<T>(endpoint, { ...options, method: 'GET' });
  }

  static post<T>(endpoint: string, data?: unknown, options?: RequestOptions): Promise<T> {
    return this.request<T>(endpoint, {
      ...options,
      method: 'POST',
      body: data ? JSON.stringify(data) : undefined
    });
  }

  static put<T>(endpoint: string, data?: unknown, options?: RequestOptions): Promise<T> {
    return this.request<T>(endpoint, {
      ...options,
      method: 'PUT',
      body: data ? JSON.stringify(data) : undefined
    });
  }

  static delete<T>(endpoint: string, options?: RequestOptions): Promise<T> {
    return this.request<T>(endpoint, { ...options, method: 'DELETE' });
  }
}
