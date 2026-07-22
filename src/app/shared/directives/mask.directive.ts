import { Directive, HostListener, Input } from '@angular/core';
import { NgControl } from '@angular/forms';

@Directive({
  selector: '[appMask]',
  standalone: true
})
export class MaskDirective {
  @Input('appMask') maskType: 'cpf' | 'cnpj' | 'cpfCnpj' | 'placa' | 'telefone' = 'cpfCnpj';

  constructor(private ngControl: NgControl) {}

  @HostListener('input', ['$event'])
  onInputChange(event: Event) {
    const input = event.target as HTMLInputElement;
    let value = input.value;

    if (this.maskType === 'placa') {
      value = value.toUpperCase().replace(/[^A-Z0-9]/g, '').slice(0, 7);
    } else if (this.maskType === 'cpfCnpj') {
      const nums = value.replace(/\D/g, '').slice(0, 14);
      if (nums.length <= 11) {
        value = nums.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, '$1.$2.$3-$4');
      } else {
        value = nums.replace(/(\d{2})(\d{3})(\d{3})(\d{4})(\d{2})/, '$1.$2.$3/$4-$5');
      }
    } else if (this.maskType === 'telefone') {
      const nums = value.replace(/\D/g, '').slice(0, 11);
      if (nums.length <= 10) {
        value = nums.replace(/(\d{2})(\d{4})(\d{4})/, '($1) $2-$3');
      } else {
        value = nums.replace(/(\d{2})(\d{5})(\d{4})/, '($1) $2-$3');
      }
    }

    if (this.ngControl && this.ngControl.control) {
      this.ngControl.control.setValue(value, { emitEvent: false });
    }
  }
}
