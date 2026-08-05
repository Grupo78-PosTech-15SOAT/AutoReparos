import { InjectionToken } from '@angular/core';

export const STORAGE_TOKEN = new InjectionToken<Storage>('storage', {
  providedIn: 'root',
  factory: () => localStorage,
});
