import { Component, ChangeDetectionStrategy } from '@angular/core';
import { DatePipe } from '@angular/common';
import { PlacaBadgeComponent } from '../../../../shared/components/placa-badge/placa-badge.component';
import { BaseKanbanCardComponent } from './base-kanban-card.component';

@Component({
  selector: 'app-diagnosis-kanban-card',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [DatePipe, PlacaBadgeComponent],
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
        @if (card().quantidadeServicos !== undefined) {
          <div class="info-row">
            <span>Serviços:</span> <strong>{{ card().quantidadeServicos }} itens</strong>
          </div>
        }
      </div>
      <div class="os-footer">
        @if (card().dataEntrada) {
          <span class="info-val">Entrada: {{ card().dataEntrada | date:'dd/MM/yyyy' }}</span>
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
    .os-client { font-size: 0.8rem; color: #A1A1AA; display: flex; align-items: center; gap: 0.35rem; min-width: 0; flex: 1; }
    .os-client-name { white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
    .info-row { font-size: 0.75rem; color: #A1A1AA; display: flex; gap: 0.3rem; }
    .info-row strong { color: #E2E8F0; }
    .os-footer { display: flex; justify-content: space-between; align-items: center; margin-top: 0.2rem; }
    .info-val { font-size: 0.75rem; color: #94A3B8; }
  `]
})
export class DiagnosisKanbanCardComponent extends BaseKanbanCardComponent {}
