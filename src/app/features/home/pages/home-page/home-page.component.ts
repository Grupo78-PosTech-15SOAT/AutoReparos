import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../auth/services/auth.service';

@Component({
  selector: 'app-home-page',
  standalone: true,
  imports: [CommonModule, RouterLink],
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
             Plataforma Integrada de <span class="highlight-pink">Gestão Automotiva</span>
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
 
       <!-- Dev Mode Card Elegant Redesign -->
       @if (!auth.currentUser()) {
         <section class="dev-mode-card">
           <div class="dev-header">
             <div class="dev-status-badge">
               <span class="pulse-dot"></span> Ambientação Local e Testes
             </div>
             <span class="dev-tag">Ambiente de Desenvolvimento</span>
           </div>
 
           <div class="dev-body">
             <div class="dev-cred-item">
               <span class="cred-label">E-mail de Administrador</span>
               <div class="cred-values">
                 <div class="cred-chip">
                   <span class="chip-key">Email:</span>
                   <code class="chip-val">admin&#64;autoreparos.com</code>
                 </div>
               </div>
             </div>
 
             <div class="dev-action-area">
               <a routerLink="/login" class="btn btn-accent btn-sm">
                 <svg xmlns="http://www.w3.org/2000/svg" width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M15 3h4a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2h-4"/><polyline points="10 17 15 12 10 7"/><line x1="15" y1="12" x2="3" y2="12"/></svg>
                 Ir para Login
               </a>
             </div>
           </div>
         </section>
       }
 
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
 
           <!-- Card 6: Dashboard -->
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
      background: linear-gradient(135deg, rgba(20, 20, 26, 0.95), rgba(12, 12, 16, 0.98));
      border: 1px solid rgba(237, 20, 91, 0.25);
      border-radius: 20px;
      padding: 3rem 2.75rem;
      position: relative;
      overflow: hidden;
      box-shadow: 0 16px 40px rgba(0, 0, 0, 0.6);
    }
    .hero-glow-bg {
      position: absolute;
      top: -120px;
      right: -100px;
      width: 320px;
      height: 320px;
      background: radial-gradient(circle, rgba(237, 20, 91, 0.18) 0%, transparent 70%);
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
      background: rgba(237, 20, 91, 0.1);
      border: 1px solid rgba(237, 20, 91, 0.3);
      color: #ED145B;
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
      background: #ED145B;
      border-radius: 50%;
      box-shadow: 0 0 6px #ED145B;
    }
    .hero-title {
      font-family: 'Outfit', sans-serif;
      font-size: 2.6rem;
      font-weight: 800;
      color: #ffffff;
      line-height: 1.15;
      letter-spacing: -0.02em;
    }
    .highlight-pink {
      color: #ED145B;
      background: linear-gradient(135deg, #ED145B, #FF5388);
      -webkit-background-clip: text;
      -webkit-text-fill-color: transparent;
    }
    .hero-description {
      color: #A1A1AA;
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
      background: rgba(14, 14, 18, 0.8);
      border: 1px solid rgba(255, 255, 255, 0.08);
      border-radius: 12px;
      padding: 1rem 1.25rem;
      margin-top: 1.75rem;
      flex-wrap: wrap;
    }
    .user-avatar-badge {
      width: 40px;
      height: 40px;
      border-radius: 10px;
      background: rgba(237, 20, 91, 0.15);
      color: #ED145B;
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
      color: #71717A;
      text-transform: uppercase;
      letter-spacing: 0.05em;
    }
    .user-greeting-name {
      font-size: 0.95rem;
      font-weight: 700;
      color: #F8FAFC;
    }
    .user-role-badge {
      color: #ED145B;
      font-size: 0.8rem;
    }
    .user-actions {
      margin-left: auto;
      display: flex;
      gap: 0.75rem;
      flex-wrap: wrap;
    }

    /* Dev Mode Card Sleek Style */
    .dev-mode-card {
      background: rgba(18, 18, 22, 0.85);
      backdrop-filter: blur(12px);
      border: 1px solid rgba(245, 158, 11, 0.25);
      border-radius: 14px;
      padding: 1.25rem 1.5rem;
      margin-top: 1.75rem;
      box-shadow: 0 8px 24px rgba(0, 0, 0, 0.3);
    }
    .dev-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 1rem;
      flex-wrap: wrap;
      gap: 0.5rem;
    }
    .dev-status-badge {
      display: inline-flex;
      align-items: center;
      gap: 0.5rem;
      color: #F59E0B;
      font-size: 0.8rem;
      font-weight: 700;
      letter-spacing: 0.03em;
    }
    .pulse-dot {
      width: 7px;
      height: 7px;
      background: #F59E0B;
      border-radius: 50%;
      box-shadow: 0 0 8px #F59E0B;
    }
    .dev-tag {
      font-size: 0.72rem;
      color: #71717A;
      text-transform: uppercase;
      letter-spacing: 0.05em;
    }
    .dev-body {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 1.5rem;
      flex-wrap: wrap;
      background: rgba(10, 10, 12, 0.6);
      padding: 0.85rem 1.15rem;
      border-radius: 10px;
      border: 1px solid rgba(255, 255, 255, 0.05);
    }
    .cred-label {
      font-size: 0.78rem;
      color: #A1A1AA;
      font-weight: 600;
      display: block;
      margin-bottom: 0.4rem;
    }
    .cred-values {
      display: flex;
      gap: 1rem;
      flex-wrap: wrap;
    }
    .cred-chip {
      font-size: 0.82rem;
      background: rgba(255, 255, 255, 0.04);
      padding: 0.3rem 0.65rem;
      border-radius: 6px;
      border: 1px solid rgba(255, 255, 255, 0.08);
      display: inline-flex;
      align-items: center;
      gap: 0.4rem;
    }
    .chip-key { color: #71717A; }
    .chip-val { font-family: 'JetBrains Mono', monospace; color: #F8FAFC; font-weight: 600; }

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
      color: #ffffff;
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }
    .pin-icon {
      font-size: 1.2rem;
    }
    .section-subtitle {
      color: #71717A;
      font-size: 0.88rem;
      margin-top: 0.2rem;
    }

    .modules-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(310px, 1fr));
      gap: 1.25rem;
    }
    .module-card {
      background: rgba(20, 20, 24, 0.75);
      backdrop-filter: blur(12px);
      border: 1px solid rgba(255, 255, 255, 0.07);
      border-radius: 14px;
      padding: 1.5rem;
      position: relative;
      overflow: hidden;
      display: flex;
      flex-direction: column;
      justify-content: space-between;
      transition: all 0.22s ease;
    }
    .module-card:hover {
      transform: translateY(-3px);
      border-color: rgba(237, 20, 91, 0.3);
      box-shadow: 0 10px 25px rgba(0, 0, 0, 0.5), 0 0 15px rgba(237, 20, 91, 0.1);
    }
    .card-top-line {
      position: absolute;
      top: 0; left: 0; right: 0;
      height: 3px;
      opacity: 0.8;
    }
    .blue-line { background: #3B82F6; }
    .purple-line { background: #8B5CF6; }
    .orange-line { background: #F97316; }
    .pink-line { background: #ED145B; }
    .emerald-line { background: #10B981; }
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
    .icon-blue { background: rgba(59, 130, 246, 0.12); color: #3B82F6; }
    .icon-purple { background: rgba(139, 92, 246, 0.12); color: #8B5CF6; }
    .icon-orange { background: rgba(249, 115, 22, 0.12); color: #F97316; }
    .icon-pink { background: rgba(237, 20, 91, 0.12); color: #ED145B; }
    .icon-emerald { background: rgba(16, 185, 129, 0.12); color: #10B981; }
    .icon-cyan { background: rgba(6, 182, 212, 0.12); color: #06B6D4; }

    .module-title {
      font-family: 'Outfit', sans-serif;
      font-size: 1.1rem;
      font-weight: 700;
      color: #ffffff;
      line-height: 1.25;
    }
    .module-desc {
      color: #A1A1AA;
      font-size: 0.875rem;
      line-height: 1.5;
      margin-bottom: 1.25rem;
      flex-grow: 1;
    }
    .module-link {
      color: #ED145B;
      font-size: 0.85rem;
      font-weight: 700;
      display: inline-flex;
      align-items: center;
      gap: 0.4rem;
      transition: gap 0.2s ease, color 0.2s ease;
    }
    .module-link:hover {
      color: #FF5388;
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
