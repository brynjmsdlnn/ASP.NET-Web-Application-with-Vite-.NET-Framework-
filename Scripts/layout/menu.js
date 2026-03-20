// ============================================================================
// MOBILE MENU MANAGER
// ============================================================================
// Handles mobile navigation visibility and icon state for small-screen toggling.
const MenuManager = {
  toggleBtn: document.getElementById('mobile-menu-toggle'),
  menuEl: document.getElementById('site-menu'),
  links: document.querySelectorAll('#site-menu a'),
  iconMenu: document.getElementById('icon-menu'),
  iconClose: document.getElementById('icon-close'),

  init() {
    // Exit early in layouts where menu controls are not present.
    if (!this.toggleBtn || !this.menuEl) return;

    // Toggle the menu visibility and sync accessibility state.
    this.toggleBtn.addEventListener('click', () => {
      this.menuEl.classList.toggle('hidden');
      const isOpen = !this.menuEl.classList.contains('hidden');
      this.updateUI(isOpen);
    });

    // Auto-close menu after navigation on narrow viewports.
    this.links.forEach((link) => {
      link.addEventListener('click', () => {
        if (window.innerWidth < 768) {
          this.menuEl.classList.add('hidden');
          this.updateUI(false);
        }
      });
    });
  },

  updateUI(isOpen) {
    // Keep screen readers in sync with menu state.
    this.toggleBtn.setAttribute('aria-expanded', isOpen.toString());
    this.toggleBtn.setAttribute('aria-label', isOpen ? 'Close navigation' : 'Open navigation');

    // Swap menu icons by class; CSS handles icon transitions via class state.
    this.iconMenu?.classList.toggle('hidden', isOpen);
    this.iconClose?.classList.toggle('hidden', !isOpen);
  }
};

export default MenuManager;
