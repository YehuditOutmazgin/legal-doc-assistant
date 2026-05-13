export const ValidationUtils = {
  /**
   * Validate email format
   */
  isValidEmail(email: string): boolean {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  },

  /**
   * Validate password strength
   */
  isValidPassword(password: string): boolean {
    return password.length >= 6;
  },

  /**
   * Validate phone number
   */
  isValidPhone(phone: string): boolean {
    const phoneRegex = /^[\d\s\-\+\(\)]+$/;
    return phoneRegex.test(phone) && phone.replace(/\D/g, '').length >= 10;
  },

  /**
   * Validate required field
   */
  isRequired(value: string): boolean {
    return value.trim().length > 0;
  },

  /**
   * Get validation error message
   */
  getErrorMessage(field: string, type: 'required' | 'email' | 'password' | 'phone'): string {
    const messages = {
      required: `${field} is required`,
      email: 'Invalid email address',
      password: 'Password must be at least 6 characters',
      phone: 'Invalid phone number'
    };
    return messages[type];
  }
};
