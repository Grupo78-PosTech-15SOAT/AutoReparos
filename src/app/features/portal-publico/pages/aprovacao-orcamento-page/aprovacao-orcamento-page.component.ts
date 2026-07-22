import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { OrdemServicoService } from '../../../ordens-servico/services/ordem-servico.service';
import { NotificationService } from '../../../../core/ui/notification.service';

@Component({
  selector: 'app-aprovacao-orcamento-page',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container fade-in" style="max-width: 650px; padding-top: 3rem;">
      <div class="approval-card">
        <div class="brand-header">
          <div class="brand-icon">
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.3" stroke-linecap="round" stroke-linejoin="round">
              <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/><path d="m9 15 2 2 4-4"/>
            </svg>
          </div>
          <h1 class="card-title">Aprovação de Orçamento com 1-Clique</h1>
          <p class="card-subtitle">Confirmação de execução de serviços pela oficina AutoReparos.</p>
        </div>

        @if (!respondido) {
          <div class="token-info-box">
            <div class="info-row">
              <span class="label">Token Assinado:</span>
              <code class="token-code">{{ token }}</code>
            </div>
            <p class="token-desc">Ao aprovar o orçamento abaixo, o mecânico será notificado imediatamente para dar início aos serviços no veículo.</p>
          </div>

          <!-- Botões de Decisão -->
          <div class="decision-buttons">
            <button (click)="responder(false)" [disabled]="loading" class="btn btn-danger btn-lg" style="flex: 1;">
              ✕ Recusar Orçamento
            </button>
            <button (click)="responder(true)" [disabled]="loading" class="btn btn-success btn-lg" style="flex: 1.5;">
              ✓ Aprovar Orçamento & Iniciar Serviço
            </button>
          </div>
        } @else {
          <div class="success-box fade-in">
            <div class="check-circle" [style.background]="aprovado ? 'rgba(16, 185, 129, 0.2)' : 'rgba(239, 68, 68, 0.2)'">
              <span [style.color]="aprovado ? '#10B981' : '#EF4444'" style="font-size: 2.5rem; font-weight: 800;">
                {{ aprovado ? '✓' : '✕' }}
              </span>
            </div>
            <h2 style="font-family: 'Outfit', sans-serif; color: #fff; margin-top: 1rem;">
              {{ aprovado ? 'Orçamento Aprovado com Sucesso!' : 'Orçamento Recusado' }}
            </h2>
            <p style="color: #A1A1AA; font-size: 0.95rem; margin-top: 0.5rem;">
              {{ aprovado ? 'Sua aprovação foi registrada no sistema. A equipe da oficina já foi notificada!' : 'Sua resposta foi registrada. Entraremos em contato para mais informações.' }}
            </p>
          </div>
        }
      </div>
    </div>
  `,
  styles: [`
    .approval-card {
      background: rgba(24, 24, 28, 0.9);
      backdrop-filter: blur(16px);
      border: 1px solid rgba(237, 20, 91, 0.3);
      border-radius: 16px;
      padding: 3rem 2.5rem;
      box-shadow: 0 20px 40px rgba(0, 0, 0, 0.8);
    }
    .brand-header { text-align: center; margin-bottom: 2rem; }
    .brand-icon {
      width: 58px; height: 58px;
      background: linear-gradient(135deg, #ED145B, #800A30);
      border-radius: 12px;
      display: flex; align-items: center; justify-content: center;
      color: #ffffff; margin: 0 auto 1rem;
      box-shadow: 0 0 25px rgba(237, 20, 91, 0.4);
    }
    .brand-icon svg { width: 32px; height: 32px; }
    .card-title { font-family: 'Outfit', sans-serif; font-size: 1.8rem; font-weight: 800; color: #ffffff; }
    .card-subtitle { font-size: 0.875rem; color: #A1A1AA; margin-top: 0.25rem; }

    .token-info-box {
      background: rgba(10, 10, 12, 0.7);
      border: 1px solid rgba(255, 255, 255, 0.08);
      border-radius: 10px;
      padding: 1.25rem;
      margin-bottom: 2rem;
    }
    .info-row { display: flex; flex-direction: column; gap: 0.3rem; margin-bottom: 0.75rem; }
    .label { font-size: 0.75rem; color: #71717A; text-transform: uppercase; letter-spacing: 0.05em; font-weight: 600; }
    .token-code { font-family: 'JetBrains Mono', monospace; font-size: 0.8rem; color: #ED145B; word-break: break-all; }
    .token-desc { font-size: 0.85rem; color: #A1A1AA; line-height: 1.5; }

    .decision-buttons { display: flex; gap: 1rem; flex-wrap: wrap; }
    .btn-lg { padding: 1rem 1.5rem; font-size: 1rem; }

    .success-box { text-align: center; padding: 2rem 1rem; }
    .check-circle { width: 80px; height: 80px; border-radius: 50%; display: flex; align-items: center; justify-content: center; margin: 0 auto; }
  `]
})
export class AprovacaoOrcamentoPageComponent implements OnInit {
  token = '';
  loading = false;
  respondido = false;
  aprovado = false;

  private route = inject(ActivatedRoute);
  private osService = inject(OrdemServicoService);
  private notification = inject(NotificationService);

  ngOnInit() {
    this.token = this.route.snapshot.queryParamMap.get('token') || '';
  }

  responder(aprovado: boolean) {
    if (!this.token) {
      this.notification.error('Token Inválido', 'Token de aprovação não fornecido na URL.');
      return;
    }

    this.loading = true;
    this.osService.responderOrcamentoToken(this.token, aprovado).subscribe({
      next: () => {
        this.loading = false;
        this.respondido = true;
        this.aprovado = aprovado;
        this.notification.success('Resposta Registrada', `Orçamento ${aprovado ? 'Aprovado' : 'Recusado'}.`);
      },
      error: () => {
        this.loading = false;
      }
    });
  }
}
