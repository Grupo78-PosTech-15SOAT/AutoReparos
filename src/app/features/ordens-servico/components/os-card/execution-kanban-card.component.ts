import { Component, ChangeDetectionStrategy } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { PlacaBadgeComponent } from '../../../../shared/components/placa-badge/placa-badge.component';
import { ProgressBarComponent } from '../../../../shared/components/progress-bar/progress-bar.component';
import { BaseKanbanCardComponent } from './base-kanban-card.component';

@Component({
  selector: 'app-execution-kanban-card',
  standalone: true,
  imports: [DecimalPipe, PlacaBadgeComponent, ProgressBarComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="os-card" (click)="cardClick.emit(card())" role="button" tabindex="0" (keydown.enter)="cardClick.emit(card())">
      <div class="os-card-header">
        <h3 class="os-vehicle" [title]="card().modeloVeiculo">{{ card().modeloVeiculo }}</h3>
        @if (showOsId) {
          <span class="os-number">{{ formattedId() }}</span>
        }
      </div>
      <div class="os-body">
        <div class="os-meta-row">
          <div class="os-client" [title]="card().clienteNome">
            <svg xmlns="http://www.w3.org/2000/svg" width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/></svg>
            <span class="os-client-name">{{ card().clienteNome }}</span>
          </div>
          <app-placa-badge [placa]="card().placaVeiculo" size="sm"></app-placa-badge>
        </div>

        @if (card().mecanicoResponsavel) {
          <div class="os-mecanico-row">
            <div class="osd-info-pill osd-mecanico-pill" [title]="'Mecânico Responsável: ' + card().mecanicoResponsavel">
              <svg xmlns="http://www.w3.org/2000/svg" width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M14.7 6.3a1 1 0 0 0 0 1.4l1.6 1.6a1 1 0 0 0 1.4 0l3.77-3.77a6 6 0 0 1-7.94 7.94l-6.91 6.91a2.12 2.12 0 0 1-3-3l6.91-6.91a6 6 0 0 1 7.94-7.94l-3.76 3.76z"/>
              </svg>
              <span>{{ card().mecanicoResponsavel }}</span>
            </div>
          </div>
        }

        @if (card().servicosTotal !== undefined) {
          <div class="os-progress-wrapper">
            <app-progress-bar
              [completed]="card().servicosConcluidos || 0"
              [total]="card().servicosTotal || 0"
              [showCount]="true"
              labelPosition="right"
              size="sm">
            </app-progress-bar>
          </div>
        }
      </div>
      <div class="os-footer">
        @if (card().valorAprovado) {
          <span class="price-val">R$ {{ card().valorAprovado | number:'1.2-2' }}</span>
        }
      </div>
    </div>
  `,
  styles: [`
    :host { display: block; width: 100%; }
    .os-card { background: #1E1E24; border: 1px solid rgba(255,255,255,0.03); border-radius: 8px; padding: 0.85rem 0.95rem; display: flex; flex-direction: column; gap: 0.6rem; box-shadow: 0 4px 12px rgba(0,0,0,0.25); text-decoration: none; cursor: pointer; transition: all 0.2s cubic-bezier(0.16,1,0.3,1); }
    .os-card:hover { border-color: #ED145B; box-shadow: 0 6px 20px rgba(0,0,0,0.4), 0 0 8px rgba(237, 20, 91, 0.2); transform: translateY(-2px); }
    .os-card-header { display: flex; justify-content: space-between; align-items: baseline; gap: 0.5rem; }
    .os-vehicle { font-family: 'Outfit', sans-serif; font-size: 1rem; font-weight: 700; color: #F8FAFC; margin: 0; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; max-width: 160px; flex: 1; }
    .os-number { font-family: 'JetBrains Mono', monospace; font-weight: 500; font-size: 0.7rem; color: rgba(113,113,122,0.55); }
    .os-body { display: flex; flex-direction: column; gap: 0.45rem; }
    .os-meta-row { display: flex; justify-content: space-between; align-items: center; gap: 0.5rem; }
    .os-mecanico-row { display: flex; align-items: center; margin-top: 0.1rem; }
    .osd-info-pill { display: inline-flex; align-items: center; gap: 0.35rem; font-size: 0.73rem; font-weight: 500; padding: 0.15rem 0.5rem; border-radius: 6px; line-height: 1; }
    .osd-mecanico-pill { color: #CBD5E1; background: rgba(255, 255, 255, 0.04); border: 1px solid rgba(255, 255, 255, 0.08); }
    .osd-mecanico-pill svg { color: var(--text-subtle, #71717A); }
    .os-client { font-size: 0.8rem; color: #A1A1AA; display: flex; align-items: center; gap: 0.35rem; min-width: 0; flex: 1; }
    .os-client-name { white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
    .os-progress-wrapper { margin-top: 0.15rem; }
    .os-footer { display: flex; justify-content: space-between; align-items: center; margin-top: 0.2rem; }
    .price-val { font-family: 'JetBrains Mono', monospace; font-weight: 700; color: #10B981; font-size: 0.95rem; }
  `]
})
export class ExecutionKanbanCardComponent extends BaseKanbanCardComponent {}
