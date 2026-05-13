export type ToastType = 'success' | 'error' | 'warning' | 'info';

export class Toast {
  private static container: HTMLElement | null = null;

  private static getContainer(): HTMLElement {
    if (!this.container) {
      this.container = document.getElementById('toastContainer');
      if (!this.container) {
        this.container = document.createElement('div');
        this.container.id = 'toastContainer';
        this.container.className = 'toast-container';
        document.body.appendChild(this.container);
      }
    }
    return this.container;
  }

  static show(message: string, type: ToastType = 'info', duration: number = 5000) {
    const container = this.getContainer();
    
    const toast = document.createElement('div');
    toast.className = `toast toast-${type}`;
    toast.innerHTML = `
      <div style="flex: 1;">${message}</div>
      <button class="toast-close" aria-label="Close">×</button>
    `;

    const closeBtn = toast.querySelector('.toast-close');
    closeBtn?.addEventListener('click', () => {
      toast.remove();
    });

    container.appendChild(toast);

    setTimeout(() => {
      toast.remove();
    }, duration);
  }

  static success(message: string, duration?: number) {
    this.show(message, 'success', duration);
  }

  static error(message: string, duration?: number) {
    this.show(message, 'error', duration);
  }

  static warning(message: string, duration?: number) {
    this.show(message, 'warning', duration);
  }

  static info(message: string, duration?: number) {
    this.show(message, 'info', duration);
  }
}

// Add toast close button styles
const style = document.createElement('style');
style.textContent = `
  .toast-close {
    width: 24px;
    height: 24px;
    display: flex;
    align-items: center;
    justify-content: center;
    border-radius: var(--radius-sm);
    font-size: 20px;
    line-height: 1;
    color: var(--color-gray-600);
    transition: background-color var(--transition-base);
  }

  .toast-close:hover {
    background-color: var(--color-gray-100);
  }
`;
document.head.appendChild(style);
