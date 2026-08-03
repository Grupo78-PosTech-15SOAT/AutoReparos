import { Component, ChangeDetectionStrategy, computed } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { PlacaBadgeComponent } from '../../../../shared/components/placa-badge/placa-badge.component';
import { BaseKanbanCardComponent } from './base-kanban-card.component';

@Component({
  selector: 'app-finished-kanban-card',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [DatePipe, DecimalPipe, PlacaBadgeComponent],
  template: `
    <div class="os-card" [class.os-card-delivered]="isEntregue()" (click)="cardClick.emit(card())" role="button" tabindex="0" (keydown.enter)="cardClick.emit(card())">
      <div class="os-card-header">
        <h3 class="os-vehicle" [title]="card().modeloVeiculo">{{ card().modeloVeiculo }}</h3>
        <div class="os-header-right">
          @if (isEntregue()) {
            <span class="os-delivered-badge">
              <svg xmlns="http://www.w3.org/2000/svg" width="10" height="10" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12"/></svg>
              Entregue
            </span>
          }
          @if (showOsId) {
            <span class="os-number">{{ formattedId() }}</span>
          }
        </div>
      </div>
      <div class="os-body">
        <div class="os-meta-row">
          <div class="os-client" [title]="card().clienteNome">
            <svg xmlns="http://www.w3.org/2000/svg" width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/></svg>
            <span class="os-client-name">{{ card().clienteNome }}</span>
          </div>
          <app-placa-badge [placa]="card().placaVeiculo" size="sm"></app-placa-badge>
        </div>
      </div>
      <div class="os-footer">
        @if (card().valorFinal) {
          <span class="price-val">R$ {{ card().valorFinal | number:'1.2-2' }}</span>
        }
        @if (card().dataConclusao) {
          <span class="info-val">{{ isEntregue() ? 'Entregue:' : 'Fim:' }} {{ card().dataConclusao | date:'HH:mm' }}</span>
        }
      </div>
    </div>
  `,
  styles: [`
    :host { display: block; width: 100%; }
    .os-card { background: #1E1E24; border: 1px solid rgba(255,255,255,0.03); border-radius: 8px; padding: 0.85rem 0.95rem; display: flex; flex-direction: column; gap: 0.6rem; box-shadow: 0 4px 12px rgba(0,0,0,0.25); text-decoration: none; cursor: pointer; transition: all 0.2s cubic-bezier(0.16,1,0.3,1); position: relative; }
    .os-card:hover { border-color: #ED145B; box-shadow: 0 6px 20px rgba(0,0,0,0.4), 0 0 8px rgba(237, 20, 91, 0.2); transform: translateY(-2px); }
    .os-card-delivered { border-color: rgba(16, 185, 129, 0.35); background: linear-gradient(180deg, rgba(16, 185, 129, 0.05) 0%, #1E1E24 100%); }
    .os-card-delivered:hover { border-color: #10B981; box-shadow: 0 6px 20px rgba(0,0,0,0.4), 0 0 10px rgba(16, 185, 129, 0.25); }
    .os-card-header { display: flex; justify-content: space-between; align-items: center; gap: 0.5rem; }
    .os-header-right { display: flex; align-items: center; gap: 0.4rem; }
    .os-delivered-badge { font-size: 0.65rem; font-weight: 700; color: #10B981; background: rgba(16, 185, 129, 0.15); border: 1px solid rgba(16, 185, 129, 0.3); padding: 0.1rem 0.4rem; border-radius: 999px; display: inline-flex; align-items: center; gap: 0.2rem; text-transform: uppercase; letter-spacing: 0.03em; }
    .os-vehicle { font-family: 'Outfit', sans-serif; font-size: 1rem; font-weight: 700; color: #F8FAFC; margin: 0; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; max-width: 140px; flex: 1; }
    .os-number { font-family: 'JetBrains Mono', monospace; font-weight: 500; font-size: 0.7rem; color: rgba(113,113,122,0.55); }
    .os-body { display: flex; flex-direction: column; gap: 0.45rem; }
    .os-meta-row { display: flex; justify-content: space-between; align-items: center; gap: 0.5rem; }
    .os-client { font-size: 0.8rem; color: #A1A1AA; display: flex; align-items: center; gap: 0.35rem; min-width: 0; flex: 1; }
    .os-client-name { white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
    .os-footer { display: flex; justify-content: space-between; align-items: center; margin-top: 0.2rem; }
    .price-val { font-family: 'JetBrains Mono', monospace; font-weight: 700; color: #10B981; font-size: 0.95rem; }
    .info-val { font-size: 0.75rem; color: #94A3B8; }
  `]
})
export class FinishedKanbanCardComponent extends BaseKanbanCardComponent {
  isEntregue = computed(() => this.card().status === 'Entregue' || this.card().statusEntrega === 'Entregue');
}
