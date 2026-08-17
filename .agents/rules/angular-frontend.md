# Angular Frontend Architecture & Standards

This rule defines the mandatory standards and best practices for the Angular frontend application (`AutoReparos.Web`) built with Angular 22, TypeScript, Signals, and Vitest.

---

## 1. Core Principles & Standalone Paradigm

- **Angular 22+ Standalone Only**: All components, directives, and pipes must be standalone (`standalone: true` is default in v22). **Do NOT create `NgModule`**.
- **Strict TypeScript**: `noImplicitAny: true`, `strictNullChecks: true`, `noUnusedLocals: true`. Never use `any`; use strict interfaces or generic types.
- **Dependency Injection**: Prefer the `inject()` function over constructor injection.

```typescript
// GOOD: Standalone component with inject()
@Component({
  selector: 'app-cliente-list',
  standalone: true,
  imports: [CommonModule, RouterLink, ReactiveFormsModule],
  templateUrl: './cliente-list.component.html',
  styleUrl: './cliente-list.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ClienteListComponent {
  private readonly clienteService = inject(ClienteService);
}
```

---

## 2. Control Flow Syntax

Always use native Angular control flow syntax (`@if`, `@for`, `@switch`) instead of structural directives (`*ngIf`, `*ngFor`, `*ngSwitch`).

```html
<!-- GOOD: Native Control Flow -->
@if (loading()) {
  <app-spinner />
} @else if (error()) {
  <div class="alert alert-danger">{{ error() }}</div>
} @else {
  <ul>
    @for (cliente of clientes(); track cliente.id) {
      <li>{{ cliente.nome }} - {{ cliente.documento }}</li>
    } @empty {
      <p>Nenhum cliente encontrado.</p>
    }
  </ul>
}
```

---

## 3. Signal-Based State Management

1. **State Ownership**: Primary state must be declared using `signal()` or `linkedSignal()`.
2. **Derived State**: Always use `computed()` for read-only derived values. Do not manually synchronize signals inside subscriptions or effects.
3. **Signal Inputs & Outputs**:
   - Inputs: `input<string>()` or `input.required<string>()`.
   - Outputs: `output<Cliente>()`.
   - Two-way bindings: `model<string>()`.
4. **Async Data Loading**: Use Angular's `resource()` or `rxResource()` for API fetching or `toSignal()` for RxJS streams.

```typescript
// GOOD: Signal API & Component Inputs/Outputs
export class ClienteDetailComponent {
  // Inputs & Outputs
  readonly clienteId = input.required<string>();
  readonly onUpdated = output<void>();

  // State & Signals
  private readonly clienteService = inject(ClienteService);

  readonly clienteResource = resource({
    request: () => ({ id: this.clienteId() }),
    loader: ({ request }) => firstValueFrom(this.clienteService.getById(request.id)),
  });

  readonly isVip = computed(() => {
    const data = this.clienteResource.value();
    return data ? data.ordensCount > 10 : false;
  });
}
```

---

## 4. RxJS & Signals Interoperability

- Keep RxJS for complex event-driven streams (debouncing search inputs, websocket streams, multi-step user actions).
- Convert RxJS observables to Signals at component boundaries using `toSignal()`.

```typescript
export class ClienteSearchComponent {
  private readonly clienteService = inject(ClienteService);

  readonly searchControl = new FormControl('', { nonNullable: true });

  readonly searchResults = toSignal(
    this.searchControl.valueChanges.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      switchMap((query) => this.clienteService.search(query))
    ),
    { initialValue: [] }
  );
}
```

---

## 5. Services & HTTP Client

- Declare services with `{ providedIn: 'root' }`.
- Use functional HTTP interceptors (`provideHttpClient(withInterceptors([authInterceptor, errorInterceptor]))`).
- Return strongly typed Observables or Promises from services.

```typescript
@Injectable({ providedIn: 'root' })
export class ClienteService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/clientes';

  getAll(params: ClientePagedRequest): Observable<PagedResult<ClienteDto>> {
    return this.http.get<PagedResult<ClienteDto>>(this.baseUrl, { params: { ...params } });
  }
}
```

---

## 6. Forms & Validation

- Use **Typed Reactive Forms** (`FormGroup`, `FormControl`).
- Avoid Untyped forms (`UntypedFormGroup`).
- Store validation error messages cleanly in helper methods or custom error components.

```typescript
readonly form = inject(FormBuilder).nonNullable.group({
  nome: ['', [Validators.required, Validators.maxLength(100)]],
  documento: ['', [Validators.required]],
  email: ['', [Validators.required, Validators.email]],
  telefone: ['', [Validators.required]],
});
```

---

## 7. Styling Standards & Component Isolation

- Use **SCSS** with `Emulated` view encapsulation (default).
- Maintain responsive layout with CSS Grid / Flexbox and CSS custom properties (variables).
- Follow clean naming rules (BEM or module-scoped utility classes).

---

## 8. Unit Testing with Vitest

- Framework: **Vitest** + `@angular/build`.
- Unit tests should test component logic, signals, and template rendering without excessive mocking.
- Mock HTTP responses cleanly using `HttpTestingController`.

```typescript
import { render, screen } from '@testing-library/angular'; // or TestBed
import { describe, it, expect, beforeEach } from 'vitest';

describe('ClienteListComponent', () => {
  it('should render client list correctly', () => {
    // Test assertion with Vitest
    expect(true).toBe(true);
  });
});
```
