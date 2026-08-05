import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {  Component, OnInit, inject, ChangeDetectorRef, ChangeDetectionStrategy , DestroyRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { VeiculoService } from '../../services/veiculo.service';
import { ClienteService } from '../../../clientes/services/cliente.service';
import { Veiculo } from '../../models/veiculo.model';
import { Cliente } from '../../../clientes/models/cliente.model';
import { PlacaPipe } from '../../../../shared/pipes/placa.pipe';
import { MaskDirective } from '../../../../shared/directives/mask.directive';
import { NotificationService } from '../../../../core/ui/notification.service';
import { PaginationComponent } from '../../../../shared/components/pagination/pagination.component';
import { CustomSelectComponent, SelectOption } from '../../../../shared/components/custom-select/custom-select.component';
import { PageContainerComponent } from '../../../../shared/components/page-container/page-container.component';
import { PlacaBadgeComponent } from '../../../../shared/components/placa-badge/placa-badge.component';

@Component({
  selector: 'app-veiculos-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
     
    FormsModule, 
    PlacaPipe, 
    MaskDirective, 
    PaginationComponent, 
    CustomSelectComponent,
    PageContainerComponent,
    PlacaBadgeComponent
  ],
  template: `
    <app-page-container>
      <div class="page-header">
        <div>
          <h1 class="page-title">
            <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="#ED145B" stroke-width="2.3"><path d="M19 17h2c.6 0 1-.4 1-1v-3c0-.9-.7-1.7-1.5-1.9C18.7 10.6 16 10 16 10s-1.3-1.4-2.2-2.3c-.5-.4-1.1-.7-1.8-.7H5c-.6 0-1.1.4-1.4.9l-1.5 3C2 11.3 2 11.7 2 12v4c0 .6.4 1 1 1h2"/><circle cx="7" cy="17" r="2"/><circle cx="17" cy="17" r="2"/></svg>
            Veículos
          </h1>
        </div>
        <button (click)="abrirModalNovo()" class="btn btn-primary">
          + Veículo
        </button>
      </div>

      <!-- Tabela de Veículos -->
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
              <th>Placa</th>
              <th>Modelo / Marca</th>
              <th>Fabricação/Modelo</th>
              <th>Proprietário</th>
              <th>Ações</th>
            </tr>
          </thead>
          <tbody>
            @for (v of veiculos; track v.id) {
              <tr>
                <td>
                  <app-placa-badge [placa]="v.placa | placa"></app-placa-badge>
                </td>
                <td style="font-weight: 600;">{{ v.marca }} {{ v.modelo }}</td>
                <td>{{ v.anoFabricacao }}/{{ v.anoModelo }}</td>
                <td>{{ getClienteNome(v.clienteId) }}</td>
                <td>
                  <div style="display: inline-flex; gap: 0.5rem;">
                    <button (click)="editar(v)" class="btn btn-secondary btn-sm" title="Editar Veículo">✏️ Editar</button>
                    <button (click)="excluir(v.id!)" class="btn btn-danger btn-sm" title="Excluir Veículo">🗑️ Excluir</button>
                  </div>
                </td>
              </tr>
            } @empty {
              <tr>
                <td colspan="6" style="text-align: center; padding: 2.5rem; color: #71717A;">
                  Nenhum veículo cadastrado.
                </td>
              </tr>
            }
          </tbody>
        </table>
      </div>

      <!-- Paginação -->
      <div style="margin-top: 1rem;">
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

      <!-- Modal de Cadastro / Edição -->
      @if (exibirModal) {
        <div class="modal-backdrop fade-in">
          <div class="modal-card">
            <div class="modal-header">
              <h3>{{ editandoId ? 'Editar Veículo' : 'Novo Veículo' }}</h3>
              <button (click)="exibirModal = false" class="btn-close">&times;</button>
            </div>

            <form (ngSubmit)="salvar()">
              <div class="form-group">
                <label class="form-label">Cliente Proprietário</label>
                <app-custom-select
                  [options]="clienteOptions"
                  [value]="formVeiculo.clienteId || ''"
                  placeholder="Selecione o proprietário"
                  [loading]="loadingClientes"
                  (valueChange)="formVeiculo.clienteId = $event"
                ></app-custom-select>
              </div>

              <div class="grid-2">
                <div class="form-group">
                  <label class="form-label">Marca</label>
                  <input type="text" [(ngModel)]="formVeiculo.marca" name="marca" required placeholder="Ex: Chevrolet" class="form-control" />
                </div>

                <div class="form-group">
                  <label class="form-label">Modelo</label>
                  <input type="text" [(ngModel)]="formVeiculo.modelo" name="modelo" required placeholder="Ex: Onix" class="form-control" />
                </div>
              </div>

              <div class="grid-2">
                <div class="form-group">
                  <label class="form-label">Placa</label>
                  <input type="text" [(ngModel)]="formVeiculo.placa" name="placa" appMask="placa" required placeholder="ABC-1234 / ABC1D23" class="form-control" />
                </div>

                <div class="form-group">
                  <label class="form-label">Ano Fabricação</label>
                  <input type="number" [(ngModel)]="formVeiculo.anoFabricacao" name="anoFabricacao" required placeholder="Ex: 2022" class="form-control" />
                </div>
              </div>

              <div class="grid-2">
                <div class="form-group">
                  <label class="form-label">Ano Modelo</label>
                  <input type="number" [(ngModel)]="formVeiculo.anoModelo" name="anoModelo" required placeholder="Ex: 2023" class="form-control" />
                </div>
              </div>

              <div style="display: flex; gap: 0.75rem; justify-content: flex-end; margin-top: 1.5rem;">
                <button type="button" (click)="exibirModal = false" class="btn btn-secondary">Cancelar</button>
                <button type="submit" class="btn btn-primary">Salvar Veículo</button>
              </div>
            </form>
          </div>
        </div>
      }
    </app-page-container>
  `,
  styles: [`
    .btn-sm { padding: 0.35rem 0.65rem; font-size: 0.8rem; }
    .grid-2 { display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; }
    .modal-backdrop { position: fixed; top: 0; left: 0; right: 0; bottom: 0; background: rgba(10, 10, 12, 0.8); backdrop-filter: blur(8px); display: flex; align-items: center; justify-content: center; z-index: 1000; padding: 1rem; }
    .modal-card { background: #18181C; border: 1px solid rgba(237, 20, 91, 0.3); border-radius: 12px; padding: 2rem; max-width: 580px; width: 100%; box-shadow: 0 20px 50px rgba(0, 0, 0, 0.9); }
    .modal-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem; }
    .modal-header h3 { font-family: 'Outfit', sans-serif; font-weight: 700; color: #fff; }
    .btn-close { background: none; border: none; color: #A1A1AA; font-size: 1.5rem; cursor: pointer; }
  `]
})
export class VeiculosPageComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  veiculos: Veiculo[] = [];
  clientes: Cliente[] = [];
  clienteOptions: SelectOption[] = [];
  loadingClientes = false;
  exibirModal = false;
  editandoId: string | null = null;

  pageNumber = 1;
  pageSize = 10;
  totalItems = 0;
  totalPages = 1;

  formVeiculo: Veiculo = {
    placa: '',
    marca: '',
    modelo: '',
    anoFabricacao: new Date().getFullYear(),
    anoModelo: new Date().getFullYear(),
    clienteId: ''
  };

  loading = false;
  private readonly veiculoService = inject(VeiculoService);
  private readonly clienteService = inject(ClienteService);
  private readonly notification = inject(NotificationService);
  private readonly cdr = inject(ChangeDetectorRef);

  ngOnInit() {
    this.carregar();
  }

  carregar() {
    this.loading = true;
    this.veiculoService.getAll(this.pageNumber, this.pageSize).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: res => {
        this.veiculos = res.items || [];
        this.totalItems = res.total;
        this.totalPages = res.totalPages;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
    this.loadingClientes = true;
    this.clienteService.getAll(1, 100).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: res => {
        this.clientes = res.items || [];
        this.clienteOptions = this.clientes.map(c => ({
          value: c.id!,
          label: `${c.nome} (${c.documento})`
        }));
        this.loadingClientes = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loadingClientes = false;
        this.cdr.detectChanges();
      }
    });
  }

  onPageChange(page: number) {
    this.pageNumber = page;
    this.carregar();
  }

  onPageSizeChange(size: number) {
    this.pageSize = size;
    this.pageNumber = 1;
    this.carregar();
  }

  getClienteNome(id: string): string {
    return this.clientes.find(c => c.id === id)?.nome || 'Desconhecido';
  }

  abrirModalNovo() {
    this.editandoId = null;
    this.formVeiculo = { placa: '', marca: '', modelo: '', anoFabricacao: new Date().getFullYear(), anoModelo: new Date().getFullYear(), clienteId: '' };
    this.exibirModal = true;
  }

  editar(v: Veiculo) {
    this.editandoId = v.id || null;
    this.formVeiculo = { ...v };
    this.exibirModal = true;
  }

  salvar() {
    if (this.editandoId) {
      this.veiculoService.atualizar(this.editandoId, this.formVeiculo).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
        next: () => {
          this.notification.success('Veículo Atualizado', 'Dados do veículo salvos.');
          this.exibirModal = false;
          this.carregar();
        }
      });
    } else {
      this.veiculoService.criar(this.formVeiculo).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
        next: () => {
          this.notification.success('Veículo Cadastrado', 'Novo veículo incluído na frota.');
          this.exibirModal = false;
          this.carregar();
        }
      });
    }
  }

  excluir(id: string) {
    if (confirm('Deseja remover este veículo?')) {
      this.veiculoService.excluir(id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
        next: () => {
          this.notification.info('Veículo Removido', 'Cadastro excluído.');
          this.carregar();
        }
      });
    }
  }
}
