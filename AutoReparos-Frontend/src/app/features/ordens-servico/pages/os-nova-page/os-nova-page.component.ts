import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {  Component, OnInit, inject, ChangeDetectionStrategy, ChangeDetectorRef , DestroyRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { OrdemServicoService } from '../../services/ordem-servico.service';
import { ClienteService } from '../../../clientes/services/cliente.service';
import { VeiculoService } from '../../../veiculos/services/veiculo.service';
import { Cliente } from '../../../clientes/models/cliente.model';
import { Veiculo } from '../../../veiculos/models/veiculo.model';
import { NotificationService } from '../../../../core/ui/notification.service';
import { MaskDirective } from '../../../../shared/directives/mask.directive';
import {
  CustomSelectComponent,
  SelectOption,
} from '../../../../shared/components/custom-select/custom-select.component';
import { PageContainerComponent } from '../../../../shared/components/page-container/page-container.component';
import { STORAGE_TOKEN } from '../../../../core/tokens/storage.token';

const DRAFT_STORAGE_KEY = 'autoreparos_draft_nova_os';

@Component({
  selector: 'app-os-nova-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormsModule, MaskDirective, CustomSelectComponent, PageContainerComponent],
  template: `
    <app-page-container maxWidth="800px">
      <div class="page-header" style="display: flex; justify-content: space-between; align-items: flex-start; flex-wrap: wrap;">
        <div>
          <h1 class="page-title">
            <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="#ED145B" stroke-width="2.3"><path d="M12 5v14M5 12h14"/></svg>
            Nova Ordem de Serviço
          </h1>
        </div>
        @if (rascunhoSalvo) {
          <div class="draft-badge">
            💾 Rascunho salvo
          </div>
        }
      </div>

      <!-- Loading Skeleton -->
      @if (loadingDados) {
        <div class="card-panel">
          <div class="skeleton-section">
            <div class="skeleton-label"></div>
            <div class="skeleton-row">
              <div class="skeleton-input"></div>
              <div class="skeleton-input"></div>
            </div>
          </div>
          <div class="skeleton-section" style="margin-top: 1.5rem;">
            <div class="skeleton-label"></div>
            <div class="skeleton-row">
              <div class="skeleton-input"></div>
              <div class="skeleton-input"></div>
            </div>
          </div>
          <div class="skeleton-section" style="margin-top: 1.5rem;">
            <div class="skeleton-label"></div>
            <div class="skeleton-textarea"></div>
          </div>
          <div style="display: flex; gap: 1rem; justify-content: flex-end; margin-top: 2rem;">
            <div class="skeleton-btn"></div>
            <div class="skeleton-btn"></div>
          </div>
        </div>
      } @else {
        <div class="card-panel">
          <form (ngSubmit)="salvarOS()">
            <!-- 1. Busca / Seleção do Cliente -->
            <div class="form-section">
              <h3 class="section-heading">1. Cliente</h3>
              <div class="grid-2">
                <div class="form-group">
                  <label class="form-label">Cliente Cadastrado</label>
                  <app-custom-select
                    [options]="clienteOptions"
                    [value]="clienteId"
                    placeholder="-- Selecione o Cliente --"
                    (valueChange)="onClienteSelect($event)"
                  ></app-custom-select>
                </div>

                <div class="form-group">
                  <label class="form-label">Filtrar Cliente por CPF/CNPJ</label>
                  <input type="text" [(ngModel)]="buscaDocumento" name="buscaDocumento" appMask="cpfCnpj" (input)="buscarCliente()" placeholder="Digite CPF ou CNPJ..." class="form-control" />
                </div>
              </div>
            </div>

            <!-- 2. Busca / Seleção do Veículo -->
            <div class="form-section" style="margin-top: 1.5rem;">
              <h3 class="section-heading">2. Identificação do Veículo</h3>
              <div class="grid-2">
                <div class="form-group">
                  <label class="form-label">Veículo do Cliente</label>
                  <app-custom-select
                    [options]="veiculoOptions"
                    [value]="veiculoId"
                    placeholder="-- Selecione o Veículo --"
                    (valueChange)="onVeiculoSelect($event)"
                  ></app-custom-select>
                </div>

                <div class="form-group">
                  <label class="form-label">Ou Buscar por Placa</label>
                  <input type="text" [(ngModel)]="buscaPlaca" name="buscaPlaca" appMask="placa" (input)="buscarVeiculoPorPlaca()" placeholder="Ex: BRA2E19" class="form-control" />
                </div>
              </div>
            </div>

            <!-- 3. Observações de Entrada -->
            <div class="form-section" style="margin-top: 1.5rem;">
              <h3 class="section-heading">3. Observações Iniciais</h3>
              <div class="form-group">
                <textarea [(ngModel)]="observacoesIniciais" name="observacoesIniciais" (input)="salvarRascunho()" rows="4" placeholder="Sintomas relatados pelo cliente..." class="form-control"></textarea>
              </div>
            </div>

            <!-- Ações -->
            <div style="display: flex; gap: 1rem; justify-content: flex-end; margin-top: 2rem;">
              <button type="button" (click)="cancelar()" class="btn btn-secondary">Cancelar</button>
              <button type="submit" [disabled]="loading || !clienteId || !veiculoId" class="btn btn-primary">
                @if (loading) {
                  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" style="animation: spin 0.8s linear infinite;">
                    <path d="M21 12a9 9 0 1 1-18 0 9 9 0 0 1 18 0"/>
                  </svg>
                  Abrindo...
                } @else {
                  Abrir OS &rarr;
                }
              </button>
            </div>
          </form>
        </div>
      }
    </app-page-container>
  `,
  styles: [`
    .form-section {
      border-bottom: 1px solid rgba(255, 255, 255, 0.08);
      padding-bottom: 1.5rem;
    }
    .section-heading {
      font-family: 'Outfit', sans-serif;
      font-size: 1.1rem;
      font-weight: 700;
      color: #ED145B;
      margin-bottom: 1rem;
    }
    .grid-2 {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(260px, 1fr));
      gap: 1.25rem;
    }
    .draft-badge {
      font-size: 0.75rem;
      color: #3B82F6;
      background: rgba(59, 130, 246, 0.1);
      border: 1px solid rgba(59, 130, 246, 0.3);
      padding: 0.3rem 0.75rem;
      border-radius: 999px;
      font-weight: 600;
      margin-top: 0.25rem;
    }

    /* Skeleton Loading */
    @keyframes shimmer {
      0% { background-position: -400px 0; }
      100% { background-position: 400px 0; }
    }
    .skeleton-section { padding-bottom: 1.5rem; border-bottom: 1px solid rgba(255,255,255,0.06); }
    .skeleton-label {
      width: 120px; height: 12px; border-radius: 4px;
      background: linear-gradient(90deg, #27272A 25%, #3F3F46 50%, #27272A 75%);
      background-size: 400px 100%;
      animation: shimmer 1.4s infinite linear;
      margin-bottom: 0.75rem;
    }
    .skeleton-row { display: grid; grid-template-columns: 1fr 1fr; gap: 1.25rem; }
    .skeleton-input {
      height: 46px; border-radius: 8px;
      background: linear-gradient(90deg, #1C1C20 25%, #27272A 50%, #1C1C20 75%);
      background-size: 400px 100%;
      animation: shimmer 1.4s infinite linear;
    }
    .skeleton-textarea {
      height: 104px; border-radius: 8px;
      background: linear-gradient(90deg, #1C1C20 25%, #27272A 50%, #1C1C20 75%);
      background-size: 400px 100%;
      animation: shimmer 1.4s infinite linear;
    }
    .skeleton-btn {
      width: 100px; height: 38px; border-radius: 8px;
      background: linear-gradient(90deg, #1C1C20 25%, #27272A 50%, #1C1C20 75%);
      background-size: 400px 100%;
      animation: shimmer 1.4s infinite linear;
    }

    @keyframes spin {
      from { transform: rotate(0deg); }
      to { transform: rotate(360deg); }
    }
  `]
})
export class OsNovaPageComponent implements OnInit {
  private destroyRef = inject(DestroyRef);
  clienteId = '';
  veiculoId = '';
  observacoesIniciais = '';

  buscaDocumento = '';
  buscaPlaca = '';

  clientes: Cliente[] = [];
  todosVeiculos: Veiculo[] = [];
  veiculosFiltrados: Veiculo[] = [];

  clienteOptions: SelectOption[] = [];
  veiculoOptions: SelectOption[] = [];

  loading = false;
  loadingDados = true;
  rascunhoSalvo = false;

  private osService = inject(OrdemServicoService);
  private clienteService = inject(ClienteService);
  private veiculoService = inject(VeiculoService);
  private notification = inject(NotificationService);
  private router = inject(Router);
  private storage = inject(STORAGE_TOKEN);
  private cdr = inject(ChangeDetectorRef);

  ngOnInit() {
    this.carregarDados();
    this.carregarRascunho();
  }

  carregarDados() {
    this.loadingDados = true;
    let clientesCarregados = false;
    let veiculosCarregados = false;

    const checarConcluido = () => {
      if (clientesCarregados && veiculosCarregados) {
        this.loadingDados = false;
      }
    };

    this.clienteService.getAll(1, 100).pipe(takeUntilDestroyed(this.destroyRef)).subscribe(res => {
      this.clientes = res.items || [];
      this.clienteOptions = this.clientes.map(c => ({
        value: c.id!,
        label: `${c.nome} (${c.documento})`
      }));
      clientesCarregados = true;
      checarConcluido();
    });

    this.veiculoService.getAll(1, 100).pipe(takeUntilDestroyed(this.destroyRef)).subscribe(res => {
      const data = res.items || [];
      this.todosVeiculos = data;
      this.veiculosFiltrados = data;
      this.atualizarVeiculoOptions();
      veiculosCarregados = true;
      checarConcluido();
      this.cdr.markForCheck();
    });
  }

  atualizarVeiculoOptions() {
    this.veiculoOptions = this.veiculosFiltrados.map(v => ({
      value: v.id!,
      label: `${v.modelo} - ${v.placa} (${v.marca})`
    }));
  }

  salvarRascunho() {
    if (this.clienteId || this.veiculoId || this.observacoesIniciais) {
      const draft = {
        clienteId: this.clienteId,
        veiculoId: this.veiculoId,
        observacoesIniciais: this.observacoesIniciais
      };
      this.storage.setItem(DRAFT_STORAGE_KEY, JSON.stringify(draft));
      this.rascunhoSalvo = true;
    }
  }

  carregarRascunho() {
    const json = this.storage.getItem(DRAFT_STORAGE_KEY);
    if (json) {
      try {
        const draft = JSON.parse(json);
        this.clienteId = draft.clienteId || '';
        this.veiculoId = draft.veiculoId || '';
        this.observacoesIniciais = draft.observacoesIniciais || '';
        this.rascunhoSalvo = true;
      } catch (e) {
        this.storage.removeItem(DRAFT_STORAGE_KEY);
      }
    }
  }

  limparRascunho() {
    this.storage.removeItem(DRAFT_STORAGE_KEY);
    this.rascunhoSalvo = false;
  }

  onClienteSelect(id: string) {
    this.clienteId = id;
    this.onClienteChange();
    this.salvarRascunho();
  }

  onVeiculoSelect(id: string) {
    this.veiculoId = id;
    this.salvarRascunho();
  }

  onClienteChange() {
    if (this.clienteId) {
      this.veiculosFiltrados = this.todosVeiculos.filter(v => v.clienteId === this.clienteId);
      if (this.veiculosFiltrados.length === 1 && this.veiculosFiltrados[0].id) {
        this.veiculoId = this.veiculosFiltrados[0].id;
      } else {
        this.veiculoId = '';
      }
    } else {
      this.veiculosFiltrados = this.todosVeiculos;
    }
    this.atualizarVeiculoOptions();
  }

  buscarCliente() {
    const doc = this.buscaDocumento.replace(/\D/g, '');
    if (doc.length >= 8) {
      const match = this.clientes.find(c => c.documento.replace(/\D/g, '').includes(doc));
      if (match && match.id) {
        this.clienteId = match.id;
        this.onClienteChange();
        this.salvarRascunho();
      }
    }
  }

  buscarVeiculoPorPlaca() {
    const p = this.buscaPlaca.toUpperCase().replace(/[^A-Z0-9]/g, '');
    if (p.length >= 4) {
      const match = this.todosVeiculos.find(v => v.placa.replace(/[^A-Z0-9]/gi, '').includes(p));
      if (match && match.id) {
        this.veiculoId = match.id;
        if (match.clienteId) {
          this.clienteId = match.clienteId;
        }
        this.salvarRascunho();
      }
    }
  }

  salvarOS() {
    if (!this.clienteId || !this.veiculoId) {
      this.notification.warning('Atenção', 'Selecione o Cliente e o Veículo para abrir a OS.');
      return;
    }

    this.loading = true;
    this.osService.criar({
      clienteId: this.clienteId,
      veiculoId: this.veiculoId,
      observacoesIniciais: this.observacoesIniciais
    }).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (osCriada) => {
        this.loading = false;
        this.limparRascunho();
        this.notification.success('OS Aberta!', `Ordem de Serviço #${osCriada.id.slice(0, 8)} registrada.`);
        this.cdr.markForCheck();
        this.router.navigate(['/ordens-servico', osCriada.id]);
      },
      error: () => {
        this.loading = false;
        this.cdr.markForCheck();
      }
    });
  }

  cancelar() {
    this.limparRascunho();
    this.router.navigate(['/ordens-servico/fila']);
  }
}
