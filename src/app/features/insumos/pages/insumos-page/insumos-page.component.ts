import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { InsumoService } from '../../services/insumo.service';
import { Insumo } from '../../models/insumo.model';
import { NotificationService } from '../../../../core/ui/notification.service';

@Component({
  selector: 'app-insumos-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="container fade-in">
      <div class="page-header">
        <div>
          <h1 class="page-title">
            <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="#ED145B" stroke-width="2.3"><path d="M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z"/></svg>
            Gestão de Insumos e Estoque Mínimo
          </h1>
          <p class="page-subtitle">Controle de peças de reposição e alertas de nível crítico de estoque.</p>
        </div>
        <button (click)="abrirModalNovo()" class="btn btn-primary">
          + Cadastrar Insumo / Peça
        </button>
      </div>

      <!-- Tabela de Insumos -->
      <div class="data-table-container">
        <table class="data-table">
          <thead>
            <tr>
              <th>Nome da Peça / Insumo</th>
              <th>Preço Unitário</th>
              <th>Estoque Atual</th>
              <th>Estoque Mínimo</th>
              <th>Status do Estoque</th>
              <th style="text-align: right;">Ações</th>
            </tr>
          </thead>
          <tbody>
            @for (item of insumos; track item.id) {
              <tr>
                <td>
                  <div style="font-weight: 600;">{{ item.nome }}</div>
                  @if (item.descricao) {
                    <div style="font-size: 0.75rem; color: #A1A1AA;">{{ item.descricao }}</div>
                  }
                </td>
                <td>
                  <span style="font-family: 'JetBrains Mono', monospace; font-weight: 600;">R$ {{ item.precoUnitario | number:'1.2-2' }}</span>
                </td>
                <td>
                  <span style="font-family: 'JetBrains Mono', monospace; font-weight: 700; font-size: 1rem;" [style.color]="item.quantidadeEstoque <= item.quantidadeMinima ? '#EF4444' : '#F8FAFC'">
                    {{ item.quantidadeEstoque }} un.
                  </span>
                </td>
                <td>{{ item.quantidadeMinima }} un.</td>
                <td>
                  @if (item.quantidadeEstoque <= item.quantidadeMinima) {
                    <span class="stock-badge danger">
                      ⚠️ Abaixo do Mínimo!
                    </span>
                  } @else {
                    <span class="stock-badge success">
                      ✓ Normal
                    </span>
                  }
                </td>
                <td style="text-align: right;">
                  <div style="display: inline-flex; gap: 0.5rem;">
                    <button (click)="editar(item)" class="btn btn-secondary btn-sm">Editar</button>
                    <button (click)="excluir(item.id!)" class="btn btn-danger btn-sm">Excluir</button>
                  </div>
                </td>
              </tr>
            } @empty {
              <tr>
                <td colspan="6" style="text-align: center; padding: 2.5rem; color: #71717A;">
                  Nenhum insumo cadastrado no estoque.
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
              <h3>{{ editandoId ? 'Editar Insumo' : 'Novo Insumo / Peça' }}</h3>
              <button (click)="exibirModal = false" class="btn-close">&times;</button>
            </div>

            <form (ngSubmit)="salvar()">
              <div class="form-group">
                <label class="form-label">Nome do Insumo / Peça</label>
                <input type="text" [(ngModel)]="formInsumo.nome" name="nome" required placeholder="Ex: Amortecedor Dianteiro Honda Civic" class="form-control" />
              </div>

              <div class="form-group">
                <label class="form-label">Descrição Técnica / Especificações</label>
                <input type="text" [(ngModel)]="formInsumo.descricao" name="descricao" placeholder="Ex: Par de amortecedores de liga reforçada" class="form-control" />
              </div>

              <div class="grid-3">
                <div class="form-group">
                  <label class="form-label">Preço Unitário (R$)</label>
                  <input type="number" step="0.01" [(ngModel)]="formInsumo.precoUnitario" name="precoUnitario" required class="form-control" />
                </div>

                <div class="form-group">
                  <label class="form-label">Qtd em Estoque</label>
                  <input type="number" [(ngModel)]="formInsumo.quantidadeEstoque" name="quantidadeEstoque" required class="form-control" />
                </div>

                <div class="form-group">
                  <label class="form-label">Estoque Mínimo</label>
                  <input type="number" [(ngModel)]="formInsumo.quantidadeMinima" name="quantidadeMinima" required class="form-control" />
                </div>
              </div>

              <div style="display: flex; gap: 0.75rem; justify-content: flex-end; margin-top: 1.5rem;">
                <button type="button" (click)="exibirModal = false" class="btn btn-secondary">Cancelar</button>
                <button type="submit" class="btn btn-primary">Salvar Insumo</button>
              </div>
            </form>
          </div>
        </div>
      }
    </div>
  `,
  styles: [`
    .btn-sm { padding: 0.35rem 0.65rem; font-size: 0.8rem; }
    .grid-3 { display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 1rem; }
    .stock-badge { padding: 0.25rem 0.65rem; border-radius: 999px; font-size: 0.75rem; font-weight: 700; display: inline-block; }
    .stock-badge.danger { background: rgba(239, 68, 68, 0.15); color: #EF4444; border: 1px solid rgba(239, 68, 68, 0.3); }
    .stock-badge.success { background: rgba(16, 185, 129, 0.15); color: #10B981; border: 1px solid rgba(16, 185, 129, 0.3); }

    .modal-backdrop { position: fixed; top: 0; left: 0; right: 0; bottom: 0; background: rgba(10, 10, 12, 0.8); backdrop-filter: blur(8px); display: flex; align-items: center; justify-content: center; z-index: 1000; padding: 1rem; }
    .modal-card { background: #18181C; border: 1px solid rgba(237, 20, 91, 0.3); border-radius: 12px; padding: 2rem; max-width: 580px; width: 100%; box-shadow: 0 20px 50px rgba(0, 0, 0, 0.9); }
    .modal-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem; }
    .modal-header h3 { font-family: 'Outfit', sans-serif; font-weight: 700; color: #fff; }
    .btn-close { background: none; border: none; color: #A1A1AA; font-size: 1.5rem; cursor: pointer; }
  `]
})
export class InsumosPageComponent implements OnInit {
  insumos: Insumo[] = [];
  exibirModal = false;
  editandoId: string | null = null;

  formInsumo: Insumo = {
    nome: '',
    descricao: '',
    precoUnitario: 0,
    quantidadeEstoque: 10,
    quantidadeMinima: 3
  };

  private insumoService = inject(InsumoService);
  private notification = inject(NotificationService);

  ngOnInit() {
    this.carregar();
  }

  carregar() {
    this.insumoService.getAll().subscribe(data => this.insumos = data);
  }

  abrirModalNovo() {
    this.editandoId = null;
    this.formInsumo = { nome: '', descricao: '', precoUnitario: 0, quantidadeEstoque: 10, quantidadeMinima: 3 };
    this.exibirModal = true;
  }

  editar(item: Insumo) {
    this.editandoId = item.id || null;
    this.formInsumo = { ...item };
    this.exibirModal = true;
  }

  salvar() {
    if (this.editandoId) {
      this.insumoService.atualizar(this.editandoId, this.formInsumo).subscribe({
        next: () => {
          this.notification.success('Insumo Atualizado', 'Item salvo no estoque.');
          this.exibirModal = false;
          this.carregar();
        }
      });
    } else {
      this.insumoService.criar(this.formInsumo).subscribe({
        next: () => {
          this.notification.success('Insumo Cadastrado', 'Novo insumo incluído.');
          this.exibirModal = false;
          this.carregar();
        }
      });
    }
  }

  excluir(id: string) {
    if (confirm('Deseja remover este insumo do catálogo?')) {
      this.insumoService.excluir(id).subscribe({
        next: () => {
          this.notification.info('Insumo Removido', 'Item excluído.');
          this.carregar();
        }
      });
    }
  }
}
