import { Component, ChangeDetectionStrategy, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../../features/auth/services/auth.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, RouterLinkActive],
  template: `
    <!-- Mobile Top Header Bar (Visible on mobile view) -->
    <div class="mobile-topbar">
      <a routerLink="/" (click)="closeMobileMenu()" class="brand-link">
        <div class="brand-icon">
          <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.3" stroke-linecap="round" stroke-linejoin="round">
            <path d="M14.7 6.3a1 1 0 0 0 0 1.4l1.6 1.6a1 1 0 0 0 1.4 0l3.77-3.77a6 6 0 0 1-7.94 7.94l-6.91 6.91a2.12 2.12 0 0 1-3-3l6.91-6.91a6 6 0 0 1 7.94-7.94l-3.76 3.76z"></path>
          </svg>
        </div>
        <span class="title">AutoReparos</span>
      </a>

      <button (click)="toggleMobileMenu()" class="mobile-toggle-btn" aria-label="Abrir Menu">
        @if (isMobileMenuOpen()) {
          <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
        } @else {
          <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="3" y1="12" x2="21" y2="12"/><line x1="3" y1="6" x2="21" y2="6"/><line x1="3" y1="18" x2="21" y2="18"/></svg>
        }
      </button>
    </div>

    <!-- Main Sidebar Container -->
    <aside class="sidebar" [class.mobile-open]="isMobileMenuOpen()">
      <!-- Desktop Sidebar Brand Header -->
      <div class="sidebar-brand">
        <a routerLink="/" (click)="closeMobileMenu()" class="brand-link">
          <div class="brand-icon">
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.3" stroke-linecap="round" stroke-linejoin="round">
              <path d="M14.7 6.3a1 1 0 0 0 0 1.4l1.6 1.6a1 1 0 0 0 1.4 0l3.77-3.77a6 6 0 0 1-7.94 7.94l-6.91 6.91a2.12 2.12 0 0 1-3-3l6.91-6.91a6 6 0 0 1 7.94-7.94l-3.76 3.76z"></path>
            </svg>
          </div>
          <div class="brand-text">
            <span class="title">AutoReparos</span>
            <span class="subtitle">Oficina Mecânica</span>
          </div>
        </a>
      </div>

      <!-- Sidebar Navigation Menu -->
      <div class="sidebar-content">
        @if (auth.currentUser()) {
          <div class="nav-section">
            <span class="nav-section-title">Navegação Principal</span>
            <nav class="nav-menu">
              <a routerLink="/" routerLinkActive="active" [routerLinkActiveOptions]="{exact: true}" (click)="closeMobileMenu()" class="nav-item">
                <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="m3 9 9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/><polyline points="9 22 9 12 15 12 15 22"/></svg>
                <span>Início</span>
              </a>
              <a routerLink="/ordens-servico/fila" routerLinkActive="active" (click)="closeMobileMenu()" class="nav-item">
                <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="3" width="18" height="18" rx="2"/><path d="M9 3v18"/><path d="M15 3v18"/></svg>
                <span>Fila Kanban</span>
              </a>
              <a routerLink="/ordens-servico" routerLinkActive="active" [routerLinkActiveOptions]="{exact: true}" (click)="closeMobileMenu()" class="nav-item">
                <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/><line x1="16" y1="13" x2="8" y2="13"/><line x1="16" y1="17" x2="8" y2="17"/></svg>
                <span>Ordens de Serviço</span>
              </a>
              <a routerLink="/clientes" routerLinkActive="active" (click)="closeMobileMenu()" class="nav-item">
                <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/></svg>
                <span>Clientes</span>
              </a>
              <a routerLink="/veiculos" routerLinkActive="active" (click)="closeMobileMenu()" class="nav-item">
                <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M19 17h2c.6 0 1-.4 1-1v-3c0-.9-.7-1.7-1.5-1.9C18.7 10.6 16 10 16 10s-1.3-1.4-2.2-2.3c-.5-.4-1.1-.7-1.8-.7H5c-.6 0-1.1.4-1.4.9l-1.5 3C2 11.3 2 11.7 2 12v4c0 .6.4 1 1 1h2"/><circle cx="7" cy="17" r="2"/><circle cx="17" cy="17" r="2"/></svg>
                <span>Veículos</span>
              </a>
            </nav>
          </div>

          @if (auth.hasRole(['Administrador', 'Mecanico', 'Atendente'])) {
            <div class="nav-section">
              <span class="nav-section-title">Gestão e Administração</span>
              <nav class="nav-menu">
                @if (auth.hasRole(['Administrador'])) {
                  <a routerLink="/dashboard" routerLinkActive="active" (click)="closeMobileMenu()" class="nav-item">
                    <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="3" width="7" height="7"/><rect x="14" y="3" width="7" height="7"/><rect x="14" y="14" width="7" height="7"/><rect x="3" y="14" width="7" height="7"/></svg>
                    <span>Dashboard</span>
                  </a>
                }
                @if (auth.hasRole(['Administrador', 'Mecanico'])) {
                  <a routerLink="/insumos" routerLinkActive="active" (click)="closeMobileMenu()" class="nav-item">
                    <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z"/></svg>
                    <span>Estoque e Insumos</span>
                  </a>
                }
                @if (auth.hasRole(['Administrador', 'Atendente', 'Mecanico'])) {
                  <a routerLink="/servicos" routerLinkActive="active" (click)="closeMobileMenu()" class="nav-item">
                    <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 20h9"/><path d="M16.5 3.5a2.121 2.121 0 0 1 3 3L7 19l-4 1 1-4L16.5 3.5z"/></svg>
                    <span>Catálogo de Serviços</span>
                  </a>
                }
                @if (auth.hasRole(['Administrador'])) {
                  <a routerLink="/usuarios" routerLinkActive="active" (click)="closeMobileMenu()" class="nav-item">
                    <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M16 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="8.5" cy="7" r="4"/><line x1="20" y1="8" x2="20" y2="14"/><line x1="23" y1="11" x2="17" y2="11"/></svg>
                    <span>Usuários</span>
                  </a>
                }
              </nav>
            </div>
          }
        } @else {
          <div class="nav-section">
            <span class="nav-section-title">Portal do Cliente</span>
            <nav class="nav-menu">
              <a routerLink="/" routerLinkActive="active" [routerLinkActiveOptions]="{exact: true}" (click)="closeMobileMenu()" class="nav-item">
                <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="m3 9 9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/><polyline points="9 22 9 12 15 12 15 22"/></svg>
                <span>Início</span>
              </a>
              <a routerLink="/consulta-publica" routerLinkActive="active" (click)="closeMobileMenu()" class="nav-item">
                <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>
                <span>Consultar Status OS</span>
              </a>
              <a routerLink="/aprovar-orcamento" routerLinkActive="active" (click)="closeMobileMenu()" class="nav-item">
                <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/><line x1="16" y1="13" x2="8" y2="13"/><line x1="16" y1="17" x2="8" y2="17"/></svg>
                <span>Aprovar Orçamento</span>
              </a>
            </nav>
          </div>
        }
      </div>

      <!-- Sidebar Footer / User Info -->
      <div class="sidebar-footer">
        @if (auth.currentUser(); as user) {
          <div class="user-card">
            <div class="user-avatar">
              <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M19 21v-2a4 4 0 0 0-4-4H9a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/></svg>
            </div>
            <div class="user-info">
              <span class="user-name">{{ user.nome || user.nomeCompleto || user.email }}</span>
              <span class="role-tag">{{ user.role || 'Usuário' }}</span>
            </div>
            <button (click)="auth.logout()" class="btn-logout" title="Sair do Sistema">
              <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"/><polyline points="16 17 21 12 16 7"/><line x1="21" y1="12" x2="9" y2="12"/></svg>
            </button>
          </div>
        } @else {
          <a routerLink="/login" (click)="closeMobileMenu()" class="btn btn-primary btn-block">
            <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M15 3h4a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2h-4"/><polyline points="10 17 15 12 10 7"/><line x1="15" y1="12" x2="3" y2="12"/></svg>
            Acesso Interno
          </a>
        }
      </div>
    </aside>

    <!-- Backdrop for Mobile Navigation -->
    @if (isMobileMenuOpen()) {
      <div (click)="closeMobileMenu()" class="sidebar-backdrop"></div>
    }
  `,
  styles: [`
    :host {
      display: block;
    }
    .mobile-topbar {
      display: none;
    }
    .sidebar {
      width: 260px;
      height: 100vh;
      background: rgba(28, 28, 33, 0.95);
      backdrop-filter: var(--glass-blur);
      border-right: 1px solid var(--border-color);
      display: flex;
      flex-direction: column;
      position: sticky;
      top: 0;
      z-index: 100;
      box-shadow: var(--shadow-lg);
      transition: transform 0.3s ease, left 0.3s ease;
    }
    .sidebar-brand {
      padding: 1.5rem 1.25rem;
      border-bottom: 1px solid var(--border-color);
    }
    .brand-link {
      display: flex;
      align-items: center;
      gap: 0.85rem;
    }
    .brand-icon {
      width: 44px;
      height: 44px;
      background: linear-gradient(135deg, #F43F5E, #E11D48);
      border-radius: 12px;
      display: flex;
      align-items: center;
      justify-content: center;
      color: #fff;
      box-shadow: 0 4px 12px rgba(244, 63, 94, 0.25);
      flex-shrink: 0;
    }
    .brand-icon svg { width: 24px; height: 24px; }
    .brand-text { display: flex; flex-direction: column; }
    .brand-text .title {
      font-family: 'Outfit', sans-serif;
      font-weight: 800;
      font-size: 1.25rem;
      color: var(--text-main);
      line-height: 1.1;
      letter-spacing: -0.01em;
    }
    .brand-text .subtitle {
      font-size: 0.725rem;
      color: var(--text-muted);
      margin-top: 2px;
    }
    .sidebar-content {
      flex: 1;
      overflow-y: auto;
      padding: 1.25rem 0.85rem;
      display: flex;
      flex-direction: column;
      gap: 1.5rem;
    }
    .nav-section {
      display: flex;
      flex-direction: column;
      gap: 0.35rem;
    }
    .nav-section-title {
      font-size: 0.68rem;
      font-weight: 700;
      color: var(--text-subtle);
      text-transform: uppercase;
      letter-spacing: 0.08em;
      padding: 0 0.6rem 0.35rem;
    }
    .nav-menu {
      display: flex;
      flex-direction: column;
      gap: 0.3rem;
    }
    .nav-item {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      padding: 0.65rem 0.85rem;
      border-radius: 8px;
      color: var(--text-muted);
      font-size: 0.875rem;
      font-weight: 500;
      transition: all 0.2s ease;
    }
    .nav-item svg {
      color: var(--text-subtle);
      transition: color 0.2s ease;
      flex-shrink: 0;
    }
    .nav-item:hover {
      color: var(--text-main);
      background: var(--bg-surface-hover);
    }
    .nav-item:hover svg {
      color: var(--text-main);
    }
    .nav-item.active {
      color: var(--text-main);
      background: linear-gradient(90deg, rgba(244, 63, 94, 0.16) 0%, rgba(244, 63, 94, 0.04) 100%);
      font-weight: 600;
      border-left: 3px solid var(--primary);
    }
    .nav-item.active svg {
      color: var(--primary);
    }
    .sidebar-footer {
      padding: 1rem 1rem 1.25rem;
      border-top: 1px solid var(--border-color);
      background: rgba(18, 18, 21, 0.6);
    }
    .user-card {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      background: rgba(255, 255, 255, 0.04);
      border: 1px solid var(--border-color);
      padding: 0.65rem 0.85rem;
      border-radius: 10px;
    }
    .user-avatar {
      width: 36px;
      height: 36px;
      border-radius: 8px;
      background: var(--primary-subtle);
      border: 1px solid var(--border-pink);
      display: flex;
      align-items: center;
      justify-content: center;
      color: var(--primary);
      flex-shrink: 0;
    }
    .user-info {
      flex: 1;
      min-width: 0;
      display: flex;
      flex-direction: column;
    }
    .user-name {
      font-size: 0.825rem;
      font-weight: 600;
      color: var(--text-main);
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }
    .role-tag {
      font-size: 0.675rem;
      color: var(--primary);
      font-weight: 700;
      text-transform: uppercase;
      letter-spacing: 0.05em;
    }
    .btn-logout {
      background: transparent;
      border: 1px solid var(--border-color);
      color: var(--text-muted);
      padding: 0.4rem;
      border-radius: 6px;
      cursor: pointer;
      display: flex;
      align-items: center;
      justify-content: center;
      transition: all 0.2s ease;
      flex-shrink: 0;
    }
    .btn-logout:hover {
      color: #EF4444;
      border-color: #EF4444;
      background: rgba(239, 68, 68, 0.12);
    }
    .btn-block { width: 100%; }

    /* Mobile Styles */
    @media (max-width: 768px) {
      .mobile-topbar {
        display: flex;
        align-items: center;
        justify-content: space-between;
        padding: 0.75rem 1.25rem;
        background: rgba(28, 28, 33, 0.95);
        backdrop-filter: var(--glass-blur);
        border-bottom: 1px solid var(--border-color);
        position: fixed;
        top: 0;
        left: 0;
        right: 0;
        z-index: 999;
      }
      .mobile-topbar .brand-link {
        display: flex;
        align-items: center;
        gap: 0.65rem;
      }
      .mobile-topbar .brand-icon {
        width: 34px;
        height: 34px;
      }
      .mobile-topbar .brand-icon svg { width: 18px; height: 18px; }
      .mobile-topbar .title {
        font-family: 'Outfit', sans-serif;
        font-weight: 800;
        font-size: 1.15rem;
        color: var(--text-main);
      }
      .mobile-toggle-btn {
        background: transparent;
        border: 1px solid var(--border-color);
        color: var(--text-main);
        padding: 0.4rem;
        border-radius: 6px;
        cursor: pointer;
        display: flex;
        align-items: center;
      }
      .sidebar {
        position: fixed;
        top: 0;
        left: -280px;
        width: 270px;
        height: 100vh;
        z-index: 1000;
      }
      .sidebar.mobile-open {
        left: 0;
      }
      .sidebar-backdrop {
        position: fixed;
        top: 0;
        left: 0;
        right: 0;
        bottom: 0;
        background: rgba(18, 18, 21, 0.75);
        backdrop-filter: blur(4px);
        z-index: 998;
      }
    }
  `]
})
export class NavbarComponent {
  auth = inject(AuthService);
  isMobileMenuOpen = signal(false);

  toggleMobileMenu(): void {
    this.isMobileMenuOpen.update(v => !v);
  }

  closeMobileMenu(): void {
    this.isMobileMenuOpen.set(false);
  }
}
