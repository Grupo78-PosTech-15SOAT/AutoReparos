import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { VeiculoService } from '../../services/veiculo.service';
import { ClienteService } from '../../../clientes/services/cliente.service';
import { Veiculo } from '../../models/veiculo.model';
import { Cliente } from '../../../clientes/models/cliente.model';
import { PlacaPipe } from '../../../../shared/pipes/placa.pipe';
import { MaskDirective } from '../../../../shared/directives/mask.directive';
import { NotificationService } from '../../../../core/ui/notification.service';

@Component({
  selector: 'app-veiculos-page',
  standalone: true,
  imports: [CommonModule, FormsModule, PlacaPipe, MaskDirective],
  template: `
    <div class="container fade-in">
      <div class="page-header">
        <div>
          <h1 class="page-title">
            <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="#ED145B" stroke-width="2.3"><path d="M19 17h2c.6 0 1-.4 1-1v-3c0-.9-.7-1.7-1.5-1.9C18.7 10.6 16 10 16 10s-1.3-1.4-2.2-2.3c-.5-.4-1.1-.7-1.8-.7H5c-.6 0-1.1.4-1.4.9l-1.5 3C2 11.3 2 11.7 2 12v4c0 .6.4 1 1 1h2"/><circle cx="7" cy="17" r="2"/><circle cx="17" cy="17" r="2"/></svg>
            Gestão de Veículos
          </h1>
          <p class="page-subtitle">Frota de veículos cadastrados e vínculo com proprietários.</p>
        </div>
        <button (click)="abrirModalNovo()" class="btn btn-primary">
          + Cadastrar Veículo
        </button>
      </div>

      <!-- Tabela de Veículos -->
      <div class="data-table-container">
        <table class="data-table">
          <thead>
            <tr>
              <th>Placa</th>
              <th>Modelo & Marca</th>
              <th>Ano</th>
              <th>Cor</th>
              <th>Proprietário</th>
              <th style="text-align: right;">Ações</th>
            </tr>
          </thead>
          <tbody>
            @for (v of veiculos; track v.id) {
              <tr>
                <td>
                  <span class="mono-badge" style="color: #ED145B; font-weight: 700;">{{ v.placa | placa }}</span>
                </td>
                <td style="font-weight: 600;">{{ v.marca }} {{ v.modelo }}</td>
                <td>{{ v.ano }}</td>
                <td>{{ v.cor || '-' }}</td>
                <td>{{ v.clienteNome || 'Cliente não identificado' }}</td>
                <td style="text-align: right;">
                  <div style="display: inline-flex; gap: 0.5rem;">
                    <button (click)="editar(v)" class="btn btn-secondary btn-sm">Editar</button>
                    <button (click)="excluir(v.id!)" class="btn btn-danger btn-sm">Excluir</button>
                  </div>
                </td>
              </tr>
            } @empty {
              <tr>
                <td colspan="6" style="text-align: center; padding: 2.5rem; color: #71717A;">
                  Nenhum veículo cadastrado na frota.
                </td>
              </tr>
            }
          </tbody>
        </table>
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
                <label class="form-label">Proprietário (Cliente)</label>
                <select [(ngModel)]="formVeiculo.clienteId" name="clienteId" required class="form-control">
                  <option value="">-- Selecione o Cliente --</option>
                  @for (c of clientes; track c.id) {
                    <option [value]="c.id">{{ c.nome }} ({{ c.documento }})</option>
                  }
                </select>
              </div>

              <div class="grid-2">
                <div class="form-group">
                  <label class="form-label">Placa do Veículo</label>
                  <input type="text" [(ngModel)]="formVeiculo.placa" name="placa" appMask="placa" required placeholder="Ex: BRA2E19" class="form-control" />
                </div>

                <div class="form-group">
                  <label class="form-label">Marca / Fabricante</label>
                  <input type="text" [(ngModel)]="formVeiculo.marca" name="marca" required placeholder="Ex: Honda, Toyota, VW..." class="form-control" />
                </div>
              </div>

              <div class="grid-2">
                <div class="form-group">
                  <label class="form-label">Modelo</label>
                  <input type="text" [(ngModel)]="formVeiculo.modelo" name="modelo" required placeholder="Ex: Civic 2.0 EXL" class="form-control" />
                </div>

                <div class="form-group">
                  <label class="form-label">Ano Fabricação/Modelo</label>
                  <input type="number" [(ngModel)]="formVeiculo.ano" name="ano" required placeholder="2022" class="form-control" />
                </div>
              </div>

              <div class="form-group">
                <label class="form-label">Cor Predominante</label>
                <input type="text" [(ngModel)]="formVeiculo.cor" name="cor" placeholder="Ex: Previsão Preto Carbon / Prata" class="form-control" />
              </div>

              <div style="display: flex; gap: 0.75rem; justify-content: flex-end; margin-top: 1.5rem;">
                <button type="button" (click)="exibirModal = false" class="btn btn-secondary">Cancelar</button>
                <button type="submit" class="btn btn-primary">Salvar Veículo</button>
              </div>
            </form>
          </div>
        </div>
      }
    </div>
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
  veiculos: Veiculo[] = [];
  clientes: Cliente[] = [];
  exibirModal = false;
  editandoId: string | null = null;

  formVeiculo: Veiculo = {
    placa: '',
    marca: '',
    modelo: '',
    ano: new Date().getFullYear(),
    cor: '',
    clienteId: ''
  };

  private veiculoService = inject(VeiculoService);
  private clienteService = inject(ClienteService);
  private notification = inject(NotificationService);

  ngOnInit() {
    this.carregar();
  }

  carregar() {
    this.veiculoService.getAll().subscribe(data => this.veiculos = data);
    this.clienteService.getAll().subscribe(data => this.clientes = data);
  }

  abrirModalNovo() {
    this.editandoId = null;
    this.formVeiculo = { placa: '', marca: '', modelo: '', ano: new Date().getFullYear(), cor: '', clienteId: '' };
    this.exibirModal = true;
  }

  editar(v: Veiculo) {
    this.editandoId = v.id || null;
    this.formVeiculo = { ...v };
    this.exibirModal = true;
  }

  salvar() {
    if (this.editandoId) {
      this.veiculoService.atualizar(this.editandoId, this.formVeiculo).subscribe({
        next: () => {
          this.notification.success('Veículo Atualizado', 'Dados do veículo salvos.');
          this.exibirModal = false;
          this.carregar();
        }
      });
    } else {
      this.veiculoService.criar(this.formVeiculo).subscribe({
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
      this.veiculoService.excluir(id).subscribe({
        next: () => {
          this.notification.info('Veículo Removido', 'Cadastro excluído.');
          this.carregar();
        }
      });
    }
  }
}
