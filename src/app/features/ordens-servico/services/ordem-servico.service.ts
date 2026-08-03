import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import {
  OrdemServico,
  CriarOSRequest,
  StatusOS,
  AdicionarServicoOSRequest,
  AdicionarInsumoOSRequest,
  KanbanColumn
} from '../models/ordem-servico.model';
import { API_ENDPOINTS } from '../../../core/config/api-endpoints';
import { PagedResult } from '../../../shared/models/pagination.model';

@Injectable({
  providedIn: 'root'
})
export class OrdemServicoService {
  private readonly http = inject(HttpClient);

  getAll(pageNumber = 1, pageSize = 10): Observable<PagedResult<OrdemServico>> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<unknown>(API_ENDPOINTS.ORDENS_SERVICO.BASE, { params }).pipe(
      map((res: any) => this.normalizePagedResult(res, pageNumber, pageSize))
    );
  }

  getById(id: string): Observable<OrdemServico> {
    return this.http.get<unknown>(API_ENDPOINTS.ORDENS_SERVICO.BY_ID(id)).pipe(
      map((res: any) => ({
        ...res,
        status: this.normalizeStatus(res?.status ?? res?.Status)
      }))
    );
  }

  getFilaKanban(pageNumber = 1, pageSize = 50): Observable<KanbanColumn[]> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<KanbanColumn[]>(API_ENDPOINTS.ORDENS_SERVICO.FILA_KANBAN, { params });
  }

  buscarPorPlacaOuCpf(termo: string, pageNumber = 1, pageSize = 10): Observable<PagedResult<OrdemServico>> {
    const params = new HttpParams()
      .set('termo', termo)
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<unknown>(API_ENDPOINTS.ORDENS_SERVICO.CONSULTA_PUBLICA, { params }).pipe(
      map((res: any) => this.normalizePagedResult(res, pageNumber, pageSize))
    );
  }

  criar(req: CriarOSRequest): Observable<OrdemServico> {
    return this.http.post<OrdemServico>(API_ENDPOINTS.ORDENS_SERVICO.BASE, req);
  }

  atualizarStatus(id: string, novoStatus: StatusOS): Observable<OrdemServico> {
    return this.http.patch<OrdemServico>(API_ENDPOINTS.ORDENS_SERVICO.STATUS(id), { novoStatus });
  }

  registrarDiagnostico(id: string, observacoes: string): Observable<OrdemServico> {
    return this.http.put<OrdemServico>(API_ENDPOINTS.ORDENS_SERVICO.DIAGNOSTICO(id), { observacoes });
  }

  adicionarServico(osId: string, req: AdicionarServicoOSRequest): Observable<OrdemServico> {
    return this.http.post<OrdemServico>(API_ENDPOINTS.ORDENS_SERVICO.SERVICOS(osId), req);
  }

  adicionarInsumo(osId: string, req: AdicionarInsumoOSRequest): Observable<OrdemServico> {
    return this.http.post<OrdemServico>(API_ENDPOINTS.ORDENS_SERVICO.INSUMOS(osId), req);
  }

  alternarStatusItemServico(osId: string, itemId: string, concluido: boolean): Observable<void> {
    return this.http.put<void>(API_ENDPOINTS.ORDENS_SERVICO.SERVICO_STATUS(osId, itemId), { concluido });
  }

  enviarParaAprovacao(osId: string): Observable<void> {
    return this.http.post<void>(API_ENDPOINTS.ORDENS_SERVICO.ENVIAR_APROVACAO(osId), {});
  }

  responderOrcamentoToken(token: string, aprovado: boolean): Observable<void> {
    return this.http.post<void>(API_ENDPOINTS.ORDENS_SERVICO.RESPONDER_ORCAMENTO, { token, aprovado });
  }

  entregarVeiculo(osId: string): Observable<OrdemServico> {
    return this.http.post<OrdemServico>(API_ENDPOINTS.ORDENS_SERVICO.ENTREGAR(osId), {});
  }

  private normalizePagedResult(res: any, pageNumber: number, pageSize: number): PagedResult<OrdemServico> {
    if (res) {
      const items = res.items ?? res.Items ?? res.data ?? res.Data ?? (Array.isArray(res) ? res : null);
      if (Array.isArray(items)) {
        const total = res.totalItems ?? res.TotalItems ?? res.total ?? res.Total ?? items.length;
        const pNum = res.pageNumber ?? res.PageNumber ?? pageNumber ?? 1;
        const pSize = res.pageSize ?? res.PageSize ?? pageSize ?? 10;
        const totalPages = res.totalPages ?? res.TotalPages ?? Math.max(1, Math.ceil(total / (pSize || 1)));
        return {
          items: items.map(item => ({
            ...item,
            status: this.normalizeStatus(item.status ?? item.Status)
          })),
          total,
          pageNumber: pNum,
          pageSize: pSize,
          totalPages
        };
      }
    }
    return { items: [], total: 0, pageNumber: pageNumber || 1, pageSize: pageSize || 10, totalPages: 1 };
  }

  /** A API expõe o enum como texto; a tela usa os valores numéricos de StatusOS. */
  private normalizeStatus(status: unknown): StatusOS {
    if (typeof status === 'number' && status in StatusOS) {
      return status;
    }

    const normalized = String(status)
      .normalize('NFD')
      .replace(/[\u0300-\u036f]/g, '')
      .replace(/[\s_-]/g, '')
      .toLowerCase();

    const statuses: Record<string, StatusOS> = {
      recebida: StatusOS.Recebida,
      emdiagnostico: StatusOS.EmDiagnostico,
      aguardandoaprovacao: StatusOS.AguardandoAprovacao,
      emexecucao: StatusOS.EmExecucao,
      finalizada: StatusOS.Finalizada,
      entregue: StatusOS.Entregue
    };

    return statuses[normalized] ?? (status as StatusOS);
  }
}
