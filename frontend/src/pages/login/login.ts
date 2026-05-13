import { AuthApi } from '@/api/auth.api';
import { ValidationUtils } from '@/utils/validation.utils';
import type { LoginDto } from '@/types/auth.types';

// Check if already logged in
if (AuthApi.isAuthenticated()) {
  window.location.href = '/src/pages/dashboard/dashboard.html';
}

// DOM Elements
const loginForm = document.getElementById('loginForm') as HTMLFormElement;
const emailInput = document.getElementById('email') as HTMLInputElement;
const passwordInput = document.getElementById('password') as HTMLInputElement;
const loginBtn = document.getElementById('loginBtn') as HTMLButtonElement;
const loginBtnText = document.getElementById('loginBtnText') as HTMLSpanElement;
const loginBtnLoader = document.getElementById('loginBtnLoader') as HTMLSpanElement;
const emailError = document.getElementById('emailError') as HTMLDivElement;
const passwordError = document.getElementById('passwordError') as HTMLDivElement;
const generalError = document.getElementById('generalError') as HTMLDivElement;

// Validation
function validateForm(): boolean {
  let isValid = true;

  // Reset errors
  emailError.classList.add('hidden');
  passwordError.classList.add('hidden');
  generalError.classList.add('hidden');

  // Validate email
  if (!ValidationUtils.isRequired(emailInput.value)) {
    emailError.textContent = ValidationUtils.getErrorMessage('Email', 'required');
    emailError.classList.remove('hidden');
    isValid = false;
  } else if (!ValidationUtils.isValidEmail(emailInput.value)) {
    emailError.textContent = ValidationUtils.getErrorMessage('Email', 'email');
    emailError.classList.remove('hidden');
    isValid = false;
  }

  // Validate password
  if (!ValidationUtils.isRequired(passwordInput.value)) {
    passwordError.textContent = ValidationUtils.getErrorMessage('Password', 'required');
    passwordError.classList.remove('hidden');
    isValid = false;
  }

  return isValid;
}

// Show loading state
function setLoading(loading: boolean) {
  loginBtn.disabled = loading;
  if (loading) {
    loginBtnText.classList.add('hidden');
    loginBtnLoader.classList.remove('hidden');
  } else {
    loginBtnText.classList.remove('hidden');
    loginBtnLoader.classList.add('hidden');
  }
}

// Show toast notification
function showToast(message: string, type: 'success' | 'error' = 'error') {
  const toastContainer = document.getElementById('toastContainer');
  if (!toastContainer) return;

  const toast = document.createElement('div');
  toast.className = `toast toast-${type}`;
  toast.innerHTML = `
    <div style="flex: 1;">${message}</div>
  `;

  toastContainer.appendChild(toast);

  setTimeout(() => {
    toast.remove();
  }, 5000);
}

// Handle form submission
loginForm.addEventListener('submit', async (e) => {
  e.preventDefault();

  if (!validateForm()) {
    return;
  }

  setLoading(true);

  const credentials: LoginDto = {
    email: emailInput.value.trim(),
    password: passwordInput.value
  };

  try {
    await AuthApi.login(credentials);
    
    showToast('Login successful!', 'success');
    
    // Redirect to dashboard
    setTimeout(() => {
      window.location.href = '/src/pages/dashboard/dashboard.html';
    }, 500);
  } catch (error: any) {
    console.error('Login error:', error);
    
    const errorMessage = error.message || 'Login failed. Please try again.';
    generalError.textContent = errorMessage;
    generalError.classList.remove('hidden');
    
    showToast(errorMessage, 'error');
  } finally {
    setLoading(false);
  }
});

// Clear error on input
emailInput.addEventListener('input', () => {
  emailError.classList.add('hidden');
  generalError.classList.add('hidden');
});

passwordInput.addEventListener('input', () => {
  passwordError.classList.add('hidden');
  generalError.classList.add('hidden');
});
