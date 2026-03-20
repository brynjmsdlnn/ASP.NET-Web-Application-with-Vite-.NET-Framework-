// ============================================================================
// THEME MANAGER
// ============================================================================
const ThemeManager = {
    key: 'template-theme',
    toggleBtn: null,
    iconEl: null,
    root: document.documentElement,

    init() {
        this.toggleBtn = document.getElementById('theme-toggle');
        this.iconEl = document.getElementById('theme-toggle-icon');
        
        // 1. Get initial state (which was already set by the inline script in <head>)
        const isCurrentlyDark = this.root.classList.contains('dark');
        this.updateUI(isCurrentlyDark);

        // 2. Listen for clicks on the toggle button
        if (this.toggleBtn) {
            this.toggleBtn.addEventListener('click', () => this.toggleUserPreference());
        }

        // 3. Listen for OS-level theme changes (e.g., sunset automatic dark mode)
        window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', (e) => {
            // Only auto-switch if the user hasn't explicitly saved a preference
            if (!localStorage.getItem(this.key)) {
                this.applyTheme(e.matches);
            }
        });
    },

    toggleUserPreference() {
        const isDark = this.root.classList.contains('dark');
        const willBeDark = !isDark;
        
        // Save their explicit choice
        localStorage.setItem(this.key, willBeDark ? 'dark' : 'light');
        this.applyTheme(willBeDark);
    },

    applyTheme(isDark) {
        if (isDark) {
            this.root.classList.add('dark');
        } else {
            this.root.classList.remove('dark');
        }
        this.updateUI(isDark);
    },

    updateUI(isDark) {
        // Safely update the DOM elements if they exist
        if (this.toggleBtn) {
            this.toggleBtn.setAttribute('aria-label', isDark ? 'Switch to light mode' : 'Switch to dark mode');
        }
        if (this.iconEl) {
            requestAnimationFrame(() => {
                this.iconEl.textContent = isDark ? '☀️' : '🌙';
                this.iconEl.setAttribute('aria-hidden', 'true');
            });
        }
    }
};

// ============================================================================
// MOBILE MENU MANAGER
// ============================================================================
const MenuManager = {
    toggleBtn: null,
    iconEl: null,
    menuEl: null,
    links: null,

    init() {
        this.toggleBtn = document.getElementById('mobile-menu-toggle');
        this.iconEl = document.getElementById('menu-toggle-icon');
        this.menuEl = document.getElementById('site-menu');
        this.links = document.querySelectorAll('#site-menu a');
        
        if (!this.toggleBtn || !this.menuEl) return;

        this.toggleBtn.addEventListener('click', () => {
            this.menuEl.classList.toggle('hidden');
            const isOpen = !this.menuEl.classList.contains('hidden');
            this.updateUI(isOpen);
        });

        // Close menu when a link is clicked on mobile
        this.links.forEach(link => {
            link.addEventListener('click', () => {
                if (window.innerWidth < 768) {
                    this.menuEl.classList.add('hidden');
                    this.updateUI(false);
                }
            });
        });
    },

    updateUI(isOpen) {
        this.toggleBtn.setAttribute('aria-expanded', isOpen.toString());
        this.toggleBtn.setAttribute('aria-label', isOpen ? 'Close navigation' : 'Open navigation');
        
        if (this.iconEl) {
            requestAnimationFrame(() => {
                this.iconEl.textContent = isOpen ? '✕' : '☰';
                this.iconEl.setAttribute('aria-hidden', 'true');
            });
        }
    }
};

// ============================================================================
// BOOT
// ============================================================================
document.addEventListener('DOMContentLoaded', () => {
    ThemeManager.init();
    MenuManager.init();
});
