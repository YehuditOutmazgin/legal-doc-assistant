export interface ModalOptions {
  title: string;
  content: string | HTMLElement;
  onConfirm?: () => void | Promise<void>;
  onCancel?: () => void;
  confirmText?: string;
  cancelText?: string;
  showCancel?: boolean;
}

export class Modal {
  private overlay: HTMLElement;
  private modal: HTMLElement;

  constructor(private options: ModalOptions) {
    this.overlay = this.createOverlay();
    this.modal = this.createModal();
    this.overlay.appendChild(this.modal);
  }

  private createOverlay(): HTMLElement {
    const overlay = document.createElement('div');
    overlay.className = 'modal-overlay';
    overlay.addEventListener('click', (e) => {
      if (e.target === overlay) {
        this.close();
      }
    });
    return overlay;
  }

  private createModal(): HTMLElement {
    const modal = document.createElement('div');
    modal.className = 'modal';

    // Header
    const header = document.createElement('div');
    header.className = 'modal-header';
    header.innerHTML = `
      <h3 class="modal-title">${this.options.title}</h3>
      <button class="modal-close" aria-label="Close">×</button>
    `;
    header.querySelector('.modal-close')?.addEventListener('click', () => this.close());

    // Body
    const body = document.createElement('div');
    body.className = 'modal-body';
    if (typeof this.options.content === 'string') {
      body.innerHTML = this.options.content;
    } else {
      body.appendChild(this.options.content);
    }

    // Footer
    const footer = document.createElement('div');
    footer.className = 'modal-footer';

    if (this.options.showCancel !== false) {
      const cancelBtn = document.createElement('button');
      cancelBtn.className = 'btn btn-secondary';
      cancelBtn.textContent = this.options.cancelText || 'Cancel';
      cancelBtn.addEventListener('click', () => {
        if (this.options.onCancel) {
          this.options.onCancel();
        }
        this.close();
      });
      footer.appendChild(cancelBtn);
    }

    if (this.options.onConfirm) {
      const confirmBtn = document.createElement('button');
      confirmBtn.className = 'btn btn-primary';
      confirmBtn.textContent = this.options.confirmText || 'Confirm';
      confirmBtn.addEventListener('click', async () => {
        if (this.options.onConfirm) {
          await this.options.onConfirm();
        }
        this.close();
      });
      footer.appendChild(confirmBtn);
    }

    modal.appendChild(header);
    modal.appendChild(body);
    if (footer.children.length > 0) {
      modal.appendChild(footer);
    }

    return modal;
  }

  show() {
    document.body.appendChild(this.overlay);
  }

  close() {
    this.overlay.remove();
  }

  static confirm(title: string, message: string): Promise<boolean> {
    return new Promise((resolve) => {
      const modal = new Modal({
        title,
        content: `<p>${message}</p>`,
        confirmText: 'Confirm',
        cancelText: 'Cancel',
        onConfirm: () => resolve(true),
        onCancel: () => resolve(false)
      });
      modal.show();
    });
  }

  static alert(title: string, message: string): Promise<void> {
    return new Promise((resolve) => {
      const modal = new Modal({
        title,
        content: `<p>${message}</p>`,
        confirmText: 'Close',
        showCancel: false,
        onConfirm: () => resolve()
      });
      modal.show();
    });
  }
}
