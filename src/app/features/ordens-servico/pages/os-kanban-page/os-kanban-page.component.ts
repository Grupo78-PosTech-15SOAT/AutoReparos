import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { OrdemServicoService } from '../../services/ordem-servico.service';
import { OrdemServico, StatusOS } from '../../models/ordem-servico.model';
import { OsCardComponent } from '../../components/os-card/os-card.component';
import { NotificationService } from '../../../../core/ui/notification.service';

@Component({
  selector: 'app-os-kanban-page',
  standalone: true,
  imports: [CommonModule, RouterLink, OsCardComponent],
  template: `
    <div class="container fade-in">
      <div class="page-header">
        <div>
          <h1 class="page-title">
            <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="#ED145B" stroke-width="2.3"><rect x="3" y="3" width="18" height="18" rx="2"/><path d="M9 3v18"/><path d="M15 3v18"/></svg>
            Fila Kanban da Oficina
          </h1>
          <p class="page-subtitle">Acompanhamento e priorização de ordens de serviço em tempo real.</p>
        </div>
        <div style="display: flex; gap: 0.75rem;">
          <button (click)="carregarFila()" class="btn btn-secondary">
            🔄 Atualizar Fila
          </button>
          <a routerLink="/ordens-servico/nova" class="btn btn-primary">
            + Nova OS
          </a>
        </div>
      </div>

      <!-- Grid de Colunas Kanban -->
      <div class="kanban-grid">
        <!-- Coluna 1: Recebidas -->
        <div class="kanban-column">
          <div class="column-header status-recebida-border">
            <span class="column-title">1. Recebidas</span>
            <span class="column-count">{{ recebidas.length }}</span>
          </div>
          <div class="column-cards">
            @for (os of recebidas; track os.id) {
              <app-os-card [os]="os"></app-os-card>
            } @empty {
              <div class="empty-column">Nenhuma OS nesta etapa</div>
            }
          </div>
        </div>

        <!-- Coluna 2: Em Diagnóstico -->
        <div class="kanban-column">
          <div class="column-header status-diagnostico-border">
            <span class="column-title">2. Em Diagnóstico</span>
            <span class="column-count">{{ diagnostico.length }}</span>
          </div>
          <div class="column-cards">
            @for (os of diagnostico; track os.id) {
              <app-os-card [os]="os"></app-os-card>
            } @empty {
              <div class="empty-column">Nenhuma OS nesta etapa</div>
            }
          </div>
        </div>

        <!-- Coluna 3: Aguardando Aprovação -->
        <div class="kanban-column">
          <div class="column-header status-aguardando-border">
            <span class="column-title">3. Aguardando Aprovação</span>
            <span class="column-count">{{ aguardando.length }}</span>
          </div>
          <div class="column-cards">
            @for (os of aguardando; track os.id) {
              <app-os-card [os]="os"></app-os-card>
            } @empty {
              <div class="empty-column">Nenhuma OS nesta etapa</div>
            }
          </div>
        </div>

        <!-- Coluna 4: Em Execução (Prioridade Alta) -->
        <div class="kanban-column active-column">
          <div class="column-header status-execucao-border">
            <span class="column-title">⚡ 4. Em Execução</span>
            <span class="column-count active-count">{{ execucao.length }}</span>
          </div>
          <div class="column-cards">
            @for (os of execucao; track os.id) {
              <app-os-card [os]="os"></app-os-card>
            } @empty {
              <div class="empty-column">Nenhuma OS nesta etapa</div>
            }
          </div>
        </div>

        <!-- Coluna 5: Finalizadas -->
        <div class="kanban-column">
          <div class="column-header status-finalizada-border">
            <span class="column-title">5. Finalizadas</span>
            <span class="column-count">{{ finalizadas.length }}</span>
          </div>
          <div class="column-cards">
            @for (os of finalizadas; track os.id) {
              <app-os-card [os]="os"></app-os-card>
            } @empty {
              <div class="empty-column">Nenhuma OS nesta etapa</div>
            }
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .kanban-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
      gap: 1.25rem;
      align-items: start;
    }
    .kanban-column {
      background: rgba(24, 24, 28, 0.7);
      backdrop-filter: blur(10px);
      border: 1px solid rgba(255, 255, 255, 0.08);
      border-radius: 12px;
      padding: 1rem;
      display: flex;
      flex-direction: column;
      gap: 1rem;
      min-height: 500px;
    }
    .active-column {
      border-color: rgba(249, 115, 22, 0.4);
      background: rgba(249, 115, 22, 0.04);
    }
    .column-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding-bottom: 0.75rem;
      border-bottom: 2px solid #3B82F6;
    }
    .status-recebida-border { border-bottom-color: #3B82F6; }
    .status-diagnostico-border { border-bottom-color: #F59E0B; }
    .status-aguardando-border { border-bottom-color: #8B5CF6; }
    .status-execucao-border { border-bottom-color: #F97316; }
    .status-finalizada-border { border-bottom-color: #10B981; }

    .column-title {
      font-family: 'Outfit', sans-serif;
      font-weight: 700;
      font-size: 0.9rem;
      color: #F8FAFC;
    }
    .column-count {
      background: rgba(255, 255, 255, 0.1);
      color: #A1A1AA;
      padding: 0.15rem 0.6rem;
      border-radius: 999px;
      font-size: 0.75rem;
      font-weight: 700;
    }
    .active-count {
      background: rgba(249, 115, 22, 0.2);
      color: #F97316;
    }
    .column-cards {
      display: flex;
      flex-direction: column;
      gap: 1rem;
    }
    .empty-column {
      text-align: center;
      padding: 2rem 1rem;
      font-size: 0.8rem;
      color: #71717A;
      border: 1px dashed rgba(255, 255, 255, 0.1);
      border-radius: 8px;
    }
  `]
})
export class OsKanbanPageComponent implements OnInit {
  recebidas: OrdemServico[] = [];
  diagnostico: OrdemServico[] = [];
  aguardando: OrdemServico[] = [];
  execucao: OrdemServico[] = [];
  finalizadas: OrdemServico[] = [];

  private osService = inject(OrdemServicoService);
  private notification = inject(NotificationService);

  ngOnInit() {
    this.carregarFila();
  }

  carregarFila() {
    this.osService.getFilaKanban().subscribe({
      next: (lista) => {
        this.recebidas = lista.filter(x => x.status === StatusOS.Recebida);
        this.diagnostico = lista.filter(x => x.status === StatusOS.EmDiagnostico);
        this.aguardando = lista.filter(x => x.status === StatusOS.AguardandoAprovacao);
        this.execucao = lista.filter(x => x.status === StatusOS.EmExecucao);
        this.finalizadas = lista.filter(x => x.status === StatusOS.Finalizada);
      },
      error: () => {
        // Tratinho amigável handled by ErrorInterceptor
      }
    });
  }
}
