import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'cpfCnpj',
  standalone: true
})
export class CpfCnpjPipe implements PipeTransform {
  transform(value: string | null | undefined): string {
    if (!value) return '-';
    const raw = value.replace(/\D/g, '');
    if (raw.length === 11) {
      return raw.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, '$1.$2.$3-$4');
    }
    if (raw.length === 14) {
      return raw.replace(/(\d{2})(\d{3})(\d{3})(\d{4})(\d{2})/, '$1.$2.$3/$4-$5');
    }
    return value;
  }
}
