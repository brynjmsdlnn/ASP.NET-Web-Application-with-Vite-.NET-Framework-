// ============================================================================
// APP ENTRY POINT
// ============================================================================
// Bootstraps all UI modules after the DOM is fully available.
import { initIcons } from './layout/icons.js';
import ThemeManager from './layout/theme.js';
import MenuManager from './layout/menu.js';

// ============================================================================
// BOOT
// ============================================================================
// 1) Render Lucide SVG icons for all [data-lucide] placeholders.
// 2) Initialize the theme toggle behavior (with persistence + system sync).
// 3) Initialize the mobile menu interaction handlers.
document.addEventListener('DOMContentLoaded', () => {
  initIcons();
  ThemeManager.init();
  MenuManager.init();
});
