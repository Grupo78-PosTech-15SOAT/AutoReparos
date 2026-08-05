import { Component, ChangeDetectionStrategy, input, output, computed } from '@angular/core';
import { KanbanCard } from '../../models/ordem-servico.model';

@Component({
  template: '',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export abstract class BaseKanbanCardComponent {
  card = input.required<KanbanCard>();
  cardClick = output<KanbanCard>();

  /**
   * Propriedade configurável centralizada para definir se o ID da OS deve ser exibido na capa de todos os cards.
   * Defina como `true` caso deseje reexibir o #ID em todos os cards do Kanban.
   */
  readonly showOsId = false;

  formattedId = computed(() => this.card()?.id ? `#${this.card().id}` : '');
  shortId = computed(() => this.card()?.id ? `#${this.card().id.slice(0, 8)}` : '');
}
