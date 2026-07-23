import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { OrdemServicoService } from '../../services/ordem-servico.service';
import { OrdemServico, StatusOS } from '../../models/ordem-servico.model';
import { ClienteService } from '../../../clientes/services/cliente.service';
import { VeiculoService } from '../../../veiculos/services/veiculo.service';
import { StatusBadgeComponent } from '../../../../shared/components/status-badge/status-badge.component';
import { NotificationService } from '../../../../core/ui/notification.service';
import { PaginationComponent } from '../../../../shared/components/pagination/pagination.component';
import { CustomSelectComponent, SelectOption } from '../../../../shared/components/custom-select/custom-select.component';

@Component({
  selector: 'app-os-lista-page',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule, StatusBadgeComponent, PaginationComponent, CustomSelectComponent],
  template: `
    <div class="container fade-in">
      <div class="page-header">
        <div>
          <h1 class="page-title">
            <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="#ED145B" stroke-width="2.3"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/><line x1="16" y1="13" x2="8" y2="13"/><line x1="16" y1="17" x2="8" y2="17"/></svg>
            Ordens de Serviço
          </h1>
        </div>
        <a routerLink="/ordens-servico/nova" class="btn btn-primary">
          + Nova OS
        </a>
      </div>

      <!-- Filtros de Busca -->
      <div class="card-panel" style="margin-bottom: 1.5rem;">
        <div style="display: flex; gap: 1rem; flex-wrap: wrap; align-items: flex-end;">
          <input type="text" [(ngModel)]="filtroTermo" (input)="filtrar()" placeholder="Buscar cliente, placa ou OS..." class="form-control" style="flex: 1; min-width: 260px;" />
          <div style="width: 210px;">
            <app-custom-select
              [options]="statusOptions"
              [value]="filtroStatusStr"
              placeholder="Todos os Status"
              (valueChange)="onFiltroStatusChange($event)"
            ></app-custom-select>
          </div>
        </div>
      </div>

      <!-- Tabela de OSs -->
      <div class="data-table-container table-loading-container">
        @if (loading) {
          <div class="table-loading-overlay">
            <div class="table-loading-spinner"></div>
            <span class="table-loading-text">Carregando dados...</span>
          </div>
        }
        <table class="data-table">
          <thead>
            <tr>
              <th>OS #</th>
              <th>Cliente</th>
              <th>Veículo</th>
              <th>Status</th>
              <th>Abertura</th>
              <th style="text-align: center;">Valor</th>
              <th>Ações</th>
            </tr>
          </thead>
          <tbody>
            @for (os of ordensFiltradas; track os.id) {
              <tr>
                <td>
                  <span class="mono-badge" style="color: #ED145B;">#{{ os.id.substring(0, 8) }}</span>
                </td>
                <td>
                  <div style="font-weight: 600;">{{ getClienteNome(os.clienteId) }}</div>
                </td>
                <td>
                  <div>{{ getVeiculoDesc(os.veiculoId) }}</div>
                  <span class="mono-badge" style="font-size: 0.75rem;">{{ getVeiculoPlaca(os.veiculoId) }}</span>
                </td>
                <td>
                  <app-status-badge [status]="os.status"></app-status-badge>
                </td>
                <td>{{ os.dataAbertura | date:'dd/MM/yy HH:mm' }}</td>
                <td style="text-align: center;">
                  <span style="font-family: 'JetBrains Mono', monospace; font-weight: 700; color: #10B981;">
                    R$ {{ os.valorTotal | number:'1.2-2' }}
                  </span>
                </td>
                <td>
                  <div style="display: inline-flex; gap: 0.5rem;">
                    <a [routerLink]="['/ordens-servico', os.id]" class="btn btn-secondary btn-sm" title="Abrir OS">🔍 OS</a>
                    @if (os.status === StatusOS.Finalizada) {
                      <button (click)="entregar(os.id)" class="btn btn-success btn-sm" title="Entregar Veículo">✅ Entregar</button>
                    }
                  </div>
                </td>
              </tr>
            } @empty {
              <tr>
                <td colspan="7" style="text-align: center; padding: 2.5rem; color: #71717A;">
                  Nenhuma OS encontrada.
                </td>
              </tr>
            }
          </tbody>
        </table>
      </div>

      <!-- Paginação Estruturada -->
      <app-pagination
        [pageNumber]="pageNumber"
        [pageSize]="pageSize"
        [totalItems]="totalItems"
        [totalPages]="totalPages"
        [pageSizeOptions]="[5, 10, 20, 50]"
        (pageChange)="onPageChange($event)"
        (pageSizeChange)="onPageSizeChange($event)">
      </app-pagination>
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
  filtroStatusStr = '0';

  statusOptions: SelectOption[] = [
    { value: '0', label: 'Todos os Status' },
    { value: '1', label: '1. Recebida' },
    { value: '2', label: '2. Diagnóstico' },
    { value: '3', label: '3. Aprovação' },
    { value: '4', label: '4. Execução' },
    { value: '5', label: '5. Finalizada' },
    { value: '6', label: '6. Entregue' },
  ];

  pageNumber = 1;
  pageSize = 10;
  totalItems = 0;
  totalPages = 1;

  loading = false;
  clientes: any[] = [];
  veiculos: any[] = [];
  private osService = inject(OrdemServicoService);
  private clienteService = inject(ClienteService);
  private veiculoService = inject(VeiculoService);
  private notification = inject(NotificationService);
  private cdr = inject(ChangeDetectorRef);

  ngOnInit() {
    this.carregarOrdens();
  }

  carregarOrdens() {
    this.loading = true;
    
    this.clienteService.getAll(1, 500).subscribe(c => {
      this.clientes = c.items || [];
      this.cdr.detectChanges();
    });
    this.veiculoService.getAll(1, 500).subscribe(v => {
      this.veiculos = v.items || [];
      this.cdr.detectChanges();
    });

    this.osService.getAll(this.pageNumber, this.pageSize).subscribe({
      next: (res) => {
        this.todasOrdens = res.items || [];
        this.totalItems = res.total;
        this.totalPages = res.totalPages;
        this.filtrar();
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  onPageChange(page: number) {
    this.pageNumber = page;
    this.carregarOrdens();
  }

  onPageSizeChange(size: number) {
    this.pageSize = size;
    this.pageNumber = 1;
    this.carregarOrdens();
  }

  onFiltroStatusChange(val: string) {
    this.filtroStatusStr = val;
    this.filtroStatus = Number(val);
    this.filtrar();
  }

  filtrar() {
    const termo = this.filtroTermo.toLowerCase().trim();
    const st = Number(this.filtroStatus);

    this.ordensFiltradas = this.todasOrdens.filter(os => {
      const cNome = this.getClienteNome(os.clienteId).toLowerCase();
      const vPlaca = this.getVeiculoPlaca(os.veiculoId).toLowerCase();
      const vDesc = this.getVeiculoDesc(os.veiculoId).toLowerCase();
      
      const matchTermo = !termo ||
        os.id.toLowerCase().includes(termo) ||
        cNome.includes(termo) ||
        vPlaca.includes(termo) ||
        vDesc.includes(termo);

      const matchStatus = st === 0 || os.status === st;

      return matchTermo && matchStatus;
    });
  }

  getClienteNome(id: string): string {
    return this.clientes.find(c => c.id === id)?.nome || 'Carregando...';
  }

  getVeiculoDesc(id: string): string {
    const v = this.veiculos.find(v => v.id === id);
    return v ? `${v.marca} ${v.modelo}` : 'Carregando...';
  }
  
  getVeiculoPlaca(id: string): string {
    const v = this.veiculos.find(v => v.id === id);
    return v ? v.placa : '---';
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
