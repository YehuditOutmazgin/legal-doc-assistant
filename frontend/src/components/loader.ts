export class Loader {
  private static overlay: HTMLElement | null = null;

  static show() {
    if (this.overlay) return;

    this.overlay = document.createElement('div');
    this.overlay.className = 'loader-overlay';
    this.overlay.innerHTML = '<div class="loader"></div>';
    document.body.appendChild(this.overlay);
  }

  static hide() {
    if (this.overlay) {
      this.overlay.remove();
      this.overlay = null;
    }
  }
}
