import {
  Component,
  input,
  output,
  inject,
  signal,
  computed,
  effect,
  ChangeDetectionStrategy
} from '@angular/core';
import { DatePipe, DecimalPipe, SlicePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { DrawerComponent } from '../../../../shared/components/drawer/drawer.component';
import { StatusBadgeComponent } from '../../../../shared/components/status-badge/status-badge.component';
import { PlacaBadgeComponent } from '../../../../shared/components/placa-badge/placa-badge.component';
import { ProgressBarComponent } from '../../../../shared/components/progress-bar/progress-bar.component';
import { OrdemServicoService } from '../../services/ordem-servico.service';
import { KanbanCard, OsDetalhe, StatusOS } from '../../models/ordem-servico.model';

@Component({
  selector: 'app-os-drawer',
  standalone: true,
  imports: [
    DatePipe,
    DecimalPipe,
    SlicePipe,
    RouterLink,
    DrawerComponent,
    StatusBadgeComponent,
    PlacaBadgeComponent,
    ProgressBarComponent
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <app-drawer [isOpen]="isOpen()" width="520px" (closed)="onClose()">

      <!-- ════════════════════════════════════════ -->
      <!--  HEADER                                  -->
      <!-- ════════════════════════════════════════ -->
      <div drawer-header class="osd-header">
        <!-- Linha 1: ID completo da OS e Botão Fechar -->
        <div class="osd-header-top">
          <div class="osd-meta">
            <span class="osd-num">#{{ os()?.id || osId() || '—' }}</span>
          </div>
          <button class="osd-close" (click)="onClose()" aria-label="Fechar drawer">
            <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24"
                 fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round">
              <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
            </svg>
          </button>
        </div>

        <!-- Linha 2: Título Principal do Veículo -->
        <h2 class="osd-vehicle">{{ os()?.modeloVeiculo || cardPreview()?.modeloVeiculo || '—' }}</h2>

        <!-- Linha 3: Metadados secundários (Placa + Cliente + Mecânico alinhados) -->
        <div class="osd-header-info">
          @if (os()?.placaVeiculo || cardPreview()?.placaVeiculo) {
            <app-placa-badge [placa]="(os()?.placaVeiculo || cardPreview()?.placaVeiculo)!" size="md"></app-placa-badge>
          }
          <div class="osd-info-pill osd-client-pill" [title]="'Cliente: ' + (os()?.clienteNome || cardPreview()?.clienteNome || '—')">
            <svg xmlns="http://www.w3.org/2000/svg" width="12" height="12" viewBox="0 0 24 24"
                 fill="none" stroke="currentColor" stroke-width="2">
              <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/>
              <circle cx="12" cy="7" r="4"/>
            </svg>
            <span>{{ os()?.clienteNome || cardPreview()?.clienteNome || '—' }}</span>
          </div>
          @if (os()?.responsavelNome || os()?.responsavelId || cardPreview()?.mecanicoResponsavel) {
            <div class="osd-info-pill osd-mecanico-pill" [title]="'Mecânico Responsável: ' + (os()?.responsavelNome || os()?.responsavelId || cardPreview()?.mecanicoResponsavel)">
              <svg xmlns="http://www.w3.org/2000/svg" width="12" height="12" viewBox="0 0 24 24"
                   fill="none" stroke="currentColor" stroke-width="2">
                <path d="M14.7 6.3a1 1 0 0 0 0 1.4l1.6 1.6a1 1 0 0 0 1.4 0l3.77-3.77a6 6 0 0 1-7.94 7.94l-6.91 6.91a2.12 2.12 0 0 1-3-3l6.91-6.91a6 6 0 0 1 7.94-7.94l-3.76 3.76z"/>
              </svg>
              <span>{{ os()?.responsavelNome || os()?.responsavelId || cardPreview()?.mecanicoResponsavel }}</span>
            </div>
          }
        </div>
      </div>

      <!-- ════════════════════════════════════════ -->
      <!--  BODY                                    -->
      <!-- ════════════════════════════════════════ -->
      <div drawer-body class="osd-body">

        @if (loading()) {
          <!-- Skeleton -->
          <div class="osd-section">
            <div class="skeleton" style="width:45%;height:13px;margin-bottom:1rem;"></div>
            @for (_ of [1,2,3,4]; track $index) {
              <div style="display:flex;justify-content:space-between;margin-bottom:0.65rem;">
                <div class="skeleton" style="width:38%;height:11px;"></div>
                <div class="skeleton" style="width:48%;height:11px;"></div>
              </div>
            }
          </div>
          <div class="osd-section">
            <div class="skeleton" style="width:40%;height:13px;margin-bottom:0.75rem;"></div>
            <div class="skeleton" style="width:100%;height:56px;border-radius:6px;"></div>
          </div>
          <div class="osd-section">
            <div class="skeleton" style="width:50%;height:13px;margin-bottom:0.75rem;"></div>
            <div class="skeleton" style="width:100%;height:10px;border-radius:4px;"></div>
          </div>
        } @else if (erro()) {
          <div class="osd-empty">
            <svg xmlns="http://www.w3.org/2000/svg" width="36" height="36" viewBox="0 0 24 24"
                 fill="none" stroke="#EF4444" stroke-width="2">
              <circle cx="12" cy="12" r="10"/>
              <line x1="12" y1="8" x2="12" y2="12"/>
              <line x1="12" y1="16" x2="12.01" y2="16"/>
            </svg>
            <span>Erro ao carregar OS</span>
          </div>
        } @else if (os()) {

          <!-- ─── 1. Linha do Tempo de Etapas ─── -->
          <div class="osd-section">
            <div class="osd-section-header">
              <h4 class="osd-section-title" style="margin: 0;">
                <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24"
                     fill="none" stroke="currentColor" stroke-width="2">
                  <circle cx="12" cy="12" r="10"/>
                  <polyline points="12 6 12 12 16 14"/>
                </svg>
                Fluxo da Ordem de Serviço
              </h4>
              <app-status-badge [status]="os()!.status" size="sm"></app-status-badge>
            </div>

            <div class="osd-timeline">
              <!-- Evento: Abertura -->
              <div class="osd-tl-item tl-done">
                <div class="osd-tl-marker">
                  <svg xmlns="http://www.w3.org/2000/svg" width="10" height="10" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3.5"><polyline points="20 6 9 17 4 12"/></svg>
                </div>
                <div class="osd-tl-content">
                  <div class="osd-tl-header">
                    <span class="osd-tl-title">Abertura</span>
                  </div>
                  <div class="osd-tl-date">{{ os()!.criadoEm | date:'dd/MM/yyyy HH:mm' }}</div>
                </div>
              </div>

              <!-- Evento: Envio para Aprovação -->
              @if (os()!.envioAprovacaoEm) {
                <div class="osd-tl-item tl-done">
                  <div class="osd-tl-marker">
                    <svg xmlns="http://www.w3.org/2000/svg" width="10" height="10" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3.5"><polyline points="20 6 9 17 4 12"/></svg>
                  </div>
                  <div class="osd-tl-content">
                    <div class="osd-tl-header">
                      <span class="osd-tl-title">Envio Aprovação</span>
                      @if (isAprovado()) {
                        <span class="osd-tl-badge badge-approved">
                          <svg xmlns="http://www.w3.org/2000/svg" width="10" height="10" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12"/></svg>
                          Aprovado
                        </span>
                      } @else {
                        <span class="osd-tl-badge badge-active">Aguardando</span>
                      }
                    </div>
                    <div class="osd-tl-date">{{ os()!.envioAprovacaoEm | date:'dd/MM/yyyy HH:mm' }}</div>
                  </div>
                </div>
              }

              <!-- Evento: Início da Execução -->
              @if (os()!.iniciadoEm) {
                <div class="osd-tl-item tl-done">
                  <div class="osd-tl-marker">
                    <svg xmlns="http://www.w3.org/2000/svg" width="10" height="10" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3.5"><polyline points="20 6 9 17 4 12"/></svg>
                  </div>
                  <div class="osd-tl-content">
                    <div class="osd-tl-header">
                      <span class="osd-tl-title">Início Execução</span>
                    </div>
                    <div class="osd-tl-date">{{ os()!.iniciadoEm | date:'dd/MM/yyyy HH:mm' }}</div>
                  </div>
                </div>
              }

              <!-- Evento: Finalização -->
              @if (os()!.finalizadoEm) {
                <div class="osd-tl-item tl-done">
                  <div class="osd-tl-marker">
                    <svg xmlns="http://www.w3.org/2000/svg" width="10" height="10" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3.5"><polyline points="20 6 9 17 4 12"/></svg>
                  </div>
                  <div class="osd-tl-content">
                    <div class="osd-tl-header">
                      <span class="osd-tl-title">Finalização</span>
                    </div>
                    <div class="osd-tl-date">{{ os()!.finalizadoEm | date:'dd/MM/yyyy HH:mm' }}</div>
                  </div>
                </div>
              }

              <!-- Evento: Entrega -->
              @if (os()!.entregueEm) {
                <div class="osd-tl-item tl-delivered">
                  <div class="osd-tl-marker">
                    <svg xmlns="http://www.w3.org/2000/svg" width="10" height="10" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3.5"><polyline points="20 6 9 17 4 12"/></svg>
                  </div>
                  <div class="osd-tl-content">
                    <div class="osd-tl-header">
                      <span class="osd-tl-title">Entrega</span>
                    </div>
                    <div class="osd-tl-date">{{ os()!.entregueEm | date:'dd/MM/yyyy HH:mm' }}</div>
                  </div>
                </div>
              }
            </div>
          </div>

          <!-- ─── 2. Diagnóstico ─── -->
          <div class="osd-section">
            <h4 class="osd-section-title">
              <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24"
                   fill="none" stroke="currentColor" stroke-width="2">
                <circle cx="11" cy="11" r="8"/>
                <line x1="21" y1="21" x2="16.65" y2="16.65"/>
              </svg>
              Diagnóstico
            </h4>
            <div class="osd-diagnostico" [class.osd-diagnostico-empty]="!os()!.observacao">
              {{ os()!.observacao || 'Diagnóstico ainda não registrado.' }}
            </div>
          </div>

          <!-- ─── 3. Serviços & Progresso ─── -->
          <div class="osd-section">
            <h4 class="osd-section-title">
              <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24"
                   fill="none" stroke="currentColor" stroke-width="2">
                <path d="M14.7 6.3a1 1 0 0 0 0 1.4l1.6 1.6a1 1 0 0 0 1.4 0l3.77-3.77a6 6 0 0 1-7.94 7.94l-6.91 6.91a2.12 2.12 0 0 1-3-3l6.91-6.91a6 6 0 0 1 7.94-7.94l-3.76 3.76z"/>
              </svg>
              Serviços
              <span class="osd-count">{{ os()!.servicos.length }}</span>
            </h4>

            @if (os()!.servicos.length === 0) {
              <p class="osd-empty-text">Sem serviços adicionados.</p>
            } @else {
              <div class="osd-progress-block">
                <app-progress-bar
                  [completed]="servicosConcluidos()"
                  [total]="os()!.servicos.length"
                  [showCount]="true"
                  labelPosition="bottom"
                  itemText="serviços"
                  size="md">
                </app-progress-bar>
              </div>

              <div class="osd-list">
                @for (s of os()!.servicos; track s.id) {
                  <div class="osd-list-item">
                    <div class="osd-list-left">
                      <!-- Ícone tri-estado harmonizado com a timeline -->
                      @if (s.status === 'Concluido') {
                        <span class="osd-status-icon osd-status-concluido" title="Concluído">
                          <svg xmlns="http://www.w3.org/2000/svg" width="10" height="10" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3.5"><polyline points="20 6 9 17 4 12"/></svg>
                        </span>
                      } @else if (s.status === 'EmExecucao') {
                        <span class="osd-status-icon osd-status-execucao" title="Em Execução">
                          <svg xmlns="http://www.w3.org/2000/svg" width="13" height="13"
                               viewBox="0 0 24 24" fill="currentColor">
                            <circle cx="12" cy="12" r="5"/>
                          </svg>
                        </span>
                      } @else {
                        <span class="osd-status-icon osd-status-pendente" title="Pendente">
                          <svg xmlns="http://www.w3.org/2000/svg" width="13" height="13"
                               viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                            <circle cx="12" cy="12" r="9"/>
                          </svg>
                        </span>
                      }
                      <span class="osd-item-name">{{ s.nomeServico || ('Serviço #' + (s.servicoId | slice:0:8)) }}</span>
                    </div>
                    <span class="osd-item-val">R$ {{ s.valorCobrado | number:'1.2-2' }}</span>
                  </div>
                }
              </div>
            }
          </div>

          <!-- ─── 5. Insumos ─── -->
          <div class="osd-section">
            <h4 class="osd-section-title">
              <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24"
                   fill="none" stroke="currentColor" stroke-width="2">
                <path d="M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z"/>
              </svg>
              Insumos
              <span class="osd-count">{{ os()!.insumos.length }}</span>
            </h4>
            @if (os()!.insumos.length === 0) {
              <p class="osd-empty-text">Sem insumos adicionados.</p>
            } @else {
              <div class="osd-list">
                @for (ins of os()!.insumos; track ins.id) {
                  <div class="osd-list-item">
                    <div class="osd-list-left" style="flex-direction:column;align-items:flex-start;gap:0.1rem;">
                      <span class="osd-item-name">{{ ins.descricao }}</span>
                      <span class="osd-item-sub">
                        <span class="osd-qty-badge">{{ ins.quantidade }}x</span>
                        R$ {{ ins.valorUnitario | number:'1.2-2' }}/un
                      </span>
                    </div>
                    <span class="osd-item-val">R$ {{ ins.valorTotal | number:'1.2-2' }}</span>
                  </div>
                }
              </div>
            }
          </div>

          <!-- ─── 7. Resumo Financeiro ─── -->
          <div class="osd-section osd-section-last">
            <h4 class="osd-section-title">
              <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24"
                   fill="none" stroke="currentColor" stroke-width="2">
                <line x1="12" y1="1" x2="12" y2="23"/>
                <path d="M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6"/>
              </svg>
              Resumo Financeiro
            </h4>
            <div class="osd-financeiro">
              <div class="osd-fin-row">
                <span>Subtotal Serviços</span>
                <span>R$ {{ subtotalServicos() | number:'1.2-2' }}</span>
              </div>
              <div class="osd-fin-row">
                <span>Subtotal Insumos</span>
                <span>R$ {{ subtotalInsumos() | number:'1.2-2' }}</span>
              </div>
              <div class="osd-fin-divider"></div>
              <div class="osd-fin-total">
                <span>VALOR TOTAL</span>
                <span class="osd-total-val">R$ {{ os()!.valorTotal | number:'1.2-2' }}</span>
              </div>
            </div>
          </div>

        }
      </div>

      <!-- ════════════════════════════════════════ -->
      <!--  FOOTER                                  -->
      <!-- ════════════════════════════════════════ -->
      <div drawer-footer class="osd-footer">
        @if (os()) {
          <a
            [routerLink]="['/ordens-servico', os()!.id]"
            class="btn btn-primary osd-action-btn"
            (click)="onClose()">
            <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24"
                 fill="none" stroke="currentColor" stroke-width="2.2">
              <path d="M18 13v6a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h6"/>
              <polyline points="15 3 21 3 21 9"/>
              <line x1="10" y1="14" x2="21" y2="3"/>
            </svg>
            Abrir OS Completa
          </a>
        } @else if (loading()) {
          <div class="osd-action-btn osd-action-loading">Carregando…</div>
        }
      </div>

    </app-drawer>
  `,
  styles: [`
    /* ════ Header ════ */
    .osd-header {
      padding: 1.25rem 1.25rem 1rem;
      background: rgba(24, 24, 28, 0.98);
    }

    .osd-header-top {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 0.75rem;
    }

    .osd-meta {
      display: flex;
      align-items: center;
      gap: 0.6rem;
    }

    .osd-num {
      font-family: var(--font-mono, 'JetBrains Mono', monospace);
      font-size: 0.75rem;
      color: var(--text-subtle, #71717A);
      font-weight: 600;
      letter-spacing: 0.04em;
    }

    .osd-close {
      background: none;
      border: none;
      color: var(--text-subtle, #71717A);
      cursor: pointer;
      padding: 0.3rem;
      border-radius: 6px;
      display: flex;
      align-items: center;
      justify-content: center;
      transition: color 0.15s ease, transform 0.15s ease, background 0.15s ease;
    }
    .osd-close:hover {
      color: var(--text-main, #F8FAFC);
      background: rgba(255, 255, 255, 0.06);
      transform: scale(1.1);
    }
    .osd-close:active { transform: scale(0.92); }

    .osd-vehicle-row {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 0.75rem;
      margin: 0 0 0.6rem;
    }

    .osd-vehicle {
      font-family: var(--font-display, 'Outfit', sans-serif);
      font-size: 1.2rem;
      font-weight: 700;
      color: #F8FAFC;
      margin: 0;
      line-height: 1.2;
      flex: 1;
      min-width: 0;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }

    .osd-header-info {
      display: flex;
      align-items: center;
      gap: 0.55rem;
      flex-wrap: wrap;
    }

    .osd-info-pill {
      display: inline-flex;
      align-items: center;
      gap: 0.35rem;
      font-size: 0.78rem;
      font-weight: 500;
      padding: 0.18rem 0.55rem;
      border-radius: 6px;
      line-height: 1;
    }

    .osd-client-pill {
      color: var(--text-muted, #A1A1AA);
      background: rgba(255, 255, 255, 0.04);
      border: 1px solid rgba(255, 255, 255, 0.08);
    }

    .osd-mecanico-pill {
      color: #CBD5E1;
      background: rgba(255, 255, 255, 0.04);
      border: 1px solid rgba(255, 255, 255, 0.08);
    }

    .osd-mecanico-pill svg {
      color: var(--text-subtle, #71717A);
    }

    /* ════ Body ════ */
    .osd-body {
      padding: 0;
    }

    .osd-section {
      padding: 1.1rem 1.25rem;
      border-bottom: 1px solid rgba(255, 255, 255, 0.05);
      animation: osdFadeIn 0.3s ease-out both;
    }

    .osd-section-last {
      border-bottom: none;
    }

    @keyframes osdFadeIn {
      from { opacity: 0; transform: translateY(5px); }
      to   { opacity: 1; transform: translateY(0); }
    }

    /* ── Timeline de Etapas Equilibrio ── */
    .osd-timeline {
      display: flex;
      flex-direction: column;
      gap: 0;
      position: relative;
      padding-left: 0.1rem;
    }

    .osd-tl-item {
      display: flex;
      align-items: flex-start;
      gap: 0.85rem;
      position: relative;
      padding-bottom: 0.95rem;
    }

    .osd-tl-item:last-child {
      padding-bottom: 0.1rem;
    }

    .osd-tl-item:not(:last-child)::after {
      content: '';
      position: absolute;
      left: 9px;
      top: 20px;
      bottom: 0;
      width: 2px;
      background: rgba(255, 255, 255, 0.08);
    }

    .osd-tl-item.tl-done:not(:last-child)::after {
      background: rgba(16, 185, 129, 0.3);
    }

    .osd-tl-marker {
      width: 20px;
      height: 20px;
      border-radius: 50%;
      background: rgba(255, 255, 255, 0.05);
      border: 2px solid rgba(255, 255, 255, 0.15);
      color: var(--text-subtle, #71717A);
      display: flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;
      margin-top: 0.05rem;
      z-index: 1;
      transition: all 0.2s ease;
    }

    .osd-tl-item.tl-done .osd-tl-marker {
      background: rgba(16, 185, 129, 0.15);
      border-color: #10B981;
      color: #10B981;
      box-shadow: 0 0 8px rgba(16, 185, 129, 0.35);
    }

    .osd-tl-item.tl-delivered .osd-tl-marker {
      background: #10B981;
      border-color: #10B981;
      color: #FFFFFF;
      box-shadow: 0 0 10px rgba(16, 185, 129, 0.5);
    }

    .osd-tl-content {
      display: flex;
      flex-direction: column;
      gap: 0.15rem;
      flex: 1;
      min-width: 0;
    }

    .osd-tl-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      gap: 0.5rem;
    }

    .osd-tl-title {
      font-family: var(--font-display, 'Outfit', sans-serif);
      font-size: 0.85rem;
      font-weight: 700;
      color: var(--text-main, #F8FAFC);
    }

    .osd-tl-date {
      font-family: var(--font-mono, 'JetBrains Mono', monospace);
      font-size: 0.73rem;
      color: var(--text-muted, #A1A1AA);
    }

    .osd-tl-badge {
      font-size: 0.62rem;
      font-weight: 700;
      padding: 0.08rem 0.45rem;
      border-radius: 999px;
      text-transform: uppercase;
      letter-spacing: 0.03em;
      display: inline-flex;
      align-items: center;
      gap: 0.25rem;
      white-space: nowrap;
    }

    .badge-done {
      background: rgba(59, 130, 246, 0.12);
      color: #60A5FA;
      border: 1px solid rgba(59, 130, 246, 0.25);
    }

    .badge-approved {
      background: rgba(16, 185, 129, 0.15);
      color: #10B981;
      border: 1px solid rgba(16, 185, 129, 0.3);
    }

    .badge-active {
      background: rgba(237, 20, 91, 0.15);
      color: #FF5388;
      border: 1px solid rgba(237, 20, 91, 0.3);
    }

    .badge-delivered {
      background: rgba(16, 185, 129, 0.2);
      color: #34D399;
      border: 1px solid #10B981;
    }

    .osd-section-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 0.85rem;
    }

    .osd-section-title {
      display: flex;
      align-items: center;
      gap: 0.45rem;
      font-family: var(--font-display, 'Outfit', sans-serif);
      font-size: 0.82rem;
      font-weight: 700;
      text-transform: uppercase;
      letter-spacing: 0.06em;
      color: var(--text-muted, #A1A1AA);
      margin: 0 0 0.85rem;
    }

    .osd-count {
      background: rgba(255, 255, 255, 0.08);
      color: var(--text-subtle, #71717A);
      padding: 0.05rem 0.45rem;
      border-radius: 999px;
      font-size: 0.7rem;
      font-weight: 700;
      margin-left: auto;
    }

    /* ── Informações Gerais (grid key-value) ── */
    .osd-kv-grid {
      display: grid;
      grid-template-columns: auto 1fr;
      gap: 0.5rem 1.25rem;
      align-items: baseline;
    }

    .osd-key {
      font-size: 0.72rem;
      font-weight: 600;
      text-transform: uppercase;
      letter-spacing: 0.04em;
      color: var(--text-subtle, #71717A);
      white-space: nowrap;
    }

    .osd-val {
      font-size: 0.85rem;
      color: var(--text-main, #F8FAFC);
      font-weight: 500;
    }

    .osd-val-disabled {
      color: rgba(161, 161, 170, 0.45);
      font-style: italic;
    }

    .osd-check-approved {
      display: inline-flex;
      align-items: center;
      gap: 0.25rem;
      margin-left: 0.5rem;
      padding: 0.1rem 0.4rem;
      background: rgba(16, 185, 129, 0.12);
      border: 1px solid rgba(16, 185, 129, 0.3);
      border-radius: 999px;
      color: #10B981;
      font-size: 0.7rem;
      font-weight: 700;
      letter-spacing: 0.02em;
    }

    /* ── Diagnóstico ── */
    .osd-diagnostico {
      background: rgba(255, 255, 255, 0.03);
      border-left: 3px solid var(--status-diagnostico, #F59E0B);
      border-radius: 0 6px 6px 0;
      padding: 0.75rem 1rem;
      font-size: 0.87rem;
      color: var(--text-muted, #A1A1AA);
      line-height: 1.55;
    }

    .osd-diagnostico-empty {
      font-style: italic;
      color: var(--text-subtle, #71717A);
      border-left-color: rgba(255, 255, 255, 0.1);
    }

    /* ── Progresso nos Serviços ── */
    .osd-progress-block {
      margin-bottom: 1rem;
    }

    /* ── Listas (serviços / insumos) ── */
    .osd-list {
      display: flex;
      flex-direction: column;
      gap: 0;
    }

    .osd-list-item {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 0.55rem 0;
      border-bottom: 1px solid rgba(255, 255, 255, 0.04);
    }

    .osd-list-item:last-child { border-bottom: none; }

    .osd-list-left {
      display: flex;
      align-items: center;
      gap: 0.55rem;
      min-width: 0;
      flex: 1;
    }

    .osd-item-name {
      font-size: 0.85rem;
      font-weight: 600;
      color: var(--text-main, #F8FAFC);
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }

    .osd-item-val {
      font-family: var(--font-mono, 'JetBrains Mono', monospace);
      font-size: 0.83rem;
      font-weight: 700;
      color: var(--mono-text, #E2E8F0);
      white-space: nowrap;
      margin-left: 0.75rem;
    }

    .osd-item-sub {
      font-size: 0.73rem;
      color: var(--text-subtle, #71717A);
      display: flex;
      align-items: center;
      gap: 0.35rem;
    }

    .osd-qty-badge {
      font-family: var(--font-mono, 'JetBrains Mono', monospace);
      background: var(--mono-badge-bg, rgba(255, 255, 255, 0.06));
      padding: 0.05rem 0.3rem;
      border-radius: 4px;
      font-size: 0.7rem;
      font-weight: 700;
    }

    /* ── Status icons tri-estado ── */
    .osd-status-icon {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;
      width: 20px;
      height: 20px;
      border-radius: 50%;
    }

    .osd-status-concluido {
      color: #10B981;
      background: rgba(16, 185, 129, 0.15);
      border: 2px solid #10B981;
      box-shadow: 0 0 8px rgba(16, 185, 129, 0.35);
    }

    .osd-status-execucao {
      color: #F97316;
      background: rgba(249, 115, 22, 0.12);
      border: 2px solid #F97316;
    }

    .osd-status-pendente {
      color: #71717A;
      background: rgba(255, 255, 255, 0.05);
      border: 2px solid rgba(255, 255, 255, 0.1);
    }

    .osd-empty-text {
      font-size: 0.85rem;
      color: var(--text-subtle, #71717A);
      font-style: italic;
      margin: 0.5rem 0 0;
    }

    /* ── Resumo Financeiro ── */
    .osd-financeiro {
      display: flex;
      flex-direction: column;
      gap: 0.45rem;
      background: rgba(255, 255, 255, 0.02);
      border-radius: 8px;
      padding: 1rem;
      border: 1px solid rgba(255, 255, 255, 0.04);
    }

    .osd-fin-row {
      display: flex;
      justify-content: space-between;
      font-size: 0.85rem;
      color: var(--text-muted, #A1A1AA);
    }

    .osd-fin-row span:last-child {
      font-family: var(--font-mono, 'JetBrains Mono', monospace);
      font-weight: 500;
      color: #CBD5E1;
    }

    .osd-fin-divider {
      height: 1px;
      background: rgba(255, 255, 255, 0.06);
      margin: 0.4rem 0;
    }

    .osd-fin-total {
      display: flex;
      justify-content: space-between;
      align-items: center;
      font-weight: 700;
      font-size: 0.85rem;
      color: var(--text-main, #F8FAFC);
    }

    .osd-total-val {
      font-family: var(--font-mono, 'JetBrains Mono', monospace);
      font-size: 1.15rem;
      color: #10B981;
      text-shadow: 0 0 12px rgba(16, 185, 129, 0.2);
    }

    /* ── Utilities ── */
    .skeleton {
      background: linear-gradient(90deg, rgba(255,255,255,0.03) 25%, rgba(255,255,255,0.08) 50%, rgba(255,255,255,0.03) 75%);
      background-size: 400% 100%;
      animation: skeletonLoading 1.5s ease-in-out infinite;
      border-radius: 4px;
    }
    @keyframes skeletonLoading {
      0% { background-position: 200% 0; }
      100% { background-position: -200% 0; }
    }

    .osd-empty {
      padding: 3rem 1.5rem;
      text-align: center;
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 1rem;
      color: #71717A;
    }
    .osd-empty svg { opacity: 0.5; }

    /* ════ Footer ════ */
    .osd-footer {
      padding: 1rem 1.25rem;
      background: rgba(24, 24, 28, 0.98);
      border-top: 1px solid rgba(255, 255, 255, 0.06);
      display: flex;
      justify-content: flex-end;
    }

    .osd-action-btn {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      gap: 0.5rem;
      padding: 0.75rem 1.25rem;
      font-size: 0.9rem;
      font-weight: 600;
      border-radius: 8px;
      text-decoration: none;
      transition: all 0.2s ease;
      width: 100%;
    }

    .btn-primary {
      background: #ED145B;
      color: #FFFFFF;
      border: 1px solid rgba(237, 20, 91, 0.4);
      box-shadow: 0 4px 12px rgba(237, 20, 91, 0.2);
    }
    .btn-primary:hover {
      background: #E01356;
      box-shadow: 0 6px 16px rgba(237, 20, 91, 0.3);
      transform: translateY(-1px);
    }

    .osd-action-loading {
      background: rgba(255, 255, 255, 0.05);
      color: rgba(255, 255, 255, 0.3);
      cursor: not-allowed;
    }
  `]
})
export class OsDrawerComponent {
  isOpen = input<boolean>(false);
  osId = input<string | null>(null);
  cardPreview = input<KanbanCard | null>(null);

  closed = output<void>();

  // ── Estado via signals (compatível com OnPush sem cdr.markForCheck) ──
  readonly os = signal<OsDetalhe | null>(null);
  readonly loading = signal(false);
  readonly erro = signal(false);

  // ── Computed (I2: isAprovado eliminando lógica duplicada de normalização) ──
  readonly isAprovado = computed(() => {
    const status = this.os()?.status;
    if (status == null) return false;
    const num = typeof status === 'number' ? status : Number(status);
    return !Number.isNaN(num) && num > StatusOS.AguardandoAprovacao;
  });

  readonly servicosConcluidos = computed(() =>
    this.os()?.servicos?.filter(s => s.status === 'Concluido').length ?? 0
  );

  readonly subtotalServicos = computed(() =>
    this.os()?.servicos?.reduce((acc, s) => acc + s.valorCobrado, 0) ?? 0
  );

  readonly subtotalInsumos = computed(() =>
    this.os()?.insumos?.reduce((acc, i) => acc + i.valorTotal, 0) ?? 0
  );

  private readonly osService = inject(OrdemServicoService);

  constructor() {
    // B2: effect() substitui ngOnChanges + setTimeout — reage aos signals isOpen/osId
    effect(() => {
      const open = this.isOpen();
      const id   = this.osId();

      if (open && id) {
        this.carregarOs(id);
      } else if (open && !id) {
        // Drawer aberto sem ID — estado de erro imediato
        this.os.set(null);
        this.erro.set(true);
        this.loading.set(false);
      } else if (!open) {
        // Limpar estado após a animação de fechamento (300 ms)
        setTimeout(() => {
          this.os.set(null);
          this.erro.set(false);
        }, 300);
      }
    });
  }

  carregarOs(id: string): void {
    this.loading.set(true);
    this.erro.set(false);
    this.osService.getById(id).subscribe({
      next: (res) => {
        this.os.set(res);
        this.loading.set(false);
      },
      error: () => {
        this.erro.set(true);
        this.loading.set(false);
      },
    });
  }

  onClose(): void {
    this.closed.emit();
  }
}
