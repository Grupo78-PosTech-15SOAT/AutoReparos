import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ClienteService } from '../../services/cliente.service';
import { Cliente } from '../../models/cliente.model';
import { CpfCnpjPipe } from '../../../../shared/pipes/cpf-cnpj.pipe';
import { MaskDirective } from '../../../../shared/directives/mask.directive';
import { NotificationService } from '../../../../core/ui/notification.service';
import { PaginationComponent } from '../../../../shared/components/pagination/pagination.component';
import { CustomSelectComponent, SelectOption } from '../../../../shared/components/custom-select/custom-select.component';

@Component({
  selector: 'app-clientes-page',
  standalone: true,
  imports: [CommonModule, FormsModule, CpfCnpjPipe, MaskDirective, PaginationComponent, CustomSelectComponent],
  template: `
    <div class="container fade-in">
      <div class="page-header">
        <div>
          <h1 class="page-title">
            <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="#ED145B" stroke-width="2.3"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/></svg>
            Clientes
          </h1>
        </div>
        <button (click)="abrirModalNovo()" class="btn btn-primary">
          + Cliente
        </button>
      </div>

      <!-- Tabela de Clientes -->
      <div class="data-table-container">
        <table class="data-table">
          <thead>
            <tr>
              <th>Cliente</th>
              <th>CPF / CNPJ</th>
              <th>E-mail</th>
              <th>Telefone</th>
              <th style="text-align: right;">Ações</th>
            </tr>
          </thead>
          <tbody>
            @for (c of clientes; track c.id) {
              <tr>
                <td style="font-weight: 600;">{{ c.nome }}</td>
                <td>
                  <span class="mono-badge">{{ c.documento | cpfCnpj }}</span>
                </td>
                <td>{{ c.email }}</td>
                <td>{{ c.telefone }}</td>
                <td style="text-align: right;">
                  <div style="display: inline-flex; gap: 0.5rem;">
                    <button (click)="editar(c)" class="btn btn-secondary btn-sm" title="Editar Cliente">✏️ Editar</button>
                    <button (click)="excluir(c.id!)" class="btn btn-danger btn-sm" title="Excluir Cliente">🗑️ Excluir</button>
                  </div>
                </td>
              </tr>
            } @empty {
              <tr>
                <td colspan="5" style="text-align: center; padding: 2.5rem; color: #71717A;">
                  Nenhum cliente cadastrado.
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

      <!-- Modal de Cadastro / Edição -->
      @if (exibirModal) {
        <div class="modal-backdrop fade-in">
          <div class="modal-card">
            <div class="modal-header">
              <h3>{{ editandoId ? 'Editar Cliente' : 'Novo Cliente' }}</h3>
              <button (click)="exibirModal = false" class="btn-close">&times;</button>
            </div>

            <form (ngSubmit)="salvar()">
              <div class="form-group">
                <label class="form-label">Nome</label>
                <input type="text" [(ngModel)]="formCliente.nome" name="nome" required placeholder="Nome completo ou razão social" class="form-control" />
              </div>

              <div class="grid-2">
                <div class="form-group">
                  <label class="form-label">Tipo de Pessoa</label>
                  <app-custom-select
                    [options]="tipoDocumentoOptions"
                    [value]="formCliente.tipoDocumento || 'CPF'"
                    placeholder="Selecione o Tipo"
                    (valueChange)="formCliente.tipoDocumento = $any($event)"
                  ></app-custom-select>
                </div>

                <div class="form-group">
                  <label class="form-label">CPF / CNPJ</label>
                  <input type="text" [(ngModel)]="formCliente.documento" name="documento" appMask="cpfCnpj" required placeholder="000.000.000-00" class="form-control" />
                </div>
              </div>

              <div class="grid-2">
                <div class="form-group">
                  <label class="form-label">E-mail</label>
                  <input type="email" [(ngModel)]="formCliente.email" name="email" required placeholder="cliente@email.com" class="form-control" />
                </div>

                <div class="form-group">
                  <label class="form-label">Telefone</label>
                  <input type="text" [(ngModel)]="formCliente.telefone" name="telefone" appMask="telefone" required placeholder="(11) 99999-9999" class="form-control" />
                </div>
              </div>

              <div class="form-group">
                <label class="form-label">Endereço Completo</label>
                <input type="text" [(ngModel)]="formCliente.endereco" name="endereco" placeholder="Rua, Número, Bairro, Cidade - UF" class="form-control" />
              </div>

              <div style="display: flex; gap: 0.75rem; justify-content: flex-end; margin-top: 1.5rem;">
                <button type="button" (click)="exibirModal = false" class="btn btn-secondary">Cancelar</button>
                <button type="submit" class="btn btn-primary">Salvar Cliente</button>
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
export class ClientesPageComponent implements OnInit {
  clientes: Cliente[] = [];
  exibirModal = false;
  editandoId: string | null = null;

  tipoDocumentoOptions: SelectOption[] = [
    { value: 'CPF', label: 'Pessoa Física (CPF)' },
    { value: 'CNPJ', label: 'Pessoa Jurídica (CNPJ)' }
  ];

  pageNumber = 1;
  pageSize = 10;
  totalItems = 0;
  totalPages = 1;

  formCliente: Cliente = {
    nome: '',
    email: '',
    telefone: '',
    documento: '',
    tipoDocumento: 'CPF',
    endereco: ''
  };

  private clienteService = inject(ClienteService);
  private notification = inject(NotificationService);

  ngOnInit() {
    this.carregar();
  }

  carregar() {
    this.clienteService.getAll(this.pageNumber, this.pageSize).subscribe(res => {
      this.clientes = res.items || [];
      this.totalItems = res.total;
      this.totalPages = res.totalPages;
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

  abrirModalNovo() {
    this.editandoId = null;
    this.formCliente = { nome: '', email: '', telefone: '', documento: '', tipoDocumento: 'CPF', endereco: '' };
    this.exibirModal = true;
  }

  editar(c: Cliente) {
    this.editandoId = c.id || null;
    this.formCliente = { ...c };
    this.exibirModal = true;
  }

  salvar() {
    if (this.editandoId) {
      this.clienteService.atualizar(this.editandoId, this.formCliente).subscribe({
        next: () => {
          this.notification.success('Cliente Atualizado', 'Dados do cliente salvos com sucesso.');
          this.exibirModal = false;
          this.carregar();
        }
      });
    } else {
      this.clienteService.criar(this.formCliente).subscribe({
        next: () => {
          this.notification.success('Cliente Cadastrado', 'Novo cliente incluído na base.');
          this.exibirModal = false;
          this.carregar();
        }
      });
    }
  }

  excluir(id: string) {
    if (confirm('Deseja realmente remover este cliente?')) {
      this.clienteService.excluir(id).subscribe({
        next: () => {
          this.notification.info('Cliente Removido', 'Cadastro removido.');
          this.carregar();
        }
      });
    }
  }
}
