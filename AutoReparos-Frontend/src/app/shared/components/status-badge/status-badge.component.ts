import { Component, ChangeDetectionStrategy, computed, input } from '@angular/core';
import { getStatusOsInfo } from '../../../shared/utils/status-os.utils';

@Component({
  selector: 'app-status-badge',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <span class="status-badge" [class]="badgeInfo().cssClass + ' ' + size()">
      <span class="dot"></span>
      {{ badgeInfo().label }}
    </span>
  `
})
export class StatusBadgeComponent {
  status = input.required<number | string>();
  size = input<'sm' | 'md'>('md');

  badgeInfo = computed(() => getStatusOsInfo(this.status()));
}
