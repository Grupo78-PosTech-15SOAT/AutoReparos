import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {  Component, OnInit, inject, ChangeDetectionStrategy, ChangeDetectorRef , DestroyRef } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { OrdemServicoService } from '../../services/ordem-servico.service';
import { InsumoService } from '../../../insumos/services/insumo.service';
import { ServicoService } from '../../../servicos/services/servico.service';
import { ClienteService } from '../../../clientes/services/cliente.service';
import { VeiculoService } from '../../../veiculos/services/veiculo.service';
import { OrdemServico, StatusOS } from '../../models/ordem-servico.model';
import { Insumo } from '../../../insumos/models/insumo.model';
import { Servico } from '../../../servicos/models/servico.model';
import { StatusBadgeComponent } from '../../../../shared/components/status-badge/status-badge.component';
import { NotificationService } from '../../../../core/ui/notification.service';
import { AuthService } from '../../../auth/services/auth.service';
import { CustomSelectComponent, SelectOption } from '../../../../shared/components/custom-select/custom-select.component';
import { PageContainerComponent } from '../../../../shared/components/page-container/page-container.component';
import { IdBadgeComponent } from '../../../../shared/components/id-badge/id-badge.component';
import { PlacaBadgeComponent } from '../../../../shared/components/placa-badge/placa-badge.component';

