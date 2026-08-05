import { Component, inject, ChangeDetectionStrategy } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../auth/services/auth.service';

@Component({
  selector: 'app-home-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ RouterLink],
  template: `
    <div class="home-container fade-in">
 
       <!-- Hero Header Premium -->
       <section class="hero-card">
         <div class="hero-glow-bg"></div>
         <div class="hero-content">
           <div class="brand-pill">
             <span class="pill-dot"></span> FIAP Tech Challenge — AutoReparos v1.0
           </div>
 
           <h1 class="hero-title">
             Plataforma de <span class="highlight-pink">Gestão Automotiva</span>
           </h1>
           <p class="hero-description">
             Gestão de ordens de serviço, estoque e orçamentos.
           </p>
 
           @if (auth.currentUser(); as user) {
             <div class="welcome-user-box">
               <div class="user-avatar-badge">
                 <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M19 21v-2a4 4 0 0 0-4-4H9a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/></svg>
               </div>
               <div class="user-greeting-text">
                 <span class="user-greeting-label">Sessão ativa</span>
                 <span class="user-greeting-name">{{ user.nome }} <small class="user-role-badge">({{ user.role }})</small></span>
               </div>
               <div class="user-actions">
                 <a routerLink="/ordens-servico/fila" class="btn btn-primary btn-md">
                   <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="3" width="18" height="18" rx="2"/><path d="M9 3v18"/><path d="M15 3v18"/></svg>
                   Quadro Kanban
                 </a>
                 @if (auth.hasRole(['Administrador'])) {
                   <a routerLink="/dashboard" class="btn btn-secondary btn-md">
                     <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="3" width="7" height="7"/><rect x="14" y="3" width="7" height="7"/><rect x="14" y="14" width="7" height="7"/><rect x="3" y="14" width="7" height="7"/></svg>
                     Dashboard Gerencial
                   </a>
                 }
               </div>
             </div>
           } @else {
             <div class="hero-cta-group">
               <a routerLink="/login" class="btn btn-primary btn-lg">
                 <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="11" width="18" height="11" rx="2" ry="2"/><path d="M7 11V7a5 5 0 0 1 10 0v4"/></svg>
                 Painel Interno da Oficina
               </a>
               <a routerLink="/consulta-publica" class="btn btn-secondary btn-lg">
                 <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>
                 Consulta Pública do Cliente
               </a>
             </div>
           }
         </div>
       </section>
 
       <!-- Grid de Módulos e Acessos Diretos (Mantido o Pin favorito do usuário) -->
       <section class="modules-section">
         <div class="section-header">
           <h2 class="section-title">
             <span class="pin-icon">📌</span> Portais e Módulos
           </h2>
         </div>
 
         <div class="modules-grid">
 
           <!-- Card 1: Portal Público -->
           <div class="module-card">
             <div class="card-top-line blue-line"></div>
             <div class="card-header">
               <div class="card-icon icon-blue">
                 <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>
               </div>
               <h3 class="module-title">Portal Público do Cliente</h3>
             </div>
             <a routerLink="/consulta-publica" class="module-link">
               <span>Acessar Consulta Pública</span>
               <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="5" y1="12" x2="19" y2="12"/><polyline points="12 5 19 12 12 19"/></svg>
             </a>
           </div>
 
           <!-- Card 2: Aprovação Digital -->
           <div class="module-card">
             <div class="card-top-line purple-line"></div>
             <div class="card-header">
               <div class="card-icon icon-purple">
                 <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/><line x1="16" y1="13" x2="8" y2="13"/><line x1="16" y1="17" x2="8" y2="17"/></svg>
               </div>
               <h3 class="module-title">Aprovação de Orçamento</h3>
             </div>
             <a routerLink="/aprovar-orcamento" class="module-link">
               <span>Simular Aprovação por Token</span>
               <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="5" y1="12" x2="19" y2="12"/><polyline points="12 5 19 12 12 19"/></svg>
             </a>
           </div>
 
           <!-- Card 3: Fila Kanban -->
           <div class="module-card">
             <div class="card-top-line orange-line"></div>
             <div class="card-header">
               <div class="card-icon icon-orange">
                 <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="3" width="18" height="18" rx="2"/><path d="M9 3v18"/><path d="M15 3v18"/></svg>
               </div>
               <h3 class="module-title">Fila Kanban da Oficina</h3>
             </div>
             <a routerLink="/ordens-servico/fila" class="module-link">
               <span>Abrir Quadro Kanban</span>
               <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="5" y1="12" x2="19" y2="12"/><polyline points="12 5 19 12 12 19"/></svg>
             </a>
           </div>
 
           <!-- Card 4: Clientes e Veículos -->
           <div class="module-card">
             <div class="card-top-line pink-line"></div>
             <div class="card-header">
               <div class="card-icon icon-pink">
                 <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/></svg>
               </div>
               <h3 class="module-title">Gestão de Clientes e Veículos</h3>
             </div>
             <a routerLink="/clientes" class="module-link">
               <span>Gerenciar Clientes</span>
               <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="5" y1="12" x2="19" y2="12"/><polyline points="12 5 19 12 12 19"/></svg>
             </a>
           </div>
 
           <!-- Card 5: Estoque e Insumos -->
           @if (auth.hasRole(['Administrador', 'Mecanico'])) {
             <div class="module-card">
               <div class="card-top-line emerald-line"></div>
               <div class="card-header">
                 <div class="card-icon icon-emerald">
                   <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z"/></svg>
                 </div>
                 <h3 class="module-title">Controle de Estoque e Peças</h3>
               </div>
               <a routerLink="/insumos" class="module-link">
                 <span>Ver Tabela de Insumos</span>
                 <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="5" y1="12" x2="19" y2="12"/><polyline points="12 5 19 12 12 19"/></svg>
               </a>
             </div>
           }
 
           <!-- Card 6: Dashboard -->
           @if (auth.hasRole(['Administrador'])) {
             <div class="module-card">
               <div class="card-top-line cyan-line"></div>
               <div class="card-header">
                 <div class="card-icon icon-cyan">
                   <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="3" width="7" height="7"/><rect x="14" y="3" width="7" height="7"/><rect x="14" y="14" width="7" height="7"/><rect x="3" y="14" width="7" height="7"/></svg>
                 </div>
                 <h3 class="module-title">Dashboard e Métricas</h3>
               </div>
               <a routerLink="/dashboard" class="module-link">
                 <span>Visualizar Métricas</span>
                 <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="5" y1="12" x2="19" y2="12"/><polyline points="12 5 19 12 12 19"/></svg>
               </a>
             </div>
           }
 
         </div>
       </section>
 
     </div>
  `,
  styles: [`
    .home-container {
      max-width: 1200px;
      margin: 0 auto;
      padding: 2rem 1.5rem 4rem;
    }

    /* Hero Card Style */
    .hero-card {
      background: var(--bg-surface);
      border: 1px solid var(--border-color);
      border-radius: 20px;
      padding: 3rem 2.75rem;
      position: relative;
      overflow: hidden;
      box-shadow: var(--shadow-md);
    }
    .hero-glow-bg {
      position: absolute;
      top: -120px;
      right: -100px;
      width: 320px;
      height: 320px;
      background: radial-gradient(circle, rgba(244, 63, 94, 0.12) 0%, transparent 70%);
      pointer-events: none;
    }
    .hero-content {
      position: relative;
      z-index: 2;
    }
    .brand-pill {
      display: inline-flex;
      align-items: center;
      gap: 0.6rem;
      background: var(--primary-subtle);
      border: 1px solid var(--border-pink);
      color: var(--primary);
      padding: 0.35rem 0.9rem;
      border-radius: 999px;
      font-size: 0.78rem;
      font-weight: 700;
      letter-spacing: 0.03em;
      margin-bottom: 1.2rem;
    }
    .pill-dot {
      width: 6px;
      height: 6px;
      background: var(--primary);
      border-radius: 50%;
      box-shadow: 0 0 6px var(--primary-glow);
    }
    .hero-title {
      font-family: 'Outfit', sans-serif;
      font-size: 2.6rem;
      font-weight: 800;
      color: var(--text-main);
      line-height: 1.15;
      letter-spacing: -0.02em;
    }
    .highlight-pink {
      color: var(--primary);
      background: linear-gradient(135deg, var(--primary), #FB7185);
      -webkit-background-clip: text;
      -webkit-text-fill-color: transparent;
    }
    .hero-description {
      color: var(--text-muted);
      font-size: 1.05rem;
      max-width: 680px;
      margin-top: 0.85rem;
      line-height: 1.6;
    }
    .hero-cta-group {
      display: flex;
      gap: 1rem;
      margin-top: 1.75rem;
      flex-wrap: wrap;
    }
    .btn-lg {
      padding: 0.75rem 1.5rem;
      font-size: 0.95rem;
      border-radius: 10px;
    }
    .btn-md {
      padding: 0.6rem 1.1rem;
      font-size: 0.88rem;
      border-radius: 8px;
    }

    .welcome-user-box {
      display: flex;
      align-items: center;
      gap: 1.25rem;
      background: var(--bg-card);
      border: 1px solid var(--border-color);
      border-radius: 12px;
      padding: 1rem 1.25rem;
      margin-top: 1.75rem;
      flex-wrap: wrap;
    }
    .user-avatar-badge {
      width: 40px;
      height: 40px;
      border-radius: 10px;
      background: var(--primary-subtle);
      color: var(--primary);
      display: flex;
      align-items: center;
      justify-content: center;
    }
    .user-greeting-text {
      display: flex;
      flex-direction: column;
    }
    .user-greeting-label {
      font-size: 0.75rem;
      color: var(--text-subtle);
      text-transform: uppercase;
      letter-spacing: 0.05em;
    }
    .user-greeting-name {
      font-size: 0.95rem;
      font-weight: 700;
      color: var(--text-main);
    }
    .user-role-badge {
      color: var(--primary);
      font-size: 0.8rem;
    }
    .user-actions {
      margin-left: auto;
      display: flex;
      gap: 0.75rem;
      flex-wrap: wrap;
    }

    /* Modules Section */
    .modules-section {
      margin-top: 3rem;
    }
    .section-header {
      margin-bottom: 1.5rem;
    }
    .section-title {
      font-family: 'Outfit', sans-serif;
      font-size: 1.45rem;
      font-weight: 800;
      color: var(--text-main);
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }
    .pin-icon {
      font-size: 1.2rem;
    }
    .section-subtitle {
      color: var(--text-subtle);
      font-size: 0.88rem;
      margin-top: 0.2rem;
    }

    .modules-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(310px, 1fr));
      gap: 1.25rem;
    }
    .module-card {
      background: var(--bg-card);
      backdrop-filter: var(--glass-blur);
      border: 1px solid var(--border-color);
      border-radius: 14px;
      padding: 1.5rem;
      position: relative;
      overflow: hidden;
      display: flex;
      flex-direction: column;
      justify-content: space-between;
      transition: all 0.22s ease;
      box-shadow: var(--shadow-sm);
    }
    .module-card:hover {
      transform: translateY(-3px);
      border-color: var(--border-focus);
      box-shadow: var(--shadow-md);
    }
    .card-top-line {
      position: absolute;
      top: 0; left: 0; right: 0;
      height: 3px;
      opacity: 0.8;
    }
    .blue-line { background: #38BDF8; }
    .purple-line { background: #A78BFA; }
    .orange-line { background: #FB923C; }
    .pink-line { background: var(--primary); }
    .emerald-line { background: #34D399; }
    .cyan-line { background: #06B6D4; }

    .card-header {
      display: flex;
      align-items: center;
      gap: 0.85rem;
      margin-bottom: 0.85rem;
    }
    .card-icon {
      width: 40px;
      height: 40px;
      border-radius: 10px;
      display: flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;
    }
    .icon-blue { background: rgba(56, 189, 248, 0.12); color: #38BDF8; }
    .icon-purple { background: rgba(167, 139, 250, 0.12); color: #A78BFA; }
    .icon-orange { background: rgba(251, 146, 60, 0.14); color: #FB923C; }
    .icon-pink { background: var(--primary-subtle); color: var(--primary); }
    .icon-emerald { background: rgba(52, 211, 153, 0.12); color: #34D399; }
    .icon-cyan { background: rgba(6, 182, 212, 0.12); color: #06B6D4; }

    .module-title {
      font-family: 'Outfit', sans-serif;
      font-size: 1.1rem;
      font-weight: 700;
      color: var(--text-main);
      line-height: 1.25;
    }
    .module-desc {
      color: var(--text-muted);
      font-size: 0.875rem;
      line-height: 1.5;
      margin-bottom: 1.25rem;
      flex-grow: 1;
    }
    .module-link {
      color: var(--primary);
      font-size: 0.85rem;
      font-weight: 700;
      display: inline-flex;
      align-items: center;
      gap: 0.4rem;
      transition: gap 0.2s ease, color 0.2s ease;
    }
    .module-link:hover {
      color: var(--primary-hover);
      gap: 0.6rem;
    }

    @media (max-width: 768px) {
      .hero-card { padding: 2rem 1.5rem; }
      .hero-title { font-size: 2rem; }
      .modules-grid { grid-template-columns: 1fr; }
      .welcome-user-box { flex-direction: column; align-items: flex-start; }
      .user-actions { margin-left: 0; width: 100%; }
    }
  `]
})
export class HomePageComponent {
  auth = inject(AuthService);
}
