import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UsuarioService } from '../../services/usuario.service';
import { Usuario } from '../../models/usuario.model';
import { NotificationService } from '../../../../core/ui/notification.service';
import { PaginationComponent } from '../../../../shared/components/pagination/pagination.component';
import { CustomSelectComponent, SelectOption } from '../../../../shared/components/custom-select/custom-select.component';

@Component({
  selector: 'app-usuarios-page',
  standalone: true,
  imports: [CommonModule, FormsModule, PaginationComponent, CustomSelectComponent],
  template: `
    <div class="container fade-in">
      <div class="page-header">
        <div>
          <h1 class="page-title">
            <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="#ED145B" stroke-width="2.3"><path d="M16 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="8.5" cy="7" r="4"/><line x1="20" y1="8" x2="20" y2="14"/><line x1="23" y1="11" x2="17" y2="11"/></svg>
            Usuários
          </h1>
        </div>
        <button (click)="abrirModalNovo()" class="btn btn-primary">
          + Usuário
        </button>
      </div>

      <!-- Tabela de Usuários -->
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
              <th>Nome</th>
              <th>E-mail</th>
              <th>Perfil</th>
              <th>Ações</th>
            </tr>
          </thead>
          <tbody>
            @for (u of usuarios; track u.id) {
              <tr>
                <td style="font-weight: 600;">{{ u.nome }}</td>
                <td>{{ u.email }}</td>
                <td>
                  <span class="role-badge" [ngClass]="getRoleClass(u.role)">
                    {{ u.role || 'Sem Cargo' }}
                  </span>
                </td>
                <td>
                  <button (click)="excluir(u.id!)" class="btn btn-danger btn-sm" title="Excluir Usuário">🗑️ Excluir</button>
                </td>
              </tr>
            } @empty {
              <tr>
                <td colspan="4" style="text-align: center; padding: 2.5rem; color: #71717A;">
                  Nenhum usuário cadastrado.
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

      <!-- Modal de Criar Usuário -->
      @if (exibirModal) {
        <div class="modal-backdrop fade-in">
          <div class="modal-card">
            <div class="modal-header">
              <h3>Novo Usuário de Sistema</h3>
              <button (click)="exibirModal = false" class="btn-close">&times;</button>
            </div>

            <form (ngSubmit)="salvar()">
              <div class="form-group">
                <label class="form-label">Nome Completo</label>
                <input type="text" [(ngModel)]="formUsuario.nome" name="nome" required placeholder="Ex: João da Silva" class="form-control" />
              </div>

              <div class="form-group">
                <label class="form-label">E-mail Corporativo</label>
                <input type="email" [(ngModel)]="formUsuario.email" name="email" required placeholder="mecanico@autoreparos.com" class="form-control" />
              </div>

              <div class="grid-2">
                <div class="form-group">
                  <label class="form-label">Perfil de Acesso (Role)</label>
                  <app-custom-select
                    [options]="roleOptions"
                    [value]="formUsuario.role || 'Mecanico'"
                    placeholder="Selecione o Perfil"
                    (valueChange)="formUsuario.role = $any($event)"
                  ></app-custom-select>
                </div>

                <div class="form-group">
                  <label class="form-label">Senha Inicial</label>
                  <input type="password" [(ngModel)]="formUsuario.senha" name="senha" required placeholder="••••••••" class="form-control" />
                </div>
              </div>

              <div style="display: flex; gap: 0.75rem; justify-content: flex-end; margin-top: 1.5rem;">
                <button type="button" (click)="exibirModal = false" class="btn btn-secondary">Cancelar</button>
                <button type="submit" class="btn btn-primary">Criar Usuário</button>
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
    .role-badge { padding: 0.25rem 0.65rem; border-radius: 999px; font-size: 0.75rem; font-weight: 700; display: inline-block; text-transform: uppercase; }
    .role-badge.administrador { background: rgba(237, 20, 91, 0.15); color: #ED145B; border: 1px solid rgba(237, 20, 91, 0.3); }
    .role-badge.atendente { background: rgba(59, 130, 246, 0.15); color: #3B82F6; border: 1px solid rgba(59, 130, 246, 0.3); }
    .role-badge.mecanico { background: rgba(245, 158, 11, 0.15); color: #F59E0B; border: 1px solid rgba(245, 158, 11, 0.3); }
    .role-badge.sem-cargo { background: rgba(113, 113, 122, 0.15); color: #A1A1AA; border: 1px solid rgba(113, 113, 122, 0.3); }

    .modal-backdrop { position: fixed; top: 0; left: 0; right: 0; bottom: 0; background: rgba(10, 10, 12, 0.8); backdrop-filter: blur(8px); display: flex; align-items: center; justify-content: center; z-index: 1000; padding: 1rem; }
    .modal-card { background: #18181C; border: 1px solid rgba(237, 20, 91, 0.3); border-radius: 12px; padding: 2rem; max-width: 580px; width: 100%; box-shadow: 0 20px 50px rgba(0, 0, 0, 0.9); }
    .modal-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem; }
    .modal-header h3 { font-family: 'Outfit', sans-serif; font-weight: 700; color: #fff; }
    .btn-close { background: none; border: none; color: #A1A1AA; font-size: 1.5rem; cursor: pointer; }
  `]
})
export class UsuariosPageComponent implements OnInit {
  usuarios: Usuario[] = [];
  exibirModal = false;

  roleOptions: SelectOption[] = [
    { value: 'Administrador', label: 'Administrador' },
    { value: 'Atendente', label: 'Atendente' },
    { value: 'Mecanico', label: 'Mecânico' }
  ];

  pageNumber = 1;
  pageSize = 10;
  totalItems = 0;
  totalPages = 1;

  formUsuario: Usuario = {
    nome: '',
    email: '',
    role: 'Mecanico',
    senha: ''
  };

  loading = false;
  private usuarioService = inject(UsuarioService);
  private notification = inject(NotificationService);
  private cdr = inject(ChangeDetectorRef);

  getRoleClass(role?: string): string {
    if (!role) return 'sem-cargo';
    return role.toLowerCase().replace(/\s+/g, '-');
  }

  ngOnInit() {
    this.carregar();
  }

  carregar() {
    this.loading = true;
    this.usuarioService.getAll(this.pageNumber, this.pageSize).subscribe({
      next: res => {
        this.usuarios = res.items || [];
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
    this.formUsuario = { nome: '', email: '', role: 'Mecanico', senha: '' };
    this.exibirModal = true;
  }

  salvar() {
    this.usuarioService.criar(this.formUsuario).subscribe({
      next: () => {
        this.notification.success('Usuário Criado', 'Conta cadastrada com sucesso.');
        this.exibirModal = false;
        this.carregar();
      }
    });
  }

  excluir(id: string) {
    if (confirm('Deseja excluir esta conta de usuário?')) {
      this.usuarioService.excluir(id).subscribe({
        next: () => {
          this.notification.info('Usuário Removido', 'Conta excluída.');
          this.carregar();
        }
      });
    }
  }
}
