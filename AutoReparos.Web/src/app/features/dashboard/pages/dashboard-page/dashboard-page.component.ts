import { Component, OnInit, OnDestroy, inject, ChangeDetectorRef, ChangeDetectionStrategy, viewChild, ElementRef, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DecimalPipe } from '@angular/common';
import { RouterModule } from '@angular/router';
import { DashboardService, DashboardOrdemServico, DashboardInsumoCritico, DashboardMensalStatus } from '../../services/dashboard.service';
import { StatusBadgeComponent } from '../../../../shared/components/status-badge/status-badge.component';
import { PageContainerComponent } from '../../../../shared/components/page-container/page-container.component';
import { IdBadgeComponent } from '../../../../shared/components/id-badge/id-badge.component';
import { PlacaBadgeComponent } from '../../../../shared/components/placa-badge/placa-badge.component';
import type { Chart } from 'chart.js';

@Component({
  selector: 'app-dashboard-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    DecimalPipe,
    RouterModule, 
    StatusBadgeComponent,
    PageContainerComponent,
    IdBadgeComponent,
    PlacaBadgeComponent
  ],
  template: `
    <app-page-container>
      <div class="page-header-row">
        <div>
          <h1 class="page-title">
            <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="#ED145B" stroke-width="2.3"><rect x="3" y="3" width="7" height="7"/><rect x="14" y="3" width="7" height="7"/><rect x="14" y="14" width="7" height="7"/><rect x="3" y="14" width="7" height="7"/></svg>
            Dashboard
          </h1>
        </div>
      </div>

      <!-- Grid de Cards KPI -->
      <div class="kpi-grid">
        <div class="kpi-card">
          <div class="kpi-header">
            <span class="kpi-title">Faturamento</span>
            <div class="kpi-icon green">
              <svg xmlns="http://www.w3.org/2000/svg" width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="12" y1="1" x2="12" y2="23"/><path d="M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6"/></svg>
            </div>
          </div>
          <div class="kpi-value green-val" style="font-size: 1.5rem; margin-top: 0.25rem;">
            R$ {{ faturamentoMesAtual | number:'1.2-2' }}
          </div>
          <div class="kpi-desc">
            Mês Anterior: <strong style="color: #A1A1AA;">R$ {{ faturamentoMesAnterior | number:'1.2-2' }}</strong>
          </div>
        </div>

        <div class="kpi-card">
          <div class="kpi-header">
            <span class="kpi-title">Em Execução</span>
            <div class="kpi-icon orange">
              <svg xmlns="http://www.w3.org/2000/svg" width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M14.7 6.3a1 1 0 0 0 0 1.4l1.6 1.6a1 1 0 0 0 1.4 0l3.77-3.77a6 6 0 0 1-7.94 7.94l-6.91 6.91a2.12 2.12 0 0 1-3-3l6.91-6.91a6 6 0 0 1 7.94-7.94l-3.76 3.76z"/></svg>
            </div>
          </div>
          <div class="kpi-value orange-val">{{ ordensExecucao }} Ordens</div>
        </div>

        <div class="kpi-card">
          <div class="kpi-header">
            <span class="kpi-title">Total OSs (Mês)</span>
            <div class="kpi-icon blue">
              <svg xmlns="http://www.w3.org/2000/svg" width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/></svg>
            </div>
          </div>
          <div class="kpi-value">{{ totalOrdensMesAtual }} Ordens</div>
        </div>

        <div class="kpi-card">
          <div class="kpi-header">
            <span class="kpi-title">Serviços Concluídos</span>
            <div class="kpi-icon purple">
              <svg xmlns="http://www.w3.org/2000/svg" width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"></path><polyline points="22 4 12 14.01 9 11.01"></polyline></svg>
            </div>
          </div>
          <div class="kpi-value purple-val">{{ totalServicosConcluidosMes() }} Realizados</div>
        </div>
      </div>

      <!-- Chart Panel -->
      <div class="card-panel" style="margin-top: 1.5rem;">
        <h3 style="font-family: 'Outfit', sans-serif; font-size: 1.15rem; font-weight: 700; color: #fff; margin-bottom: 1rem; display: flex; align-items: center; gap: 0.5rem;">
          <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="18" y1="20" x2="18" y2="10"/><line x1="12" y1="20" x2="12" y2="4"/><line x1="6" y1="20" x2="6" y2="14"/></svg>
          Histórico Mensal
        </h3>
        <div style="height: 300px;">
          <canvas #chartCanvas style="max-height: 300px; width: 100%;"></canvas>
        </div>
      </div>

      <!-- 2 Colunas: Atividades Recentes vs Alertas de Estoque -->
      <div class="dashboard-grid" style="margin-top: 2rem;">
        <!-- Ordens de Serviço Recentes -->
        <div class="card-panel">
          <h3 style="font-family: 'Outfit', sans-serif; font-size: 1.15rem; font-weight: 700; color: #fff; margin-bottom: 1rem; display: flex; align-items: center; gap: 0.5rem;">
            <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/></svg>
            Ordens de Serviço Recentes
          </h3>
          <div class="data-table-container table-loading-container">
            @if (loading) {
              <div class="table-loading-overlay">
                <div class="table-loading-spinner"></div>
                <span class="table-loading-text">Carregando...</span>
              </div>
            }
            <table class="data-table">
              <thead>
                <tr>
                  <th>OS</th>
                  <th>Veículo</th>
                  <th>Status</th>
                  <th style="text-align: center;">Total</th>
                </tr>
              </thead>
              <tbody>
                @for (os of ultimasOrdens.slice(0, 5); track os.id) {
                  <tr>
                    <td><app-id-badge [text]="'#' + os.id.slice(0, 8).toUpperCase()"></app-id-badge></td>
                    <td><app-placa-badge [placa]="os.placaVeiculo"></app-placa-badge> {{ os.modeloVeiculo }}</td>
                    <td><app-status-badge [status]="os.status"></app-status-badge></td>
                    <td style="font-family: 'JetBrains Mono', monospace; font-weight: 700; text-align: center;">R$ {{ os.valorTotal | number:'1.2-2' }}</td>
                  </tr>
                } @empty {
                  <tr>
                    <td colspan="4" style="text-align: center; color: #71717A; padding: 2rem;">Nenhuma OS recente cadastrada</td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
        </div>

        <!-- Alertas Críticos de Estoque -->
        <div class="card-panel table-loading-container">
          @if (loading) {
            <div class="table-loading-overlay">
              <div class="table-loading-spinner"></div>
              <span class="table-loading-text">Carregando...</span>
            </div>
          }
          <h3 style="font-family: 'Outfit', sans-serif; font-size: 1.15rem; font-weight: 700; color: #EF4444; margin-bottom: 1rem; display: flex; align-items: center; gap: 0.5rem;">
            <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="m21.73 18-8-14a2 2 0 0 0-3.48 0l-8 14A2 2 0 0 0 4 21h16a2 2 0 0 0 1.73-3Z"/><line x1="12" y1="9" x2="12" y2="13"/><line x1="12" y1="17" x2="12.01" y2="17"/></svg>
            Alertas de Estoque Crítico ({{ insumosCriticos.length }})
          </h3>
          <div style="display: flex; flex-direction: column; gap: 0.75rem;">
            @for (item of insumosCriticos; track item.id) {
              <div class="stock-alert-item">
                <div>
                  <div style="font-weight: 600; color: #F8FAFC;">{{ item.nome }}</div>
                  <div style="font-size: 0.8rem; color: #A1A1AA;">Qtd Atual: <strong style="color: #EF4444;">{{ item.quantidadeEstoque }} un.</strong> | Mínimo: 5 un.</div>
                </div>
                <a routerLink="/insumos" class="btn btn-secondary btn-sm">Repor</a>
              </div>
            } @empty {
              <div style="text-align: center; padding: 2rem; color: #10B981; border: 1px dashed rgba(16, 185, 129, 0.3); border-radius: 8px; display: flex; flex-direction: column; align-items: center; gap: 0.5rem;">
                <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"></path><polyline points="22 4 12 14.01 9 11.01"></polyline></svg>
                <span>Todos os insumos estão acima do estoque mínimo!</span>
              </div>
            }
          </div>
        </div>
      </div>
    </app-page-container>
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
    .kpi-icon.purple { background: rgba(139, 92, 246, 0.15); color: #8B5CF6; }

    .kpi-value { font-family: 'Outfit', sans-serif; font-size: 1.8rem; font-weight: 800; color: #ffffff; }
    .green-val { color: #10B981; }
    .orange-val { color: #F97316; }
    .red-val { color: #EF4444; }
    .purple-val { color: #8B5CF6; }
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
    .data-table th, .data-table td {
      white-space: nowrap;
    }
  `]
})
export class DashboardPageComponent implements OnInit, OnDestroy {
  ultimasOrdens: DashboardOrdemServico[] = [];
  ordensExecucao = 0;
  insumosCriticos: DashboardInsumoCritico[] = [];
  faturamentoMesAtual = 0;
  faturamentoMesAnterior = 0;
  totalOrdensMesAtual = 0;
  historicoMensal: DashboardMensalStatus[] = [];
  loading = false;
  chart?: Chart;
  chartCanvas = viewChild<ElementRef<HTMLCanvasElement>>('chartCanvas');
  
