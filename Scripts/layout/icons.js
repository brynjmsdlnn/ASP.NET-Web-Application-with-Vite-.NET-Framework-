// ============================================================================
// ICON REGISTRATION
// ============================================================================
import { createIcons, Moon, Sun, Menu, X } from 'lucide';

// Registers Lucide icon definitions and replaces all matching `data-lucide`
// placeholders in the DOM with inline SVG icon elements.
export function initIcons() {
  createIcons({
    // Keep this list as the explicit icon contract for the app shell.
    icons: {
      Moon,
      Sun,
      Menu,
      X
    }
  });
}
