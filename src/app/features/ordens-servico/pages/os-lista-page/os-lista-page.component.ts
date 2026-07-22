import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { OrdemServicoService } from '../../services/ordem-servico.service';
import { OrdemServico, StatusOS } from '../../models/ordem-servico.model';
import { StatusBadgeComponent } from '../../../../shared/components/status-badge/status-badge.component';
import { NotificationService } from '../../../../core/ui/notification.service';

@Component({
  selector: 'app-os-lista-page',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule, StatusBadgeComponent],
  template: `
    <div class="container fade-in">
      <div class="page-header">
        <div>
          <h1 class="page-title">
            <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="#ED145B" stroke-width="2.3"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/><line x1="16" y1="13" x2="8" y2="13"/><line x1="16" y1="17" x2="8" y2="17"/></svg>
            Gerenciamento de Ordens de Serviço
          </h1>
          <p class="page-subtitle">Listagem completa e ações operacionais da oficina.</p>
        </div>
        <a routerLink="/ordens-servico/nova" class="btn btn-primary">
          + Abrir Nova OS
        </a>
      </div>

      <!-- Filtros de Busca -->
      <div class="card-panel" style="margin-bottom: 1.5rem;">
        <div style="display: flex; gap: 1rem; flex-wrap: wrap;">
          <input type="text" [(ngModel)]="filtroTermo" (input)="filtrar()" placeholder="Buscar por cliente, placa ou número da OS..." class="form-control" style="flex: 1; min-width: 260px;" />
          <select [(ngModel)]="filtroStatus" (change)="filtrar()" class="form-control" style="width: 200px;">
            <option [value]="0">Todos os Status</option>
            <option [value]="1">1. Recebida</option>
            <option [value]="2">2. Em Diagnóstico</option>
            <option [value]="3">3. Aguardando Aprovação</option>
            <option [value]="4">4. Em Execução</option>
            <option [value]="5">5. Finalizada</option>
            <option [value]="6">6. Entregue</option>
          </select>
        </div>
      </div>

      <!-- Tabela de OSs -->
      <div class="data-table-container">
        <table class="data-table">
          <thead>
            <tr>
              <th>OS #</th>
              <th>Cliente</th>
              <th>Veículo & Placa</th>
              <th>Status</th>
              <th>Data Abertura</th>
              <th>Valor Total</th>
              <th style="text-align: right;">Ações</th>
            </tr>
          </thead>
          <tbody>
            @for (os of ordensFiltradas; track os.id) {
              <tr>
                <td>
                  <span class="mono-badge" style="color: #ED145B;">#{{ os.numeroOS }}</span>
                </td>
                <td>
                  <div style="font-weight: 600;">{{ os.clienteNome }}</div>
                </td>
                <td>
                  <div>{{ os.modeloVeiculo }}</div>
                  <span class="mono-badge" style="font-size: 0.75rem;">{{ os.placaVeiculo }}</span>
                </td>
                <td>
                  <app-status-badge [status]="os.status"></app-status-badge>
                </td>
                <td>{{ os.dataAbertura | date:'dd/MM/yyyy HH:mm' }}</td>
                <td>
                  <span style="font-family: 'JetBrains Mono', monospace; font-weight: 700; color: #10B981;">
                    R$ {{ os.valorTotal | number:'1.2-2' }}
                  </span>
                </td>
                <td style="text-align: right;">
                  <div style="display: inline-flex; gap: 0.5rem;">
                    <a [routerLink]="['/ordens-servico', os.id]" class="btn btn-secondary btn-sm">Workbench</a>
                    @if (os.status === StatusOS.Finalizada) {
                      <button (click)="entregar(os.id)" class="btn btn-success btn-sm">Entregar Veículo</button>
                    }
                  </div>
                </td>
              </tr>
            } @empty {
              <tr>
                <td colspan="7" style="text-align: center; padding: 2.5rem; color: #71717A;">
                  Nenhuma Ordem de Serviço encontrada com os filtros aplicados.
                </td>
              </tr>
            }
          </tbody>
        </table>
      </div>
    </div>
  `,
  styles: [`.btn-sm { padding: 0.35rem 0.75rem; font-size: 0.8rem; }`]
})
export class OsListaPageComponent implements OnInit {
  StatusOS = StatusOS;
  todasOrdens: OrdemServico[] = [];
  ordensFiltradas: OrdemServico[] = [];

  filtroTermo = '';
  filtroStatus = 0;

  private osService = inject(OrdemServicoService);
  private notification = inject(NotificationService);

  ngOnInit() {
    this.carregarOrdens();
  }

  carregarOrdens() {
    this.osService.getAll().subscribe({
      next: (lista) => {
        this.todasOrdens = lista;
        this.filtrar();
      }
    });
  }

  filtrar() {
    const termo = this.filtroTermo.toLowerCase().trim();
    const st = Number(this.filtroStatus);

    this.ordensFiltradas = this.todasOrdens.filter(os => {
      const matchTermo = !termo ||
        os.numeroOS.toLowerCase().includes(termo) ||
        os.clienteNome.toLowerCase().includes(termo) ||
        os.placaVeiculo.toLowerCase().includes(termo) ||
        os.modeloVeiculo.toLowerCase().includes(termo);

      const matchStatus = st === 0 || os.status === st;

      return matchTermo && matchStatus;
    });
  }

  entregar(osId: string) {
    if (confirm('Confirmar entrega do veículo e encerramento da Ordem de Serviço?')) {
      this.osService.entregarVeiculo(osId).subscribe({
        next: () => {
          this.notification.success('Veículo Entregue', 'Status da OS atualizado para Entregue.');
          this.carregarOrdens();
        }
      });
    }
  }
}
