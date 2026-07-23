import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { OrdemServico } from '../../models/ordem-servico.model';
import { StatusBadgeComponent } from '../../../../shared/components/status-badge/status-badge.component';

@Component({
  selector: 'app-os-card',
  standalone: true,
  imports: [CommonModule, RouterLink, StatusBadgeComponent],
  template: `
    <div class="os-card">
      <div class="os-card-header">
        <span class="os-number">#{{ os.numeroOS }}</span>
        <app-status-badge [status]="os.status"></app-status-badge>
      </div>

      <div class="os-body">
        <h3 class="os-vehicle">{{ os.modeloVeiculo }}</h3>
        <div class="os-client">
          <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/></svg>
          {{ os.clienteNome }}
        </div>
        <div class="os-placa-row">
          Placa: <span class="mono-badge">{{ os.placaVeiculo }}</span>
        </div>
      </div>

      <!-- Resumo de Itens se houver -->
      @if (os.itensServico && os.itensServico.length > 0) {
        <div class="os-details-list">
          🔧 {{ getConcluidosCount() }} de {{ os.itensServico.length }} serviços concluídos
        </div>
      }

      <div class="os-footer">
        <div class="total-price">
          <span class="price-label">Valor Total:</span>
          <span class="price-val">R$ {{ os.valorTotal | number:'1.2-2' }}</span>
        </div>
        <a [routerLink]="['/ordens-servico', os.id]" class="btn btn-secondary btn-sm">Workbench &rarr;</a>
      </div>
    </div>
  `,
  styles: [`
    .os-card {
      background: #18181C;
      border: 1px solid rgba(255, 255, 255, 0.08);
      border-radius: 12px;
      padding: 1.25rem;
      display: flex;
      flex-direction: column;
      gap: 1rem;
      transition: all 0.25s ease;
    }
    .os-card:hover {
      border-color: #ED145B;
      box-shadow: 0 8px 24px -4px rgba(237, 20, 91, 0.25);
    }
    .os-card-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
    }
    .os-number {
      font-family: 'JetBrains Mono', monospace;
      font-weight: 700;
      font-size: 0.9rem;
      color: #ED145B;
    }
    .os-vehicle {
      font-family: 'Outfit', sans-serif;
      font-size: 1.1rem;
      font-weight: 700;
      color: #F8FAFC;
    }
    .os-client {
      font-size: 0.85rem;
      color: #A1A1AA;
      display: flex;
      align-items: center;
      gap: 0.4rem;
      margin-top: 0.2rem;
    }
    .os-placa-row {
      font-size: 0.85rem;
      color: #71717A;
      margin-top: 0.4rem;
    }
    .os-details-list {
      background: rgba(10, 10, 12, 0.6);
      border-radius: 8px;
      padding: 0.5rem 0.75rem;
      font-size: 0.8rem;
      color: #A1A1AA;
    }
    .os-footer {
      display: flex;
      justify-content: space-between;
      align-items: center;
      border-top: 1px solid rgba(255, 255, 255, 0.08);
      padding-top: 0.75rem;
      margin-top: 0.25rem;
    }
    .price-label { font-size: 0.75rem; color: #A1A1AA; display: block; }
    .price-val { font-family: 'JetBrains Mono', monospace; font-weight: 700; color: #10B981; font-size: 1rem; }
    .btn-sm { padding: 0.4rem 0.85rem; font-size: 0.8rem; }
  `]
})
export class OsCardComponent {
  @Input() os!: OrdemServico;

  getConcluidosCount(): number {
    return this.os.itensServico ? this.os.itensServico.filter(i => i.concluido).length : 0;
  }
}
