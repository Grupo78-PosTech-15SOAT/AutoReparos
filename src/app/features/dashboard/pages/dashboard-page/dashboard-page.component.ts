import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { DashboardService, DashboardMetrics } from '../../services/dashboard.service';
import { OrdemServicoService } from '../../../ordens-servico/services/ordem-servico.service';
import { InsumoService } from '../../../insumos/services/insumo.service';
import { OrdemServico } from '../../../ordens-servico/models/ordem-servico.model';
import { Insumo } from '../../../insumos/models/insumo.model';
import { StatusBadgeComponent } from '../../../../shared/components/status-badge/status-badge.component';

@Component({
  selector: 'app-dashboard-page',
  standalone: true,
  imports: [CommonModule, RouterLink, StatusBadgeComponent],
  template: `
    <div class="container fade-in">
      <div class="page-header">
        <div>
          <h1 class="page-title">
            <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="#ED145B" stroke-width="2.3"><rect x="3" y="3" width="7" height="7"/><rect x="14" y="3" width="7" height="7"/><rect x="14" y="14" width="7" height="7"/><rect x="3" y="14" width="7" height="7"/></svg>
            Dashboard Gerencial da Oficina
          </h1>
          <p class="page-subtitle">Indicadores operacionais, faturamento estimado e alertas de estoque.</p>
        </div>
      </div>

      <!-- Grid de Cards KPI -->
      <div class="kpi-grid">
        <div class="kpi-card">
          <div class="kpi-header">
            <span class="kpi-title">Faturamento Estimado</span>
            <div class="kpi-icon green">
              <svg xmlns="http://www.w3.org/2000/svg" width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="12" y1="1" x2="12" y2="23"/><path d="M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6"/></svg>
            </div>
          </div>
          <div class="kpi-value green-val">R$ {{ faturamentoTotal | number:'1.2-2' }}</div>
          <div class="kpi-desc">Total acumulado de OSs em aberto e concluídas</div>
        </div>

        <div class="kpi-card">
          <div class="kpi-header">
            <span class="kpi-title">OSs em Execução</span>
            <div class="kpi-icon orange">
              <svg xmlns="http://www.w3.org/2000/svg" width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M14.7 6.3a1 1 0 0 0 0 1.4l1.6 1.6a1 1 0 0 0 1.4 0l3.77-3.77a6 6 0 0 1-7.94 7.94l-6.91 6.91a2.12 2.12 0 0 1-3-3l6.91-6.91a6 6 0 0 1 7.94-7.94l-3.76 3.76z"/></svg>
            </div>
          </div>
          <div class="kpi-value orange-val">{{ ordensExecucao.length }} OS(s)</div>
          <div class="kpi-desc">Veículos no elevador em reparo ativo</div>
        </div>

        <div class="kpi-card">
          <div class="kpi-header">
            <span class="kpi-title">Total de Ordens Registradas</span>
            <div class="kpi-icon blue">
              <svg xmlns="http://www.w3.org/2000/svg" width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/></svg>
            </div>
          </div>
          <div class="kpi-value">{{ ultimasOrdens.length }} OS(s)</div>
          <div class="kpi-desc">Histórico completo de atendimentos</div>
        </div>

        <div class="kpi-card">
          <div class="kpi-header">
            <span class="kpi-title">Alertas de Estoque Mínimo</span>
            <div class="kpi-icon red">
              <svg xmlns="http://www.w3.org/2000/svg" width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"/><line x1="12" y1="9" x2="12" y2="13"/><line x1="12" y1="17" x2="12.01" y2="17"/></svg>
            </div>
          </div>
          <div class="kpi-value red-val">{{ insumosCriticos.length }} Peça(s)</div>
          <div class="kpi-desc">Insumos precisando de reposição imediata</div>
        </div>
      </div>

      <!-- 2 Colunas: Atividades Recentes vs Alertas de Estoque -->
      <div class="dashboard-grid" style="margin-top: 2rem;">
        <!-- Ordens de Serviço Recentes -->
        <div class="card-panel">
          <h3 style="font-family: 'Outfit', sans-serif; font-size: 1.15rem; font-weight: 700; color: #fff; margin-bottom: 1rem;">
            📋 Ordens de Serviço Recentes
          </h3>
          <div class="data-table-container">
            <table class="data-table">
              <thead>
                <tr>
                  <th>OS #</th>
                  <th>Veículo</th>
                  <th>Status</th>
                  <th>Valor</th>
                </tr>
              </thead>
              <tbody>
                @for (os of ultimasOrdens.slice(0, 5); track os.id) {
                  <tr>
                    <td><span class="mono-badge" style="color: #ED145B;">#{{ os.numeroOS }}</span></td>
                    <td>{{ os.modeloVeiculo }} ({{ os.placaVeiculo }})</td>
                    <td><app-status-badge [status]="os.status"></app-status-badge></td>
                    <td style="font-family: 'JetBrains Mono', monospace; font-weight: 700;">R$ {{ os.valorTotal | number:'1.2-2' }}</td>
                  </tr>
                } @empty {
                  <tr><td colspan="4" style="text-align: center; color: #71717A;">Nenhuma OS registrada.</td></tr>
                }
              </tbody>
            </table>
          </div>
        </div>

        <!-- Alertas Críticos de Estoque -->
        <div class="card-panel">
          <h3 style="font-family: 'Outfit', sans-serif; font-size: 1.15rem; font-weight: 700; color: #EF4444; margin-bottom: 1rem; display: flex; align-items: center; gap: 0.5rem;">
            ⚠️ Alertas de Estoque Crítico
          </h3>
          <div style="display: flex; flex-direction: column; gap: 0.75rem;">
            @for (item of insumosCriticos; track item.id) {
              <div class="stock-alert-item">
                <div>
                  <div style="font-weight: 600; color: #F8FAFC;">{{ item.nome }}</div>
                  <div style="font-size: 0.8rem; color: #A1A1AA;">Qtd Atual: <strong style="color: #EF4444;">{{ item.quantidadeEstoque }} un.</strong> | Mínimo: {{ item.quantidadeMinima }} un.</div>
                </div>
                <a routerLink="/insumos" class="btn btn-secondary btn-sm">Repor</a>
              </div>
            } @empty {
              <div style="text-align: center; padding: 2rem; color: #10B981; border: 1px dashed rgba(16, 185, 129, 0.3); border-radius: 8px;">
                ✓ Todos os insumos estão acima do estoque mínimo!
              </div>
            }
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .kpi-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
      gap: 1.25rem;
    }
    .kpi-card {
      background: rgba(24, 24, 28, 0.85);
      backdrop-filter: blur(12px);
      border: 1px solid rgba(255, 255, 255, 0.08);
      border-radius: 12px;
      padding: 1.5rem;
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
    }
    .kpi-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
    }
    .kpi-title { font-size: 0.85rem; color: #A1A1AA; font-weight: 600; text-transform: uppercase; letter-spacing: 0.05em; }
    .kpi-icon { width: 40px; height: 40px; border-radius: 10px; display: flex; align-items: center; justify-content: center; }
    .kpi-icon.green { background: rgba(16, 185, 129, 0.15); color: #10B981; }
    .kpi-icon.orange { background: rgba(249, 115, 22, 0.15); color: #F97316; }
    .kpi-icon.blue { background: rgba(59, 130, 246, 0.15); color: #3B82F6; }
    .kpi-icon.red { background: rgba(239, 68, 68, 0.15); color: #EF4444; }

    .kpi-value { font-family: 'Outfit', sans-serif; font-size: 1.8rem; font-weight: 800; color: #ffffff; }
    .green-val { color: #10B981; }
    .orange-val { color: #F97316; }
    .red-val { color: #EF4444; }
    .kpi-desc { font-size: 0.75rem; color: #71717A; }

    .dashboard-grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 1.5rem;
    }
    @media (max-width: 900px) {
      .dashboard-grid { grid-template-columns: 1fr; }
    }
    .stock-alert-item {
      background: rgba(239, 68, 68, 0.06);
      border: 1px solid rgba(239, 68, 68, 0.2);
      border-radius: 8px;
      padding: 0.85rem 1rem;
      display: flex;
      justify-content: space-between;
      align-items: center;
    }
    .btn-sm { padding: 0.35rem 0.65rem; font-size: 0.8rem; }
  `]
})
export class DashboardPageComponent implements OnInit {
  ultimasOrdens: OrdemServico[] = [];
  ordensExecucao: OrdemServico[] = [];
  insumosCriticos: Insumo[] = [];
  faturamentoTotal = 0;

  private osService = inject(OrdemServicoService);
  private insumoService = inject(InsumoService);

  ngOnInit() {
    this.carregar();
  }

  carregar() {
    this.osService.getAll().subscribe(lista => {
      this.ultimasOrdens = lista;
      this.ordensExecucao = lista.filter(x => x.status === 4);
      this.faturamentoTotal = lista.reduce((acc, item) => acc + item.valorTotal, 0);
    });

    this.insumoService.getAll().subscribe(insumos => {
      this.insumosCriticos = insumos.filter(i => i.quantidadeEstoque <= i.quantidadeMinima);
    });
  }
}
