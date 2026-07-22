import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  OrdemServico,
  CriarOSRequest,
  StatusOS,
  AdicionarServicoOSRequest,
  AdicionarInsumoOSRequest
} from '../models/ordem-servico.model';
import { API_ENDPOINTS } from '../../../core/config/api-endpoints';

@Injectable({
  providedIn: 'root'
})
export class OrdemServicoService {
  constructor(private http: HttpClient) {}

  getAll(): Observable<OrdemServico[]> {
    return this.http.get<OrdemServico[]>(API_ENDPOINTS.ORDENS_SERVICO.BASE);
  }

  getById(id: string): Observable<OrdemServico> {
    return this.http.get<OrdemServico>(API_ENDPOINTS.ORDENS_SERVICO.BY_ID(id));
  }

  getFilaKanban(): Observable<OrdemServico[]> {
    return this.http.get<OrdemServico[]>(API_ENDPOINTS.ORDENS_SERVICO.FILA_KANBAN);
  }

  buscarPorPlacaOuCpf(termo: string): Observable<OrdemServico[]> {
    const params = new HttpParams().set('termo', termo);
    return this.http.get<OrdemServico[]>(API_ENDPOINTS.ORDENS_SERVICO.CONSULTA_PUBLICA, { params });
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
}