@Component({
  selector: 'app-os-detalhe-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    DatePipe, DecimalPipe, 
    FormsModule, 
    StatusBadgeComponent, 
    CustomSelectComponent,
    PageContainerComponent,
    IdBadgeComponent,
    PlacaBadgeComponent
  ],
  template: `
    @if (os) {
      <app-page-container>
        <!-- Top Bar Header -->
        <div class="page-header">
          <div>
            <div style="display: flex; align-items: center; gap: 0.85rem; margin-bottom: 0.3rem;">
              <app-id-badge [text]="'#' + os.id.substring(0, 8)" size="lg"></app-id-badge>
              <app-status-badge [status]="os.status"></app-status-badge>
            </div>
            <h1 class="page-title">{{ getVeiculoDesc(os.veiculoId) }} <app-placa-badge [placa]="getVeiculoPlaca(os.veiculoId)" size="lg" style="vertical-align: middle;"></app-placa-badge></h1>
            <p class="page-subtitle">Proprietário: {{ getClienteNome(os.clienteId) }} | Data de Entrada: {{ os.dataAbertura | date:'dd/MM/yyyy HH:mm' }}</p>
          </div>

          <!-- Ações Rápidas de Transição de Status -->
          <div style="display: flex; gap: 0.75rem; flex-wrap: wrap;">
            @if (os.status === StatusOS.Recebida) {
              <button (click)="alterarStatus(StatusOS.EmDiagnostico)" class="btn btn-primary">
                🔧 Iniciar Diagnóstico
              </button>
            }

            @if (os.status === StatusOS.EmDiagnostico) {
              <button (click)="enviarParaAprovacao()" class="btn btn-accent">
                ✉️ Enviar Orçamento
              </button>
            }

            @if (os.status === StatusOS.AguardandoAprovacao) {
              <button (click)="alterarStatus(StatusOS.EmExecucao)" class="btn btn-primary">
                ▶️ Iniciar Execução
              </button>
            }

            @if (os.status === StatusOS.EmExecucao) {
              <button (click)="alterarStatus(StatusOS.Finalizada)" class="btn btn-success">
                ✅ Finalizar OS
              </button>
            }
          </div>
        </div>

        <!-- Grid Workbench 2 Colunas -->
        <div class="workbench-grid">
          <!-- Coluna Esquerda: Diagnóstico & Peças/Serviços -->
          <div class="main-column">

            <!-- Diagnóstico do Mecânico -->
            <div class="card-panel">
              <h3 class="card-title">📝 Diagnóstico do Mecânico</h3>
              <div class="form-group" style="margin-top: 1rem;">
                <textarea [(ngModel)]="os.observacoesDiagnostico" rows="3" placeholder="Insira o laudo técnico do veículo..." class="form-control"></textarea>
              </div>
              <button (click)="salvarDiagnostico()" class="btn btn-secondary btn-sm" style="margin-top: 0.5rem;">
                Salvar Diagnóstico
              </button>
            </div>

            <!-- Tabela de Mão de Obra e Serviços -->
            <div class="card-panel" style="margin-top: 1.5rem;">
              <div class="card-header-flex">
                <h3 class="card-title">🛠️ Serviços de Mão de Obra</h3>
                <div style="display: flex; gap: 0.5rem; align-items: center; flex-wrap: wrap;">
                  <div style="width: 260px;">
                    <app-custom-select
                      [options]="servicoOptions"
                      [value]="servicoIdSelecionado"
                      [loading]="loadingCatalogos"
                      placeholder="-- Selecionar Serviço --"
                      (valueChange)="servicoIdSelecionado = $event"
                    ></app-custom-select>
                  </div>
                  <button (click)="adicionarServico()" [disabled]="!servicoIdSelecionado || loadingCatalogos" class="btn btn-primary btn-sm">+ Adicionar</button>
                </div>
              </div>

              <table class="data-table" style="margin-top: 1rem;">
                <thead>
                  <tr>
                    <th>Serviço</th>
                    <th style="text-align: center;">Valor</th>
                    <th>Status Execução</th>
                  </tr>
                </thead>
                <tbody>
                  @for (item of os.itensServico; track item.id) {
                    <tr>
                      <td style="font-weight: 600;">{{ item.nomeServico }}</td>
                      <td style="text-align: center;">R$ {{ item.valor | number:'1.2-2' }}</td>
                      <td>
                        <label style="display: inline-flex; align-items: center; gap: 0.5rem; cursor: pointer;">
                          <input type="checkbox" [checked]="item.concluido" (change)="alternarServico(item)" />
                          <span [style.color]="item.concluido ? '#10B981' : '#F59E0B'" style="font-weight: 600; font-size: 0.85rem;">
                            {{ item.concluido ? '✓ Concluído' : 'Pendente' }}
                          </span>
                        </label>
                      </td>
                    </tr>
                  } @empty {
                    <tr><td colspan="3" class="empty-text">Nenhum serviço adicionado.</td></tr>
                  }
                </tbody>
              </table>
            </div>

            <!-- Tabela de Peças & Insumos Utilizados -->
            <div class="card-panel" style="margin-top: 1.5rem;">
              <div class="card-header-flex">
                <h3 class="card-title">📦 Peças e Insumos</h3>
                <div style="display: flex; gap: 0.5rem; align-items: center; flex-wrap: wrap;">
                  <div style="width: 260px;">
                    <app-custom-select
                      [options]="insumoOptions"
                      [value]="insumoIdSelecionado"
                      [loading]="loadingCatalogos"
                      placeholder="-- Selecionar Peça --"
                      (valueChange)="insumoIdSelecionado = $event"
                    ></app-custom-select>
                  </div>
                  <input type="number" [(ngModel)]="quantidadeInsumo" min="1" class="form-control" style="width: 70px; padding: 0.4rem 0.5rem; height: 38px;" />
                  <button (click)="adicionarInsumo()" [disabled]="!insumoIdSelecionado || loadingCatalogos" class="btn btn-primary btn-sm">+ Adicionar</button>
                </div>
              </div>

              <table class="data-table" style="margin-top: 1rem;">
                <thead>
                  <tr>
                    <th>Peça/Insumo</th>
                    <th style="text-align: center;">Qtd</th>
                    <th style="text-align: center;">Unitário</th>
                    <th style="text-align: center;">Subtotal</th>
                  </tr>
                </thead>
                <tbody>
                  @for (item of os.itensInsumo; track item.id) {
                    <tr>
                      <td style="font-weight: 600;">{{ item.nomeInsumo }}</td>
                      <td style="text-align: center;">{{ item.quantidade }}x</td>
                      <td style="text-align: center;">R$ {{ item.valorUnitario | number:'1.2-2' }}</td>
                      <td style="font-weight: 700; color: #E2E8F0; text-align: center;">R$ {{ item.valorTotal | number:'1.2-2' }}</td>
                    </tr>
                  } @empty {
                    <tr><td colspan="4" class="empty-text">Nenhuma peça adicionada.</td></tr>
                  }
                </tbody>
              </table>
            </div>

          </div>

          <!-- Coluna Direita: Resumo Financeiro & Informações -->
          <div class="side-column">
            <div class="card-panel sticky-panel">
              <h3 class="card-title">💰 Resumo Financeiro</h3>

              <div class="summary-list">
                <div class="summary-item">
                  <span>Subtotal Serviços:</span>
                  <span>R$ {{ calcularSubtotalServicos() | number:'1.2-2' }}</span>
                </div>
                <div class="summary-item">
                  <span>Subtotal Insumos:</span>
                  <span>R$ {{ calcularSubtotalInsumos() | number:'1.2-2' }}</span>
                </div>
                <div class="summary-divider"></div>
                <div class="summary-total">
                  <span>VALOR TOTAL:</span>
                  <span class="total-amount">R$ {{ os.valorTotal | number:'1.2-2' }}</span>
                </div>
              </div>

              @if (os.approvalToken) {
                <div class="token-box">
                  <div style="font-size: 0.75rem; color: #A1A1AA; margin-bottom: 0.3rem;">Token de Aprovação:</div>
                  <code class="token-code">{{ os.approvalToken }}</code>
                </div>
              }
            </div>
          </div>
        </div>
      </app-page-container>
    }
  `,
  styles: [`
    .workbench-grid {
      display: grid;
      grid-template-columns: 1fr 340px;
      gap: 1.5rem;
    }
    @media (max-width: 900px) {
      .workbench-grid { grid-template-columns: 1fr; }
    }
    .card-title {
      font-family: 'Outfit', sans-serif;
      font-size: 1.1rem;
      font-weight: 700;
      color: #ffffff;
    }
    .card-header-flex {
      display: flex;
      justify-content: space-between;
      align-items: center;
      flex-wrap: wrap;
      gap: 0.75rem;
    }
    .form-control-sm { padding: 0.4rem 0.6rem; font-size: 0.85rem; }
    .btn-sm { padding: 0.4rem 0.85rem; font-size: 0.8rem; }
    .empty-text { text-align: center; color: #71717A; padding: 1.5rem; }
    .summary-list { display: flex; flex-direction: column; gap: 0.75rem; margin-top: 1.25rem; }
    .summary-item { display: flex; justify-content: space-between; color: #A1A1AA; font-size: 0.9rem; }
    .summary-divider { height: 1px; background: rgba(255, 255, 255, 0.08); margin: 0.5rem 0; }
    .summary-total { display: flex; justify-content: space-between; align-items: center; font-weight: 700; font-size: 1rem; color: #ffffff; }
    .total-amount { font-family: 'JetBrains Mono', monospace; font-size: 1.35rem; color: #10B981; }
    .token-box { background: rgba(10, 10, 12, 0.8); border: 1px solid rgba(237, 20, 91, 0.3); border-radius: 8px; padding: 0.75rem; margin-top: 1.5rem; }
    .token-code { font-family: 'JetBrains Mono', monospace; font-size: 0.75rem; color: #ED145B; word-break: break-all; }
  `]
})
export class OsDetalhePageComponent implements OnInit {
  private destroyRef = inject(DestroyRef);
  StatusOS = StatusOS;
  osId!: string;
  os!: OrdemServico;

