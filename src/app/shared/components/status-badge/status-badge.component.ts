import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-status-badge',
  standalone: true,
  imports: [CommonModule],
  template: `
    <span class="status-badge" [ngClass]="badgeInfo.cssClass">
      <span class="dot"></span>
      {{ badgeInfo.label }}
    </span>
  `
})
export class StatusBadgeComponent {
  @Input() status!: number | string;

  get badgeInfo() {
    const val = Number(this.status);
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
