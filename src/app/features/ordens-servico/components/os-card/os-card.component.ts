import { Component, input, computed, ChangeDetectionStrategy } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { OrdemServico } from '../../models/ordem-servico.model';

@Component({
  selector: 'app-os-card',
  standalone: true,
  imports: [DecimalPipe, RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <a [routerLink]="['/ordens-servico', os().id]" class="os-card">
      <div class="os-card-header">
        <h3 class="os-vehicle" [title]="os().modeloVeiculo">{{ os().modeloVeiculo }}</h3>
      </div>

      <div class="os-body">
        <div class="os-meta-row">
          <div class="os-client" [title]="os().clienteNome">
            <svg xmlns="http://www.w3.org/2000/svg" width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/></svg>
            <span class="os-client-name">{{ os().clienteNome }}</span>
          </div>
          <span class="badge-placa">{{ os().placaVeiculo }}</span>
        </div>

        @if (os().itensServico && os().itensServico.length > 0) {
          <div class="os-progress-container" [title]="concluidosCount() + ' de ' + os().itensServico.length + ' serviços concluídos'">
            <div class="os-progress-track">
              <div class="os-progress-fill" [style.width.%]="progressPercent()"></div>
            </div>
            <span class="os-progress-label">{{ concluidosCount() }}/{{ os().itensServico.length }} serviços</span>
          </div>
        }
      </div>

      <div class="os-footer">
        <span class="price-val">R$ {{ os().valorTotal | number:'1.2-2' }}</span>
      </div>
    </a>
  `,
  styles: [`
    .os-card {
      background: #1E1E24;
      border: 1px solid rgba(255, 255, 255, 0.03);
      border-radius: 8px;
      padding: 0.85rem 0.95rem;
      display: flex;
      flex-direction: column;
      gap: 0.6rem;
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.25);
      text-decoration: none;
      cursor: pointer;
      transition: all 0.2s cubic-bezier(0.16, 1, 0.3, 1);
    }
    .os-card:hover {
      border-color: rgba(255, 255, 255, 0.1);
      box-shadow: 0 6px 20px rgba(0, 0, 0, 0.4);
      transform: translateY(-2px);
    }
    .os-card-header {
      display: flex;
      justify-content: space-between;
      align-items: baseline;
      gap: 0.5rem;
    }
    .os-vehicle {
      font-family: 'Outfit', sans-serif;
      font-size: 1rem;
      font-weight: 700;
      color: #F8FAFC;
      margin: 0;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
      max-width: 160px;
      flex: 1;
    }
    .os-number {
      font-family: 'JetBrains Mono', monospace;
      font-weight: 500;
      font-size: 0.7rem;
      color: rgba(113, 113, 122, 0.55);
    }
    .os-body {
      display: flex;
      flex-direction: column;
      gap: 0.45rem;
    }
    .os-meta-row {
      display: flex;
      justify-content: space-between;
      align-items: center;
      gap: 0.5rem;
    }
    .os-client {
      font-size: 0.8rem;
      color: #A1A1AA;
      display: flex;
      align-items: center;
      gap: 0.35rem;
      min-width: 0;
      flex: 1;
    }
    .os-client-name {
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }
    .badge-placa {
      font-size: 0.65rem;
      padding: 0.08rem 0.3rem;
      background: rgba(255, 255, 255, 0.06);
      color: #94A3B8;
      border-radius: 4px;
      font-family: 'JetBrains Mono', monospace;
      white-space: nowrap;
    }
    .os-progress-container {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      margin-top: 0.1rem;
    }
    .os-progress-track {
      background: rgba(255, 255, 255, 0.05);
      border-radius: 2.5px;
      height: 7px;
      flex: 1;
      position: relative;
      overflow: hidden;
    }
    .os-progress-fill {
      background: #10B981;
      height: 100%;
      position: absolute;
      left: 0;
      top: 0;
      border-radius: 2.5px;
    }
    .os-progress-label {
      font-size: 0.65rem;
      color: #71717A;
      font-weight: 600;
      white-space: nowrap;
    }
    .os-footer {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-top: 0.2rem;
    }
    .price-val {
      font-family: 'JetBrains Mono', monospace;
      font-weight: 700;
      color: #10B981;
      font-size: 0.95rem;
    }
  `]
})
export class OsCardComponent {
  os = input.required<OrdemServico>();

  concluidosCount = computed(() => {
    const data = this.os();
    return data.itensServico ? data.itensServico.filter(i => i.concluido).length : 0;
  });

  progressPercent = computed(() => {
    const data = this.os();
    if (!data.itensServico || data.itensServico.length === 0) return 0;
    return (this.concluidosCount() / data.itensServico.length) * 100;
  });
}
