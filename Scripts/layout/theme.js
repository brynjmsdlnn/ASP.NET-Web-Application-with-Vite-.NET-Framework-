// ============================================================================
// THEME MANAGER
// ============================================================================
// Manages dark-mode state, persistence, and accessibility labels for the theme
// toggle button.
const ThemeManager = {
  key: 'template-theme',
  toggleBtn: document.getElementById('theme-toggle'),
  root: document.documentElement,

  init() {
    // Use the current root class to initialize button label state.
    const isCurrentlyDark = this.root.classList.contains('dark');
    this.updateAria(isCurrentlyDark);

    // User-triggered toggle for template theme preference.
    if (this.toggleBtn) {
      this.toggleBtn.addEventListener('click', () => this.toggleUserPreference());
    }

    // Keep user preference aligned with system theme when no explicit override exists.
    window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', (e) => {
      if (!localStorage.getItem(this.key)) {
        this.applyTheme(e.matches);
      }
    });
  },

  toggleUserPreference() {
    // Flip the current class-based theme state and persist the next value.
    const willBeDark = !this.root.classList.contains('dark');
    localStorage.setItem(this.key, willBeDark ? 'dark' : 'light');
    this.applyTheme(willBeDark);
  },

  applyTheme(isDark) {
    // Use the boolean toggle signature to keep class changes explicit.
    this.root.classList.toggle('dark', isDark);
    this.updateAria(isDark);
  },

  updateAria(isDark) {
    // Update only accessibility copy; icon visibility is handled by CSS classes.
    if (this.toggleBtn) {
      this.toggleBtn.setAttribute(
        'aria-label',
        isDark ? 'Switch to light mode' : 'Switch to dark mode'
      );
    }
  }
};

export default ThemeManager;
