import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'statusOs',
  standalone: true
})
export class StatusOSPipe implements PipeTransform {
  transform(status: number | string): { label: string; cssClass: string } {
    const val = Number(status);
    switch (val) {
      case 1:
        return { label: 'Recebida', cssClass: 'status-recebida' };
      case 2:
        return { label: 'Em Diagnóstico', cssClass: 'status-diagnostico' };
      case 3:
        return { label: 'Aguardando Aprovação', cssClass: 'status-aguardando' };
      case 4:
        return { label: 'Em Execução', cssClass: 'status-execucao' };
      case 5:
        return { label: 'Finalizada', cssClass: 'status-finalizada' };
      case 6:
        return { label: 'Entregue', cssClass: 'status-entregue' };
      default:
        return { label: 'Desconhecido', cssClass: 'status-entregue' };
    }
  }
}
