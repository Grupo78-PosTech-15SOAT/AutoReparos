import { Component, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { OrdemServicoService } from '../../../ordens-servico/services/ordem-servico.service';
import { OrdemServico } from '../../../ordens-servico/models/ordem-servico.model';
import { StatusBadgeComponent } from '../../../../shared/components/status-badge/status-badge.component';
import { NotificationService } from '../../../../core/ui/notification.service';

@Component({
  selector: 'app-consulta-publica-page',
  standalone: true,
  imports: [CommonModule, FormsModule, StatusBadgeComponent],
  template: `
    <div class="container fade-in" style="max-width: 850px; padding-top: 3rem;">
      <!-- Banner Público -->
      <div class="public-banner">
        <div class="brand-badge">Portal do Cliente</div>
        <h1 class="public-title">Consulta de Veículo</h1>

        <!-- Campo de Pesquisa -->
        <form (ngSubmit)="consultar()" class="search-form-group">
          <div class="search-input-wrapper">
            <input
              type="text"
              [(ngModel)]="termoBusca"
              name="termoBusca"
              placeholder="Placa, CPF ou CNPJ..."
              class="form-control search-input"
            />
            <button type="submit" [disabled]="loading || !termoBusca.trim()" class="btn btn-primary search-btn">
              🔍 Consultar
            </button>
          </div>
        </form>
      </div>

      <!-- Lista de Resultados -->
      @if (buscou || loading) {
        <div class="results-container table-loading-container" style="position: relative; min-height: 150px;">
          @if (loading) {
            <div class="table-loading-overlay">
              <div class="table-loading-spinner"></div>
              <span class="table-loading-text">Buscando informações...</span>
            </div>
          }
          @if (buscou && !loading) {
            @for (os of ordensEncontradas; track os.id) {
              <div class="card-panel os-public-card">
              <div class="os-header flex-between">
                <div>
                  <span class="mono-badge" style="color: #ED145B;">OS #{{ os.numeroOS }}</span>
                  <h3 class="vehicle-title">{{ os.modeloVeiculo }}</h3>
                </div>
                <app-status-badge [status]="os.status"></app-status-badge>
              </div>

              <div class="os-info-grid">
                <div>
                  <span class="info-label">Proprietário:</span>
                  <div class="info-val">{{ os.clienteNome }}</div>
                </div>
                <div>
                  <span class="info-label">Placa:</span>
                  <div class="info-val mono-badge">{{ os.placaVeiculo }}</div>
                </div>
                <div>
                  <span class="info-label">Data de Entrada:</span>
                  <div class="info-val">{{ os.dataAbertura | date:'dd/MM/yyyy HH:mm' }}</div>
                </div>
                <div>
                  <span class="info-label">Valor Orçado:</span>
                  <div class="info-val total-highlight">R$ {{ os.valorTotal | number:'1.2-2' }}</div>
                </div>
              </div>

              <!-- Lista de Serviços -->
              @if (os.itensServico && os.itensServico.length > 0) {
                <div class="services-list-box">
                  <div class="box-title">Serviços em Execução:</div>
                  @for (s of os.itensServico; track s.id) {
                    <div class="service-item flex-between">
                      <span>{{ s.nomeServico }}</span>
                      <span [style.color]="s.concluido ? '#10B981' : '#F59E0B'" style="font-weight: 600;">
                        {{ s.concluido ? '✓ Concluído' : 'Em andamento...' }}
                      </span>
                    </div>
                  }
                </div>
              }
            </div>
          } @empty {
            <div class="card-panel empty-card">
              <div style="font-size: 2rem; margin-bottom: 0.5rem;">🔍</div>
              <h3 style="font-family: 'Outfit', sans-serif; color: #fff;">Nenhuma OS Encontrada</h3>
              <p style="color: #A1A1AA; font-size: 0.9rem;">Verifique os dados informados e tente novamente.</p>
            </div>
          }
        }
      </div>
    }
  </div>
  `,
  styles: [`
    .public-banner {
      background: rgba(24, 24, 28, 0.9);
      backdrop-filter: blur(16px);
      border: 1px solid rgba(237, 20, 91, 0.3);
      border-radius: 16px;
      padding: 2.75rem 2.25rem;
      text-align: center;
      box-shadow: 0 20px 40px rgba(0, 0, 0, 0.8);
      margin-bottom: 2rem;
    }
    .brand-badge {
      display: inline-block;
      background: rgba(237, 20, 91, 0.15);
      color: #ED145B;
      border: 1px solid rgba(237, 20, 91, 0.4);
      padding: 0.3rem 0.85rem;
      border-radius: 999px;
      font-size: 0.75rem;
      font-weight: 700;
      text-transform: uppercase;
      letter-spacing: 0.05em;
      margin-bottom: 1rem;
    }
    .public-title {
      font-family: 'Outfit', sans-serif;
      font-size: 2.1rem;
      font-weight: 800;
      color: #ffffff;
    }
    .public-desc {
      color: #A1A1AA;
      font-size: 0.95rem;
      max-width: 580px;
      margin: 0.5rem auto 1.75rem;
      line-height: 1.55;
    }
    .search-form-group {
      max-width: 620px;
      margin: 0 auto;
      display: flex;
      flex-direction: column;
      gap: 0.6rem;
    }
    .search-input-wrapper {
      display: flex;
      gap: 0.75rem;
      flex-wrap: wrap;
    }
    .search-input {
      font-size: 0.98rem;
      padding: 0.8rem 1.1rem;
      flex: 1;
      min-width: 260px;
    }
    .search-btn { padding: 0.8rem 1.4rem; font-size: 0.92rem; border-radius: 8px; }

    .search-hint {
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 0.4rem;
      color: #71717A;
      font-size: 0.82rem;
      margin-top: 0.25rem;
    }
    .hint-code {
      font-family: 'JetBrains Mono', monospace;
      color: #E2E8F0;
      background: rgba(255, 255, 255, 0.06);
      padding: 0.15rem 0.45rem;
      border-radius: 4px;
      border: 1px solid rgba(255, 255, 255, 0.1);
    }

    .results-container { display: flex; flex-direction: column; gap: 1.5rem; }
    .flex-between { display: flex; justify-content: space-between; align-items: center; }
    .vehicle-title { font-family: 'Outfit', sans-serif; font-size: 1.25rem; font-weight: 700; color: #fff; margin-top: 0.25rem; }

    .os-info-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(160px, 1fr));
      gap: 1rem;
      background: rgba(10, 10, 12, 0.6);
      border-radius: 8px;
      padding: 1rem;
      margin: 1.25rem 0;
    }
    .info-label { font-size: 0.75rem; color: #A1A1AA; text-transform: uppercase; }
    .info-val { font-size: 0.95rem; font-weight: 600; color: #F8FAFC; margin-top: 0.2rem; }
    .total-highlight { font-family: 'JetBrains Mono', monospace; color: #10B981; font-weight: 700; font-size: 1.1rem; }

    .services-list-box {
      border-top: 1px solid rgba(255, 255, 255, 0.08);
      padding-top: 1rem;
    }
    .box-title { font-size: 0.85rem; font-weight: 600; color: #ED145B; margin-bottom: 0.5rem; }
    .service-item { padding: 0.4rem 0; font-size: 0.875rem; border-bottom: 1px solid rgba(255, 255, 255, 0.04); }
    .empty-card { text-align: center; padding: 3rem; }
  `]
})
export class ConsultaPublicaPageComponent {
  termoBusca = '';
  loading = false;
  buscou = false;
  ordensEncontradas: OrdemServico[] = [];

  private osService = inject(OrdemServicoService);
  private notification = inject(NotificationService);
  private cdr = inject(ChangeDetectorRef);

  consultar() {
    if (!this.termoBusca.trim()) {
      this.notification.warning('Atenção', 'Digite uma Placa ou CPF para consultar.');
      return;
    }

    this.loading = true;
    this.cdr.detectChanges();
    this.osService.buscarPorPlacaOuCpf(this.termoBusca.trim()).subscribe({
      next: (res) => {
        this.loading = false;
        this.buscou = true;
        this.ordensEncontradas = res.items || [];
        this.cdr.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.buscou = true;
        this.ordensEncontradas = [];
        this.cdr.detectChanges();
      }
    });
  }
}