  private readonly destroyRef = inject(DestroyRef);
  private readonly dashboardService = inject(DashboardService);
  private readonly cdr = inject(ChangeDetectorRef);

  ngOnInit() {
    this.carregar();
  }

  ngOnDestroy() {
    if (this.chart) {
      this.chart.destroy();
    }
  }

  totalServicosConcluidosMes(): number {
    if (!this.historicoMensal || this.historicoMensal.length === 0) return 0;
    return this.historicoMensal.at(-1)?.totalServicosRealizados ?? 0;
  }

  carregar() {
    this.loading = true;
    this.dashboardService.getMetrics().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: metrics => {
        this.ultimasOrdens = metrics.ultimasOrdens;
        this.ordensExecucao = metrics.ordensEmExecucao;
        this.insumosCriticos = metrics.insumosCriticos;
        this.faturamentoMesAtual = metrics.faturamentoMesAtual;
        this.faturamentoMesAnterior = metrics.faturamentoMesAnterior;
        this.totalOrdensMesAtual = metrics.totalOrdensMesAtual;
        this.historicoMensal = metrics.historicoMensal;
        this.loading = false;
        this.cdr.detectChanges();
        this.renderChart();
      },
      error: () => {
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  async renderChart() {
    const { Chart } = await import('chart.js/auto');
    const canvasRef = this.chartCanvas();
    if (!canvasRef) return;
    const canvas = canvasRef.nativeElement;

    if (this.chart) {
      this.chart.destroy();
    }

    this.chart = new Chart(canvas, {
      type: 'line',
      data: {
        labels: this.historicoMensal.map(h => h.mes),
        datasets: [
          {
            label: 'Total de OS',
            data: this.historicoMensal.map(h => h.totalOrdens),
            borderColor: '#3B82F6',
            backgroundColor: 'rgba(59, 130, 246, 0.1)',
            yAxisID: 'y',
            tension: 0.3,
            pointRadius: 4,
            pointHoverRadius: 6,
            pointBackgroundColor: 'transparent',
            pointBorderWidth: 2
          },
          {
            label: 'Faturamento (R$)',
            data: this.historicoMensal.map(h => h.totalFaturado),
            borderColor: '#10B981',
            backgroundColor: 'rgba(16, 185, 129, 0.1)',
            yAxisID: 'y1',
            tension: 0.3,
            pointRadius: 4,
            pointHoverRadius: 6,
            pointBackgroundColor: 'transparent',
            pointBorderWidth: 2
          },
          {
            label: 'Serviços Concluídos',
            data: this.historicoMensal.map(h => h.totalServicosRealizados),
            borderColor: '#F97316',
            backgroundColor: 'rgba(249, 115, 22, 0.1)',
            yAxisID: 'y',
            tension: 0.3,
            pointRadius: 4,
            pointHoverRadius: 6,
            pointBackgroundColor: 'transparent',
            pointBorderWidth: 2
          }
        ]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        interaction: {
          mode: 'index',
          intersect: false,
        },
        plugins: {
          legend: {
            display: true,
            position: 'top',
            labels: {
              color: '#F8FAFC',
              font: { family: 'Outfit, sans-serif', size: 14, weight: 'bold' },
              padding: 20,
              boxWidth: 20,
              boxHeight: 12
            }
          },
          tooltip: {
            backgroundColor: 'rgba(15, 23, 42, 0.98)',
            titleColor: '#FFFFFF',
            titleFont: { family: 'Outfit, sans-serif', size: 14, weight: 'bold' },
            bodyColor: '#FFFFFF',
            bodyFont: { family: 'Outfit, sans-serif', size: 13, weight: 'normal' },
            borderColor: 'rgba(255, 255, 255, 0.2)',
            borderWidth: 1.5,
            padding: 14,
            displayColors: true,
            boxWidth: 8,
            boxHeight: 8,
            boxPadding: 8,
            usePointStyle: true,
            cornerRadius: 8,
            callbacks: {
              labelTextColor: (context: any) => {
                return context.dataset.borderColor || '#FFFFFF';
              }
            }
          }
        },
        scales: {
          y: {
            type: 'linear',
            display: true,
            position: 'left',
            grid: { color: 'rgba(255, 255, 255, 0.05)' },
            ticks: { color: '#A1A1AA', font: { family: 'Outfit', size: 11 } }
          },
          y1: {
            type: 'linear',
            display: true,
            position: 'right',
            grid: {
              drawOnChartArea: false,
            },
            ticks: { color: '#A1A1AA', font: { family: 'Outfit', size: 11 } }
          },
          x: {
            grid: { color: 'rgba(255, 255, 255, 0.05)' },
            ticks: { color: '#A1A1AA', font: { family: 'Outfit', size: 11 } }
          }
        }
      }
    });
  }
}
