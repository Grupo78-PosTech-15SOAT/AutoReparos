import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  ElementRef,
  OnInit,
  inject,
  signal,
  viewChild
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { timer } from 'rxjs';
import { NotificationService } from '../../../../core/ui/notification.service';
import { PageContainerComponent } from '../../../../shared/components/page-container/page-container.component';
import { ApprovalKanbanCardComponent } from '../../components/os-card/approval-kanban-card.component';
import { DiagnosisKanbanCardComponent } from '../../components/os-card/diagnosis-kanban-card.component';
import { ExecutionKanbanCardComponent } from '../../components/os-card/execution-kanban-card.component';
import { FinishedKanbanCardComponent } from '../../components/os-card/finished-kanban-card.component';
import { ReceivedKanbanCardComponent } from '../../components/os-card/received-kanban-card.component';
import { OsDrawerComponent } from '../../components/os-drawer/os-drawer.component';
import { KanbanCard, KanbanColumn } from '../../models/ordem-servico.model';
import { OrdemServicoService } from '../../services/ordem-servico.service';

@Component({
  selector: 'app-os-kanban-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    
    RouterLink,
    ReceivedKanbanCardComponent,
    DiagnosisKanbanCardComponent,
    ApprovalKanbanCardComponent,
    ExecutionKanbanCardComponent,
    FinishedKanbanCardComponent,
    PageContainerComponent,
    OsDrawerComponent,
  ],
  template: `
    <app-page-container type="kanban">
      <div class="page-header" style="margin-bottom: 1.25rem;">
        <div>
          <h1 class="page-title" style="display: flex; align-items: center; gap: 0.6rem;">
            <svg
              xmlns="http://www.w3.org/2000/svg"
              width="28"
              height="28"
              viewBox="0 0 24 24"
              fill="none"
              stroke="#ED145B"
              stroke-width="2.3"
            >
              <rect x="3" y="3" width="18" height="18" rx="2" />
              <path d="M9 3v18" />
              <path d="M15 3v18" />
            </svg>
            Fila Kanban
          </h1>
          @switch (statusConexao()) {
            @case ('conectando') {
              <div class="sync-badge connecting-badge">
                <span class="connecting-dot"></span>
                <span>Conectando...</span>
              </div>
            }
            @case ('ao-vivo') {
              <div class="sync-badge">
                <span class="pulse-dot"></span>
                <span>Ao vivo</span>
                @if (ultimaAtualizacao()) {
                  <span style="color: #A1A1AA;">• {{ ultimaAtualizacao() }}</span>
                }
              </div>
            }
            @case ('sem-comunicacao') {
              <div class="sync-badge error-badge">
                <span class="error-dot"></span>
                <span>Sem comunicação</span>
              </div>
            }
          }
        </div>
        <div style="display: flex; gap: 0.75rem; flex-wrap: wrap;">
          @if (statusConexao() === 'sem-comunicacao') {
            <button (click)="tentarNovamente()" class="btn btn-retry">
              <svg
                xmlns="http://www.w3.org/2000/svg"
                width="15"
                height="15"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                stroke-width="2.5"
              >
                <polyline points="23 4 23 10 17 10" />
                <path d="M20.49 15a9 9 0 1 1-2.12-9.36L23 10" />
              </svg>
              Tentar novamente
            </button>
          }
          <a routerLink="/ordens-servico/nova" class="btn btn-primary"> + Nova OS </a>
        </div>
      </div>

      <!-- Kanban Board Layout -->
      @if (statusConexao() === 'sem-comunicacao') {
        <div class="comunicacao-erro-container">
          <div class="erro-icon-container">
            <svg
              xmlns="http://www.w3.org/2000/svg"
              width="48"
              height="48"
              viewBox="0 0 24 24"
              fill="none"
              stroke="#EF4444"
              stroke-width="2.5"
              stroke-linecap="round"
              stroke-linejoin="round"
            >
              <path d="m21.73 18-8-14a2 2 0 0 0-3.48 0l-8 14A2 2 0 0 0 4 21h16a2 2 0 0 0 1.73-3Z" />
              <line x1="12" y1="9" x2="12" y2="13" />
              <line x1="12" y1="17" x2="12.01" y2="17" />
            </svg>
          </div>
          <h2 class="erro-titulo">Sem Comunicação com o Servidor</h2>
          <p class="erro-subtitulo">
            Não foi possível carregar as ordens de serviço. Verifique a conexão com a API do
            sistema.
          </p>
          <button (click)="tentarNovamente()" class="btn btn-retry btn-large">
            <svg
              xmlns="http://www.w3.org/2000/svg"
              width="16"
              height="16"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              stroke-width="2.5"
              style="margin-right: 8px;"
            >
              <polyline points="23 4 23 10 17 10" />
              <path d="M20.49 15a9 9 0 1 1-2.12-9.36L23 10" />
            </svg>
            Tentar Conectar Novamente
          </button>
        </div>
      } @else if (statusConexao() === 'conectando') {
        <div class="comunicacao-loading-container">
          <div class="large-spinner"></div>
          <span class="loading-text">Conectando ao painel...</span>
        </div>
      } @else {
        <div
          class="kanban-wrapper"
          #kanbanWrapper
          (scroll)="onScroll()"
          [class.at-start]="isAtStart()"
          [class.at-end]="isAtEnd()"
          [class.no-scroll]="statusConexao() !== 'ao-vivo'"
        >
          <div class="kanban-board table-loading-container">
            @if (loading()) {
              <div class="table-loading-overlay">
                <div class="table-loading-spinner"></div>
                <span class="table-loading-text">Carregando fila...</span>
              </div>
            }

            @for (col of colunas(); track col.status) {
              <div class="kanban-column" [class.active-column]="col.status === 'Execucao'">
                <div
                  class="column-header"
                  [class]="getColumnBorderClass(col.status) + ' ' + getColumnGlowClass(col.status)"
                >
                  <span class="column-title">
                    {{ getColumnTitle(col.status) }}
                  </span>
                  <span class="column-count" [class.active-count]="col.status === 'Execucao'">
                    {{ col.cards.length }}
                  </span>
                </div>

                <div class="column-cards">
                  @for (card of col.cards; track card.id) {
                    @switch (card.$type) {
                      @case ('Received') {
                        <app-received-kanban-card
                          [card]="card"
                          (cardClick)="abrirDrawer($event)"
                        ></app-received-kanban-card>
                      }
                      @case ('Diagnosis') {
                        <app-diagnosis-kanban-card
                          [card]="card"
                          (cardClick)="abrirDrawer($event)"
                        ></app-diagnosis-kanban-card>
                      }
                      @case ('Approval') {
                        <app-approval-kanban-card
                          [card]="card"
                          (cardClick)="abrirDrawer($event)"
                        ></app-approval-kanban-card>
                      }
                      @case ('Execution') {
                        <app-execution-kanban-card
                          [card]="card"
                          (cardClick)="abrirDrawer($event)"
                        ></app-execution-kanban-card>
                      }
                      @case ('Finished') {
                        <app-finished-kanban-card
                          [card]="card"
                          (cardClick)="abrirDrawer($event)"
                        ></app-finished-kanban-card>
                      }
                    }
                  } @empty {
                    <div class="empty-column">Nenhuma OS nesta etapa</div>
                  }
                </div>
              </div>
            }
          </div>
        </div>
      }

      <app-os-drawer
        [isOpen]="drawerOpen()"
        [osId]="drawerOsId()"
        [cardPreview]="drawerCard()"
        (closed)="fecharDrawer()"
      >
      </app-os-drawer>
    </app-page-container>
  `,
  styles: [
    `
      .sync-badge {
        display: inline-flex;
        align-items: center;
        gap: 0.5rem;
        font-size: 0.75rem;
        color: #10b981;
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
        background: #10b981;
        border-radius: 50%;
        box-shadow: 0 0 8px #10b981;
        animation: pulse 2s infinite;
      }
      @keyframes pulse {
        0% {
          transform: scale(0.95);
          box-shadow: 0 0 0 0 rgba(16, 185, 129, 0.7);
        }
        70% {
          transform: scale(1);
          box-shadow: 0 0 0 6px rgba(16, 185, 129, 0);
        }
        100% {
          transform: scale(0.95);
          box-shadow: 0 0 0 0 rgba(16, 185, 129, 0);
        }
      }
      .error-badge {
        color: #ef4444;
        background: rgba(239, 68, 68, 0.1);
        border-color: rgba(239, 68, 68, 0.3);
      }
      .error-dot {
        width: 7px;
        height: 7px;
        background: #ef4444;
        border-radius: 50%;
        box-shadow: 0 0 8px #ef4444;
        animation: pulse-error 2s infinite;
      }
      @keyframes pulse-error {
        0% {
          transform: scale(0.95);
          box-shadow: 0 0 0 0 rgba(239, 68, 68, 0.7);
        }
        70% {
          transform: scale(1);
          box-shadow: 0 0 0 6px rgba(239, 68, 68, 0);
        }
        100% {
          transform: scale(0.95);
          box-shadow: 0 0 0 0 rgba(239, 68, 68, 0);
        }
      }
      .connecting-badge {
        color: #f59e0b;
        background: rgba(245, 158, 11, 0.1);
        border-color: rgba(245, 158, 11, 0.3);
      }
      .connecting-dot {
        width: 7px;
        height: 7px;
        background: #f59e0b;
        border-radius: 50%;
        box-shadow: 0 0 8px #f59e0b;
        animation: pulse-connecting 2s infinite;
      }
      @keyframes pulse-connecting {
        0% {
          transform: scale(0.95);
          box-shadow: 0 0 0 0 rgba(245, 158, 11, 0.7);
        }
        70% {
          transform: scale(1);
          box-shadow: 0 0 0 6px rgba(245, 158, 11, 0);
        }
        100% {
          transform: scale(0.95);
          box-shadow: 0 0 0 0 rgba(245, 158, 11, 0);
        }
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
        color: #ef4444;
        cursor: pointer;
        transition: all 0.2s ease;
      }
      .btn-retry:hover {
        background: rgba(239, 68, 68, 0.2);
        border-color: #ef4444;
      }

      .kanban-wrapper {
        flex: 1;
        min-height: 0;
        width: 100%;
        overflow-x: auto;
        overflow-y: hidden;
        display: flex;
        flex-direction: column;
        padding-bottom: 0.5rem;
        
        --mask-left: transparent;
        --mask-right: transparent;

        mask-image:
          linear-gradient(to right, var(--mask-left), #000 24px, #000 calc(100% - 24px), var(--mask-right)),
          linear-gradient(to top, #000, #000);
        mask-size:
          100% calc(100% - 16px),
          100% 16px;
        mask-position:
          top left,
          bottom left;
        mask-repeat: no-repeat, no-repeat;
        
        -webkit-mask-image:
          linear-gradient(to right, var(--mask-left), #000 24px, #000 calc(100% - 24px), var(--mask-right)),
          linear-gradient(to top, #000, #000);
        -webkit-mask-size:
          100% calc(100% - 16px),
          100% 16px;
        -webkit-mask-position:
          top left,
          bottom left;
        -webkit-mask-repeat: no-repeat, no-repeat;
      }
      .kanban-wrapper.at-start {
        --mask-left: #000;
      }
      .kanban-wrapper.at-end {
        --mask-right: #000;
      }
      .kanban-wrapper::-webkit-scrollbar {
        height: 6px;
      }
      .kanban-wrapper::-webkit-scrollbar-track {
        background: transparent;
      }
      .kanban-wrapper::-webkit-scrollbar-thumb {
        background: rgba(255, 255, 255, 0.15);
        border-radius: 3px;
      }
      .kanban-wrapper::-webkit-scrollbar-thumb:hover {
        background: rgba(255, 255, 255, 0.3);
      }

      .kanban-board {
        display: grid;
        grid-template-columns: repeat(5, minmax(290px, 1fr));
        gap: 1.25rem;
        flex: 1;
        min-height: 0;
        width: 100%;
        min-width: 1550px;
      }

      .kanban-column {
        display: flex;
        flex-direction: column;
        background: rgba(24, 24, 28, 0.6);
        backdrop-filter: blur(16px);
        border: 1px solid rgba(255, 255, 255, 0.05);
        border-radius: 12px;
        padding: 1rem;
        height: 100%;
        min-height: 0;
        box-sizing: border-box;
        overflow: hidden;
        transition:
          border-color 0.2s ease,
          background-color 0.2s ease;
      }
      .active-column {
        border-color: rgba(249, 115, 22, 0.3);
        background: rgba(249, 115, 22, 0.02);
      }

      .column-header {
        background: transparent;
        display: flex;
        justify-content: space-between;
        align-items: center;
        padding: 0.25rem 0.25rem 0.75rem 0.25rem;
        border-bottom: 2px solid #3b82f6;
        margin-bottom: 1rem;
        box-sizing: border-box;
        transition:
          border-color 0.3s ease,
          box-shadow 0.3s ease;
        position: relative;
        z-index: 0;
      }
      .status-recebida-border {
        border-bottom-color: #3b82f6;
      }
      .status-diagnostico-border {
        border-bottom-color: #f59e0b;
      }
      .status-aguardando-border {
        border-bottom-color: #8b5cf6;
      }
      .status-execucao-border {
        border-bottom-color: #f97316;
      }
      .status-finalizada-border {
        border-bottom-color: #10b981;
      }
      .no-scroll {
        overflow-x: hidden !important;
      }
      .column-header.glow-recebida,
      .column-header.glow-diagnostico,
      .column-header.glow-aguardando,
      .column-header.glow-execucao,
      .column-header.glow-finalizada {
        background: rgba(255, 255, 255, 0.05);
        border-radius: 4px;
      }
      .column-header.glow-recebida {
        box-shadow: 0 0 10px rgba(59, 130, 246, 0.35);
      }
      .column-header.glow-diagnostico {
        box-shadow: 0 0 10px rgba(245, 158, 11, 0.35);
      }
      .column-header.glow-aguardando {
        box-shadow: 0 0 10px rgba(139, 92, 246, 0.35);
      }
      .column-header.glow-execucao {
        box-shadow: 0 0 10px rgba(249, 115, 22, 0.4);
      }
      .column-header.glow-finalizada {
        box-shadow: 0 0 10px rgba(16, 185, 129, 0.35);
      }

      .column-title {
        font-family: 'Outfit', sans-serif;
        font-weight: 700;
        font-size: 0.92rem;
        color: #f8fafc;
        transform-origin: left center;
        display: inline-block;
        transition: transform 0.3s cubic-bezier(0.16, 1, 0.3, 1);
      }
      .column-count {
        background: rgba(255, 255, 255, 0.08);
        color: #a1a1aa;
        padding: 0.15rem 0.55rem;
        border-radius: 999px;
        font-size: 0.72rem;
        font-weight: 700;
      }
      .active-count {
        background: rgba(249, 115, 22, 0.15);
        color: #f97316;
      }

      .column-cards {
        display: flex;
        flex-direction: column;
        gap: 0.85rem;
        overflow-y: auto;
        flex: 1;
        padding-top: 0.5rem;
        padding-right: 0.25rem;
        min-height: 0;
        position: relative;
        z-index: 1;
      }
      .column-cards::-webkit-scrollbar {
        width: 4px;
      }
      .column-cards::-webkit-scrollbar-track {
        background: transparent;
      }
      .column-cards::-webkit-scrollbar-thumb {
        background: rgba(255, 255, 255, 0.12);
        border-radius: 2px;
      }
      .column-cards::-webkit-scrollbar-thumb:hover {
        background: rgba(255, 255, 255, 0.24);
      }

      /* Column Hover Specific header glows */
      .kanban-column:hover .status-recebida-border {
        border-bottom-color: #3b82f6;
        box-shadow: 0 3px 10px rgba(59, 130, 246, 0.25);
      }
      .kanban-column:hover .status-diagnostico-border {
        border-bottom-color: #f59e0b;
        box-shadow: 0 3px 10px rgba(245, 158, 11, 0.25);
      }
      .kanban-column:hover .status-aguardando-border {
        border-bottom-color: #8b5cf6;
        box-shadow: 0 3px 10px rgba(139, 92, 246, 0.25);
      }
      .kanban-column:hover .status-execucao-border {
        border-bottom-color: #f97316;
        box-shadow: 0 3px 10px rgba(249, 115, 22, 0.25);
      }
      .kanban-column:hover .status-finalizada-border {
        border-bottom-color: #10b981;
        box-shadow: 0 3px 10px rgba(16, 185, 129, 0.25);
      }

      /* Acender a borda da coluna inteira com a sua cor correspondente no hover */
      .kanban-column:hover:has(.status-recebida-border) {
        border-color: rgba(59, 130, 246, 0.25);
      }
      .kanban-column:hover:has(.status-diagnostico-border) {
        border-color: rgba(245, 158, 11, 0.25);
      }
      .kanban-column:hover:has(.status-aguardando-border) {
        border-color: rgba(139, 92, 246, 0.25);
      }
      .kanban-column:hover:has(.status-execucao-border) {
        border-color: rgba(249, 115, 22, 0.4);
      }
      .kanban-column:hover:has(.status-finalizada-border) {
        border-color: rgba(16, 185, 129, 0.25);
      }

      /* Column Hover Header Glows */
      .kanban-column:hover .column-header.glow-recebida {
        background: rgba(59, 130, 246, 0.08);
        box-shadow: 0 0 14px rgba(59, 130, 246, 0.45);
      }
      .kanban-column:hover .column-header.glow-diagnostico {
        background: rgba(245, 158, 11, 0.08);
        box-shadow: 0 0 14px rgba(245, 158, 11, 0.45);
      }
      .kanban-column:hover .column-header.glow-aguardando {
        background: rgba(139, 92, 246, 0.08);
        box-shadow: 0 0 14px rgba(139, 92, 246, 0.45);
      }
      .kanban-column:hover .column-header.glow-execucao {
        background: rgba(249, 115, 22, 0.1);
        box-shadow: 0 0 14px rgba(249, 115, 22, 0.5);
      }
      .kanban-column:hover .column-header.glow-finalizada {
        background: rgba(16, 185, 129, 0.08);
        box-shadow: 0 0 14px rgba(16, 185, 129, 0.45);
      }

      /* Smooth scale for column title on hover (does not affect cards layout) */
      .kanban-column:hover .column-header .column-title {
        transform: scale(1.05);
      }

      .empty-column {
        text-align: center;
        padding: 2.25rem 1rem;
        font-size: 0.78rem;
        color: #52525b;
        border: 1px dashed rgba(255, 255, 255, 0.06);
        border-radius: 8px;
        background: rgba(0, 0, 0, 0.08);
      }

      /* Tela de Erro de Conexao Premium */
      .comunicacao-erro-container {
        display: flex;
        flex-direction: column;
        align-items: center;
        justify-content: center;
        text-align: center;
        padding: 3rem;
        background: rgba(24, 24, 28, 0.6);
        backdrop-filter: blur(16px);
        border: 1px solid rgba(239, 68, 68, 0.15);
        border-radius: 12px;
        margin-top: 1.5rem;
        flex: 1;
        min-height: 350px;
      }
      .erro-icon-container {
        margin-bottom: 1.25rem;
        animation: pulse-error-icon 2s infinite ease-in-out;
      }
      @keyframes pulse-error-icon {
        0%,
        100% {
          transform: scale(1);
          filter: drop-shadow(0 0 2px rgba(239, 68, 68, 0.2));
        }
        50% {
          transform: scale(1.05);
          filter: drop-shadow(0 0 12px rgba(239, 68, 68, 0.5));
        }
      }
      .erro-titulo {
        font-family: 'Outfit', sans-serif;
        font-weight: 700;
        font-size: 1.35rem;
        color: #f8fafc;
        margin-bottom: 0.5rem;
      }
      .erro-subtitulo {
        font-size: 0.9rem;
        color: #94a3b8;
        max-width: 420px;
        margin-bottom: 1.5rem;
        line-height: 1.5;
      }
      .btn-large {
        padding: 0.65rem 1.5rem;
        font-size: 0.88rem;
      }

      /* Tela de Loading de Conexao */
      .comunicacao-loading-container {
        display: flex;
        flex-direction: column;
        align-items: center;
        justify-content: center;
        gap: 1.25rem;
        padding: 4rem;
        flex: 1;
        min-height: 350px;
      }
      .large-spinner {
        width: 44px;
        height: 44px;
        border: 3px solid rgba(237, 20, 91, 0.1);
        border-radius: 50%;
        border-top-color: #ed145b;
        animation: spin 1s ease-in-out infinite;
      }
      .loading-text {
        font-family: 'Outfit', sans-serif;
        font-size: 0.95rem;
        color: #a1a1aa;
        font-weight: 500;
      }
      @keyframes spin {
        to {
          transform: rotate(360deg);
        }
      }
    `,
  ],
})
export class OsKanbanPageComponent implements OnInit {
  private readonly kanbanWrapperElement = viewChild<ElementRef<HTMLElement>>('kanbanWrapper');

