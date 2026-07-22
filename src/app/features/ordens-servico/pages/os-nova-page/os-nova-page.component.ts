import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { OrdemServicoService } from '../../services/ordem-servico.service';
import { ClienteService } from '../../../clientes/services/cliente.service';
import { VeiculoService } from '../../../veiculos/services/veiculo.service';
import { Cliente } from '../../../clientes/models/cliente.model';
import { Veiculo } from '../../../veiculos/models/veiculo.model';
import { NotificationService } from '../../../../core/ui/notification.service';
import { MaskDirective } from '../../../../shared/directives/mask.directive';

@Component({
  selector: 'app-os-nova-page',
  standalone: true,
  imports: [CommonModule, FormsModule, MaskDirective],
  template: `
    <div class="container fade-in" style="max-width: 800px;">
      <div class="page-header">
        <div>
          <h1 class="page-title">
            <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="#ED145B" stroke-width="2.3"><path d="M12 5v14M5 12h14"/></svg>
            Abertura de Nova Ordem de Serviço
          </h1>
          <p class="page-subtitle">Recepção do veículo e registro de entrada do cliente.</p>
        </div>
      </div>

      <div class="card-panel">
        <form (ngSubmit)="salvarOS()">
          <!-- 1. Busca / Seleção do Cliente -->
          <div class="form-section">
            <h3 class="section-heading">1. Identificação do Cliente</h3>
            <div class="grid-2">
              <div class="form-group">
                <label class="form-label">Cliente Cadastrado</label>
                <select [(ngModel)]="clienteId" name="clienteId" (change)="onClienteChange()" required class="form-control">
                  <option value="">-- Selecione o Cliente --</option>
                  @for (c of clientes; track c.id) {
                    <option [value]="c.id">{{ c.nome }} ({{ c.documento }})</option>
                  }
                </select>
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
                <select [(ngModel)]="veiculoId" name="veiculoId" required class="form-control">
                  <option value="">-- Selecione o Veículo --</option>
                  @for (v of veiculosFiltrados; track v.id) {
                    <option [value]="v.id">{{ v.modelo }} - {{ v.placa }} ({{ v.marca }})</option>
                  }
                </select>
              </div>

              <div class="form-group">
                <label class="form-label">Ou Buscar por Placa</label>
                <input type="text" [(ngModel)]="buscaPlaca" name="buscaPlaca" appMask="placa" (input)="buscarVeiculoPorPlaca()" placeholder="Ex: BRA2E19" class="form-control" />
              </div>
            </div>
          </div>

          <!-- 3. Observações de Entrada -->
          <div class="form-section" style="margin-top: 1.5rem;">
            <h3 class="section-heading">3. Queixa Inicial do Cliente / Relato de Entrada</h3>
            <div class="form-group">
              <textarea [(ngModel)]="observacoesIniciais" name="observacoesIniciais" rows="4" placeholder="Descreva os sintomas relatados pelo cliente (ex: barulho na roda dianteira, luz do óleo acesa...)" class="form-control"></textarea>
            </div>
          </div>

          <!-- Ações -->
          <div style="display: flex; gap: 1rem; justify-content: flex-end; margin-top: 2rem;">
            <button type="button" (click)="cancelar()" class="btn btn-secondary">Cancelar</button>
            <button type="submit" [disabled]="loading || !clienteId || !veiculoId" class="btn btn-primary">
              Abrir OS & Iniciar Recebimento &rarr;
            </button>
          </div>
        </form>
      </div>
    </div>
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
  `]
})
export class OsNovaPageComponent implements OnInit {
  clienteId = '';
  veiculoId = '';
  observacoesIniciais = '';

  buscaDocumento = '';
  buscaPlaca = '';

  clientes: Cliente[] = [];
  todosVeiculos: Veiculo[] = [];
  veiculosFiltrados: Veiculo[] = [];

  loading = false;

  private osService = inject(OrdemServicoService);
  private clienteService = inject(ClienteService);
  private veiculoService = inject(VeiculoService);
  private notification = inject(NotificationService);
  private router = inject(Router);

  ngOnInit() {
    this.carregarDados();
  }

  carregarDados() {
    this.clienteService.getAll().subscribe(data => this.clientes = data);
    this.veiculoService.getAll().subscribe(data => {
      this.todosVeiculos = data;
      this.veiculosFiltrados = data;
    });
  }

  onClienteChange() {
    if (this.clienteId) {
      this.veiculosFiltrados = this.todosVeiculos.filter(v => v.clienteId === this.clienteId);
      if (this.veiculosFiltrados.length === 1 && this.veiculosFiltrados[0].id) {
        this.veiculoId = this.veiculosFiltrados[0].id;
      }
    } else {
      this.veiculosFiltrados = this.todosVeiculos;
    }
  }

  buscarCliente() {
    const doc = this.buscaDocumento.replace(/\D/g, '');
    if (doc.length >= 8) {
      const match = this.clientes.find(c => c.documento.replace(/\D/g, '').includes(doc));
      if (match && match.id) {
        this.clienteId = match.id;
        this.onClienteChange();
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
    }).subscribe({
      next: (osCriada) => {
        this.loading = false;
        this.notification.success('OS Aberta!', `Ordem de Serviço #${osCriada.numeroOS} registrada.`);
        this.router.navigate(['/ordens-servico', osCriada.id]);
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  cancelar() {
    this.router.navigate(['/ordens-servico/fila']);
  }
}
