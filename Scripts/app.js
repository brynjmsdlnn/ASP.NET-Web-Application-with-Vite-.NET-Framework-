import { createIcons, Moon, Sun, Menu, X } from 'lucide';

createIcons({
    icons: {
        Moon,
        Sun,
        Menu,
        X
    }
});

// ============================================================================
// THEME MANAGER
// ============================================================================
const ThemeManager = {
    key: 'template-theme',
    toggleBtn: document.getElementById('theme-toggle'),
    root: document.documentElement,

    init() {
        const isCurrentlyDark = this.root.classList.contains('dark');
        this.updateAria(isCurrentlyDark);

        if (this.toggleBtn) {
            this.toggleBtn.addEventListener('click', () => this.toggleUserPreference());
        }

        window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', (e) => {
            if (!localStorage.getItem(this.key)) {
                this.applyTheme(e.matches);
            }
        });
    },

    toggleUserPreference() {
        const isDark = this.root.classList.contains('dark');
        const willBeDark = !isDark;
        
        localStorage.setItem(this.key, willBeDark ? 'dark' : 'light');
        this.applyTheme(willBeDark);
    },

    applyTheme(isDark) {
        if (isDark) {
            this.root.classList.add('dark');
        } else {
            this.root.classList.remove('dark');
        }
        this.updateAria(isDark);
    },

    updateAria(isDark) {
        if (this.toggleBtn) {
            this.toggleBtn.setAttribute('aria-label', isDark ? 'Switch to light mode' : 'Switch to dark mode');
        }
    }
};

// ============================================================================
// MOBILE MENU MANAGER
// ============================================================================
const MenuManager = {
    toggleBtn: document.getElementById('mobile-menu-toggle'),
    menuEl: document.getElementById('site-menu'),
    links: document.querySelectorAll('#site-menu a'),
    iconMenu: document.getElementById('icon-menu'),
    iconClose: document.getElementById('icon-close'),

    init() {
        if (!this.toggleBtn || !this.menuEl) return;

        this.toggleBtn.addEventListener('click', () => {
            this.menuEl.classList.toggle('hidden');
            const isOpen = !this.menuEl.classList.contains('hidden');
            this.updateUI(isOpen);
        });

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

        if (this.iconMenu && this.iconClose) {
            if (isOpen) {
                this.iconMenu.classList.add('hidden');
                this.iconClose.classList.remove('hidden');
            } else {
                this.iconMenu.classList.remove('hidden');
                this.iconClose.classList.add('hidden');
            }
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
