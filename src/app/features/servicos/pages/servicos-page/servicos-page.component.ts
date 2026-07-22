import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ServicoService } from '../../services/servico.service';
import { Servico } from '../../models/servico.model';
import { NotificationService } from '../../../../core/ui/notification.service';

@Component({
  selector: 'app-servicos-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="container fade-in">
      <div class="page-header">
        <div>
          <h1 class="page-title">
            <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="#ED145B" stroke-width="2.3"><path d="M12 20h9"/><path d="M16.5 3.5a2.121 2.121 0 0 1 3 3L7 19l-4 1 1-4L16.5 3.5z"/></svg>
            Catálogo de Serviços de Mão de Obra
          </h1>
          <p class="page-subtitle">Padrões de serviço, preços base e tempos estimados de execução.</p>
        </div>
        <button (click)="abrirModalNovo()" class="btn btn-primary">
          + Cadastrar Serviço
        </button>
      </div>

      <!-- Tabela de Serviços -->
      <div class="data-table-container">
        <table class="data-table">
          <thead>
            <tr>
              <th>Serviço / Procedimento</th>
              <th>Preço Base Mão de Obra</th>
              <th>Tempo Estimado</th>
              <th style="text-align: right;">Ações</th>
            </tr>
          </thead>
          <tbody>
            @for (s of servicos; track s.id) {
              <tr>
                <td>
                  <div style="font-weight: 600;">{{ s.nome }}</div>
                  @if (s.descricao) {
                    <div style="font-size: 0.75rem; color: #A1A1AA;">{{ s.descricao }}</div>
                  }
                </td>
                <td>
                  <span style="font-family: 'JetBrains Mono', monospace; font-weight: 700; color: #10B981;">
                    R$ {{ s.precoBase | number:'1.2-2' }}
                  </span>
                </td>
                <td>⏱️ {{ s.tempoEstimadoMinutos }} min</td>
                <td style="text-align: right;">
                  <div style="display: inline-flex; gap: 0.5rem;">
                    <button (click)="editar(s)" class="btn btn-secondary btn-sm">Editar</button>
                    <button (click)="excluir(s.id!)" class="btn btn-danger btn-sm">Excluir</button>
                  </div>
                </td>
              </tr>
            } @empty {
              <tr>
                <td colspan="4" style="text-align: center; padding: 2.5rem; color: #71717A;">
                  Nenhum serviço disponível no catálogo.
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
              <h3>{{ editandoId ? 'Editar Serviço' : 'Novo Serviço' }}</h3>
              <button (click)="exibirModal = false" class="btn-close">&times;</button>
            </div>

            <form (ngSubmit)="salvar()">
              <div class="form-group">
                <label class="form-label">Nome do Serviço</label>
                <input type="text" [(ngModel)]="formServico.nome" name="nome" required placeholder="Ex: Alinhamento 3D e Balanceamento" class="form-control" />
              </div>

              <div class="form-group">
                <label class="form-label">Descrição do Procedimento</label>
                <input type="text" [(ngModel)]="formServico.descricao" name="descricao" placeholder="Ex: Regulagem de geometria da suspensão dianteira e traseira" class="form-control" />
              </div>

              <div class="grid-2">
                <div class="form-group">
                  <label class="form-label">Preço Base (R$)</label>
                  <input type="number" step="0.01" [(ngModel)]="formServico.precoBase" name="precoBase" required class="form-control" />
                </div>

                <div class="form-group">
                  <label class="form-label">Tempo Estimado (Minutos)</label>
                  <input type="number" [(ngModel)]="formServico.tempoEstimadoMinutos" name="tempoEstimadoMinutos" required placeholder="60" class="form-control" />
                </div>
              </div>

              <div style="display: flex; gap: 0.75rem; justify-content: flex-end; margin-top: 1.5rem;">
                <button type="button" (click)="exibirModal = false" class="btn btn-secondary">Cancelar</button>
                <button type="submit" class="btn btn-primary">Salvar Serviço</button>
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
export class ServicosPageComponent implements OnInit {
  servicos: Servico[] = [];
  exibirModal = false;
  editandoId: string | null = null;

  formServico: Servico = {
    nome: '',
    descricao: '',
    precoBase: 0,
    tempoEstimadoMinutos: 60
  };

  private servicoService = inject(ServicoService);
  private notification = inject(NotificationService);

  ngOnInit() {
    this.carregar();
  }

  carregar() {
    this.servicoService.getAll().subscribe(data => this.servicos = data);
  }

  abrirModalNovo() {
    this.editandoId = null;
    this.formServico = { nome: '', descricao: '', precoBase: 0, tempoEstimadoMinutos: 60 };
    this.exibirModal = true;
  }

  editar(s: Servico) {
    this.editandoId = s.id || null;
    this.formServico = { ...s };
    this.exibirModal = true;
  }

  salvar() {
    if (this.editandoId) {
      this.servicoService.atualizar(this.editandoId, this.formServico).subscribe({
        next: () => {
          this.notification.success('Serviço Atualizado', 'Item alterado no catálogo.');
          this.exibirModal = false;
          this.carregar();
        }
      });
    } else {
      this.servicoService.criar(this.formServico).subscribe({
        next: () => {
          this.notification.success('Serviço Cadastrado', 'Novo serviço incluído.');
          this.exibirModal = false;
          this.carregar();
        }
      });
    }
  }

  excluir(id: string) {
    if (confirm('Deseja excluir este serviço do catálogo?')) {
      this.servicoService.excluir(id).subscribe({
        next: () => {
          this.notification.info('Serviço Removido', 'Item excluído.');
          this.carregar();
        }
      });
    }
  }
}
