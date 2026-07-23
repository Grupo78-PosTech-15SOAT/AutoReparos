import { Component, OnInit, OnDestroy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Subject, timer } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { OrdemServicoService } from '../../services/ordem-servico.service';
import { OrdemServico, StatusOS } from '../../models/ordem-servico.model';
import { OsCardComponent } from '../../components/os-card/os-card.component';
import { NotificationService } from '../../../../core/ui/notification.service';
import { PaginationComponent } from '../../../../shared/components/pagination/pagination.component';

@Component({
  selector: 'app-os-kanban-page',
  standalone: true,
  imports: [CommonModule, RouterLink, OsCardComponent, PaginationComponent],
  template: `
    <div class="container fade-in">
      <div class="page-header flex-between-responsive">
        <div>
          <h1 class="page-title">
            <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="#ED145B" stroke-width="2.3"><rect x="3" y="3" width="18" height="18" rx="2"/><path d="M9 3v18"/><path d="M15 3v18"/></svg>
            Fila Kanban
          </h1>
          @if (semComunicacao()) {
            <div class="sync-badge error-badge">
              <span class="error-dot"></span>
              <span>Sem comunicação</span>
            </div>
          } @else {
            <div class="sync-badge">
              <span class="pulse-dot"></span>
              <span>Ao vivo</span>
              @if (ultimaAtualizacao()) {
                <span style="color: #A1A1AA;">• {{ ultimaAtualizacao() }}</span>
              }
            </div>
          }
        </div>
        <div style="display: flex; gap: 0.75rem; flex-wrap: wrap; margin-top: 0.5rem;">
          @if (semComunicacao()) {
            <button (click)="tentarNovamente()" class="btn btn-retry">
              <svg xmlns="http://www.w3.org/2000/svg" width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><polyline points="23 4 23 10 17 10"/><path d="M20.49 15a9 9 0 1 1-2.12-9.36L23 10"/></svg>
              Tentar novamente
            </button>
          } @else {
            <button (click)="carregarFilaManualmente()" [disabled]="loading" class="btn btn-secondary">
              {{ loading ? '⏳ ...' : '🔄 Atualizar' }}
            </button>
          }
          <a routerLink="/ordens-servico/nova" class="btn btn-primary">
            + Nova OS
          </a>
        </div>
      </div>

      <!-- Grid de Colunas Kanban Responsivo -->
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

      <!-- Paginação Estruturada -->
      <app-pagination
        [pageNumber]="pageNumber"
        [pageSize]="pageSize"
        [totalItems]="totalItems"
        [totalPages]="totalPages"
        [pageSizeOptions]="[10, 20, 50, 100]"
        (pageChange)="onPageChange($event)"
        (pageSizeChange)="onPageSizeChange($event)">
      </app-pagination>
    </div>
  `,
  styles: [`
    .flex-between-responsive {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      flex-wrap: wrap;
      gap: 1rem;
    }
    .sync-badge {
      display: inline-flex;
      align-items: center;
      gap: 0.5rem;
      font-size: 0.75rem;
      color: #10B981;
      background: rgba(16, 185, 129, 0.1);
      border: 1px solid rgba(16, 185, 129, 0.25);
      padding: 0.25rem 0.65rem;
      border-radius: 999px;
      margin-top: 0.5rem;
      font-weight: 600;
    }
    .pulse-dot {
      width: 7px;
      height: 7px;
      background: #10B981;
      border-radius: 50%;
      box-shadow: 0 0 8px #10B981;
      animation: pulse 2s infinite;
    }
    @keyframes pulse {
      0% { transform: scale(0.95); box-shadow: 0 0 0 0 rgba(16, 185, 129, 0.7); }
      70% { transform: scale(1); box-shadow: 0 0 0 6px rgba(16, 185, 129, 0); }
      100% { transform: scale(0.95); box-shadow: 0 0 0 0 rgba(16, 185, 129, 0); }
    }
    .error-badge {
      color: #EF4444;
      background: rgba(239, 68, 68, 0.1);
      border-color: rgba(239, 68, 68, 0.3);
    }
    .error-dot {
      width: 7px;
      height: 7px;
      background: #EF4444;
      border-radius: 50%;
      box-shadow: 0 0 8px #EF4444;
      animation: pulse-error 2s infinite;
    }
    @keyframes pulse-error {
      0% { transform: scale(0.95); box-shadow: 0 0 0 0 rgba(239, 68, 68, 0.7); }
      70% { transform: scale(1); box-shadow: 0 0 0 6px rgba(239, 68, 68, 0); }
      100% { transform: scale(0.95); box-shadow: 0 0 0 0 rgba(239, 68, 68, 0); }
    }
    .btn-retry {
      display: inline-flex;
      align-items: center;
      gap: 0.5rem;
      padding: 0.5rem 1rem;
      font-size: 0.875rem;
      font-weight: 700;
      border-radius: 8px;
      border: 1px solid rgba(239, 68, 68, 0.5);
      background: rgba(239, 68, 68, 0.1);
      color: #EF4444;
      cursor: pointer;
      transition: all 0.2s ease;
    }
    .btn-retry:hover {
      background: rgba(239, 68, 68, 0.2);
      border-color: #EF4444;
    }
    .kanban-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(230px, 1fr));
      gap: 1.25rem;
      align-items: start;
    }
    @media (max-width: 768px) {
      .kanban-grid { grid-template-columns: 1fr; }
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
      min-height: 450px;
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
export class OsKanbanPageComponent implements OnInit, OnDestroy {
  recebidas: OrdemServico[] = [];
  diagnostico: OrdemServico[] = [];
  aguardando: OrdemServico[] = [];
  execucao: OrdemServico[] = [];
  finalizadas: OrdemServico[] = [];

  loading = false;
  semComunicacao = signal<boolean>(false);
  ultimaAtualizacao = signal<string>('');

  pageNumber = 1;
  pageSize = 50;
  totalItems = 0;
  totalPages = 1;

  private destroy$ = new Subject<void>();
  private osService = inject(OrdemServicoService);
  private notification = inject(NotificationService);

  ngOnInit() {
    this.iniciarPolling();
  }

  private iniciarPolling() {
    this.destroy$ = new Subject<void>();
    timer(0, 12000)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.carregarFila());
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  carregarFilaManualmente() {
    this.loading = true;
    this.carregarFila(() => {
      this.loading = false;
      this.notification.info('Fila Atualizada', 'Quadro Kanban atualizado.');
    });
  }

  tentarNovamente() {
    this.semComunicacao.set(false);
    this.iniciarPolling();
  }

  onPageChange(page: number) {
    this.pageNumber = page;
    this.carregarFila();
  }

  onPageSizeChange(size: number) {
    this.pageSize = size;
    this.pageNumber = 1;
    this.carregarFila();
  }

  private carregarFila(callback?: () => void) {
    this.osService.getFilaKanban(this.pageNumber, this.pageSize).subscribe({
      next: (res) => {
        this.semComunicacao.set(false);
        const lista = res.items || [];
        this.totalItems = res.total;
        this.totalPages = res.totalPages;
        this.recebidas = lista.filter(x => x.status === StatusOS.Recebida);
        this.diagnostico = lista.filter(x => x.status === StatusOS.EmDiagnostico);
        this.aguardando = lista.filter(x => x.status === StatusOS.AguardandoAprovacao);
        this.execucao = lista.filter(x => x.status === StatusOS.EmExecucao);
        this.finalizadas = lista.filter(x => x.status === StatusOS.Finalizada);

        const agora = new Date();
        this.ultimaAtualizacao.set(agora.toLocaleTimeString('pt-BR'));
        if (callback) callback();
      },
      error: () => {
        // Para o polling ao detectar falha de comunicação
        this.destroy$.next();
        this.semComunicacao.set(true);
        if (callback) callback();
      }
    });
  }
}
