import { Directive, HostListener, input, inject } from '@angular/core';
import { NgControl } from '@angular/forms';

@Directive({
  selector: '[appMask]',
  standalone: true
})
export class MaskDirective {
  maskType = input<'cpf' | 'cnpj' | 'cpfCnpj' | 'placa' | 'telefone'>('cpfCnpj', { alias: 'appMask' });
  private readonly ngControl = inject(NgControl, { optional: true, self: true });

  @HostListener('input', ['$event'])
  onInputChange(event: Event) {
    const inputEl = event.target as HTMLInputElement;
    const value = this.applyMask(inputEl.value, this.maskType());

    if (this.ngControl?.control) {
      this.ngControl.control.setValue(value, { emitEvent: false });
    }
  }

  private applyMask(value: string, type: string): string {
    if (type === 'placa') {
      return value.toUpperCase().replace(/[^A-Z0-9]/g, '').slice(0, 7);
    } else if (type === 'cpfCnpj') {
      const nums = value.replace(/\D/g, '').slice(0, 14);
      if (nums.length <= 11) {
        value = nums.replace(/(\d{2})(\d{3})(\d{3})(\d{4})(\d{2})/, '$1.$2.$3/$4-$5');
      }
    } else if (type === 'telefone') {
      const nums = value.replace(/\D/g, '').slice(0, 11);
      if (nums.length <= 10) {
        value = nums.replace(/(\d{2})(\d{4})(\d{4})/, '($1) $2-$3');
      } else {
        value = nums.replace(/(\d{2})(\d{5})(\d{4})/, '($1) $2-$3');
      }
    }

    if (this.ngControl?.control) {
      this.ngControl.control.setValue(value, { emitEvent: false });
    }
    
    return value;
  }
}
