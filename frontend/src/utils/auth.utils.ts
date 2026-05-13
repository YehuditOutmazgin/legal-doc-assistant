import { AuthApi } from '@/api/auth.api';
import type { UserDto, UserRole } from '@/types/auth.types';

export const AuthUtils = {
  /**
   * Require authentication - redirect to login if not authenticated
   */
  requireAuth(): void {
    if (!AuthApi.isAuthenticated()) {
      window.location.href = '/src/pages/login/login.html';
    }
  },

  /**
   * Get current user from localStorage
   */
  getUser(): UserDto | null {
    return AuthApi.getStoredUser();
  },

  /**
   * Get user role
   */
  getRole(): UserRole | null {
    const user = this.getUser();
    return user?.role || null;
  },

  /**
   * Check if user has specific role
   */
  hasRole(role: UserRole): boolean {
    return this.getRole() === role;
  },

  /**
   * Check if user has any of the specified roles
   */
  hasAnyRole(roles: UserRole[]): boolean {
    const userRole = this.getRole();
    return userRole ? roles.includes(userRole) : false;
  },

  /**
   * Redirect to login page
   */
  redirectToLogin(): void {
    window.location.href = '/src/pages/login/login.html';
  },

  /**
   * Redirect to dashboard
   */
  redirectToDashboard(): void {
    window.location.href = '/src/pages/dashboard/dashboard.html';
  }
};