  servicosDisponiveis: Servico[] = [];
  insumosDisponiveis: Insumo[] = [];

  servicoOptions: SelectOption[] = [];
  insumoOptions: SelectOption[] = [];

  servicoIdSelecionado = '';
  insumoIdSelecionado = '';
  quantidadeInsumo = 1;
  loadingCatalogos = true;
  clientes: any[] = [];
  veiculos: any[] = [];

  private route = inject(ActivatedRoute);
  private osService = inject(OrdemServicoService);
  private servicoService = inject(ServicoService);
  private insumoService = inject(InsumoService);
  private clienteService = inject(ClienteService);
  private veiculoService = inject(VeiculoService);
  private notification = inject(NotificationService);

  ngOnInit() {
    this.osId = this.route.snapshot.paramMap.get('id') || '';
    if (this.osId) {
      this.carregarOS();
      this.carregarCatalogos();
    }
  }

  carregarOS() {
    this.osService.getById(this.osId).pipe(takeUntilDestroyed(this.destroyRef)).subscribe(data => this.os = data as unknown as OrdemServico);
    this.clienteService.getAll(1, 500).pipe(takeUntilDestroyed(this.destroyRef)).subscribe(c => this.clientes = c.items || []);
    this.veiculoService.getAll(1, 500).pipe(takeUntilDestroyed(this.destroyRef)).subscribe(v => this.veiculos = v.items || []);
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

  carregarCatalogos() {
    this.loadingCatalogos = true;
    let s = false, i = false;
    const check = () => { if (s && i) this.loadingCatalogos = false; };

    this.servicoService.getAll(1, 100).pipe(takeUntilDestroyed(this.destroyRef)).subscribe(res => {
      this.servicosDisponiveis = res.items || [];
      this.servicoOptions = this.servicosDisponiveis.map(sv => ({
        value: sv.id!,
        label: `${sv.nome} (R$ ${sv.valorTabelado.toFixed(2)})`
      }));
      s = true; check();
    });
    this.insumoService.getAll(1, 100).pipe(takeUntilDestroyed(this.destroyRef)).subscribe(res => {
      this.insumosDisponiveis = res.items || [];
      this.insumoOptions = this.insumosDisponiveis.map(ins => ({
        value: ins.id!,
        label: `${ins.nome} (Estoque: ${ins.quantidadeEstoque})`
      }));
      i = true; check();
    });
  }

  alterarStatus(novoStatus: StatusOS) {
    this.osService.atualizarStatus(this.osId, novoStatus).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (atualizada) => {
        this.os = atualizada;
        this.notification.success('Status Atualizado', 'Status da OS alterado com sucesso.');
      }
    });
  }

  salvarDiagnostico() {
    if (!this.os.observacoesDiagnostico) return;
    this.osService.registrarDiagnostico(this.osId, this.os.observacoesDiagnostico).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.notification.success('Diagnóstico Salvo', 'Laudo de diagnóstico registrado.');
      }
    });
  }

  adicionarServico() {
    if (!this.servicoIdSelecionado) return;
    this.osService.adicionarServico(this.osId, { servicoId: this.servicoIdSelecionado }).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (atualizada) => {
        this.os = atualizada;
        this.servicoIdSelecionado = '';
        this.notification.success('Serviço Adicionado', 'Item incluído na ordem.');
      }
    });
  }

  adicionarInsumo() {
    if (!this.insumoIdSelecionado || this.quantidadeInsumo < 1) return;
    this.osService.adicionarInsumo(this.osId, { insumoId: this.insumoIdSelecionado, quantidade: this.quantidadeInsumo }).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (atualizada) => {
        this.os = atualizada;
        this.insumoIdSelecionado = '';
        this.quantidadeInsumo = 1;
        this.notification.success('Peça Adicionada', 'Insumo incluído no orçamento.');
      }
    });
  }

  alternarServico(item: any) {
    const novoStatus = !item.concluido;
    this.osService.alternarStatusItemServico(this.osId, item.id, novoStatus).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        item.concluido = novoStatus;
        this.notification.info('Item Atualizado', `Serviço marcado como ${novoStatus ? 'Concluído' : 'Pendente'}.`);
      }
    });
  }

  enviarParaAprovacao() {
    this.osService.enviarParaAprovacao(this.osId).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.notification.success('Orçamento Enviado', 'E-mail enviado ao cliente com link de aprovação.');
        this.carregarOS();
      }
    });
  }

  calcularSubtotalServicos(): number {
    if (!this.os?.itensServico) return 0;
    return this.os.itensServico.reduce((acc, curr) => acc + curr.valor, 0);
  }

  calcularSubtotalInsumos(): number {
    if (!this.os?.itensInsumo) return 0;
    return this.os.itensInsumo.reduce((acc, curr) => acc + curr.valorTotal, 0);
  }
}
