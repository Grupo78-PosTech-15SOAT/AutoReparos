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

@Injectable({
  providedIn: 'root'
})
export class OrdemServicoService {
  private apiUrl = 'http://localhost:8080/api/v1/ordensservico';

  constructor(private http: HttpClient) {}

  getAll(): Observable<OrdemServico[]> {
    return this.http.get<OrdemServico[]>(this.apiUrl);
  }

  getById(id: string): Observable<OrdemServico> {
    return this.http.get<OrdemServico>(`${this.apiUrl}/${id}`);
  }

  getFilaKanban(): Observable<OrdemServico[]> {
    return this.http.get<OrdemServico[]>(`${this.apiUrl}/fila-kanban`);
  }

  buscarPorPlacaOuCpf(termo: string): Observable<OrdemServico[]> {
    const params = new HttpParams().set('termo', termo);
    return this.http.get<OrdemServico[]>(`${this.apiUrl}/consulta-publica`, { params });
  }

  criar(req: CriarOSRequest): Observable<OrdemServico> {
    return this.http.post<OrdemServico>(this.apiUrl, req);
  }

  atualizarStatus(id: string, novoStatus: StatusOS): Observable<OrdemServico> {
    return this.http.patch<OrdemServico>(`${this.apiUrl}/${id}/status`, { novoStatus });
  }

  registrarDiagnostico(id: string, observacoes: string): Observable<OrdemServico> {
    return this.http.put<OrdemServico>(`${this.apiUrl}/${id}/diagnostico`, { observacoes });
  }

  adicionarServico(osId: string, req: AdicionarServicoOSRequest): Observable<OrdemServico> {
    return this.http.post<OrdemServico>(`${this.apiUrl}/${osId}/servicos`, req);
  }

  adicionarInsumo(osId: string, req: AdicionarInsumoOSRequest): Observable<OrdemServico> {
    return this.http.post<OrdemServico>(`${this.apiUrl}/${osId}/insumos`, req);
  }

  alternarStatusItemServico(osId: string, itemId: string, concluido: boolean): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${osId}/servicos/${itemId}/status`, { concluido });
  }

  enviarParaAprovacao(osId: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${osId}/enviar-aprovacao`, {});
  }

  responderOrcamentoToken(token: string, aprovado: boolean): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/aprovar-orcamento`, { token, aprovado });
  }

  entregarVeiculo(osId: string): Observable<OrdemServico> {
    return this.http.post<OrdemServico>(`${this.apiUrl}/${osId}/entregar`, {});
  }
}
