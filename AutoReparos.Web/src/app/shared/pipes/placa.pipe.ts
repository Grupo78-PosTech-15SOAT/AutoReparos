import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'placa',
  standalone: true
})
export class PlacaPipe implements PipeTransform {
  transform(value: string | null | undefined): string {
    if (!value) return '-';
    const clean = value.toUpperCase().replace(/[^A-Z0-9]/g, '');
    if (clean.length === 7) {
      // Mercosul (ABC1D23) ou Antiga (ABC1234)
      return `${clean.substring(0, 3)}-${clean.substring(3)}`;
    }
    return value.toUpperCase();
  }
}