  isAtStart = signal(true);
  isAtEnd = signal(false);

  colunas = signal<KanbanColumn[]>([]);
  loading = signal(false);
  statusConexao = signal<'conectando' | 'ao-vivo' | 'sem-comunicacao'>('conectando');
  ultimaAtualizacao = signal<string>('');

  drawerOsId = signal<string | null>(null);
  drawerCard = signal<KanbanCard | null>(null);
  drawerOpen = signal(false);

  private readonly destroyRef = inject(DestroyRef);
  private readonly osService = inject(OrdemServicoService);
  private readonly notification = inject(NotificationService);

  ngOnInit(): void {
    this.iniciarPolling();
  }

  onScroll(): void {
    this.checkScroll();
  }

  checkScroll(): void {
    const el = this.kanbanWrapperElement()?.nativeElement;
    if (el) {
      const tolerance = 5;
      this.isAtStart.set(el.scrollLeft <= tolerance);
      this.isAtEnd.set(el.scrollLeft + el.clientWidth >= el.scrollWidth - tolerance);
    }
  }

  private iniciarPolling(): void {
    timer(0, 10000)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.carregarFila());
  }

  tentarNovamente(): void {
    this.statusConexao.set('conectando');
    this.iniciarPolling();
  }

  private carregarFila(callback?: () => void): void {
    const isFirstConnection = this.statusConexao() === 'conectando';
    if (isFirstConnection) {
      this.loading.set(true);
    }
    this.osService.getFilaKanban(1, 1000).subscribe({
      next: (res) => {
        this.statusConexao.set('ao-vivo');
        this.colunas.set(res || []);
        this.ultimaAtualizacao.set(new Date().toLocaleTimeString('pt-BR'));
        this.loading.set(false);
        if (callback) callback();
        this.checkScroll();
      },
      error: () => {
        this.statusConexao.set('sem-comunicacao');
        this.loading.set(false);
        if (callback) callback();
        this.checkScroll();
      },
    });
  }

  getColumnTitle(status: string): string {
    switch (status) {
      case 'Recebida':
        return 'Recebidas';
      case 'Diagnostico':
        return 'Em Diagnóstico';
      case 'Aprovacao':
        return 'Aprovação';
      case 'Execucao':
        return 'Em Execução';
      case 'Finalizada':
        return 'Finalizadas Hoje';
      default:
        return status;
    }
  }

  getColumnBorderClass(status: string): string {
    switch (status) {
      case 'Recebida':
        return 'status-recebida-border';
      case 'Diagnostico':
        return 'status-diagnostico-border';
      case 'Aprovacao':
        return 'status-aguardando-border';
      case 'Execucao':
        return 'status-execucao-border';
      case 'Finalizada':
        return 'status-finalizada-border';
      default:
        return '';
    }
  }

  getColumnGlowClass(status: string): string {
    switch (status) {
      case 'Recebida':
        return 'glow-recebida';
      case 'Diagnostico':
        return 'glow-diagnostico';
      case 'Aprovacao':
        return 'glow-aguardando';
      case 'Execucao':
        return 'glow-execucao';
      case 'Finalizada':
        return 'glow-finalizada';
      default:
        return '';
    }
  }

  abrirDrawer(card: KanbanCard): void {
    this.drawerOsId.set(card.id);
    this.drawerCard.set(card);
    this.drawerOpen.set(true);
  }

  fecharDrawer(): void {
    this.drawerOpen.set(false);
    this.drawerOsId.set(null);
    this.drawerCard.set(null);
  }
}
