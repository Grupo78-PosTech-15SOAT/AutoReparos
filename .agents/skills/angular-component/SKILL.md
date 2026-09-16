---
name: angular-component
description: Guide and code generator for Angular (v22+) standalone components, services, state management, forms, pipes/directives, and Vitest unit tests in AutoReparos.Web.
---

# Angular Component & Frontend Skill Guide

This skill provides step-by-step procedures, architectural patterns, and code generators for building Angular 22+ frontend components in `AutoReparos.Web`.

---

## 1. Architecture Overview & Frontend Conventions

The `AutoReparos.Web` application is structured around standalone Angular components, OnPush change detection, explicit memory management, and modern Control Flow constructs:

```
AutoReparos.Web/src/app/
├── core/                           # Guards, Interceptors, Global Services, API Endpoints
│   ├── auth/                       # AuthGuard & RoleGuard
│   ├── config/api-endpoints.ts     # Centralized API URLs
│   ├── http/                       # JwtInterceptor & ErrorInterceptor
│   └── ui/                         # NotificationService, LoadingService
├── features/                       # Feature Modules (Auth, Clientes, Insumos, Servicos, Veiculos, OrdensServicos)
│   └── <feature>/
│       ├── models/                 # Data Models & Interfaces
│       ├── pages/                  # Page Components & Specs
│       └── services/               # Feature HTTP Services
├── shared/                         # Reusable UI Components, Directives, Pipes
│   ├── components/                 # PageContainerComponent, PaginationComponent, CustomSelectComponent
│   ├── directives/                 # MaskDirective
│   └── pipes/                      # CpfCnpjPipe, CurrencyPipe
├── app.config.ts                   # Standalone Application Config
└── app.routes.ts                   # Routing Table
```

### Mandatory Rules
1. **Standalone Components**: Every component must declare `standalone: true`.
2. **OnPush Strategy**: Set `changeDetection: ChangeDetectionStrategy.OnPush` on components. Call `this.cdr.detectChanges()` or `markForCheck()` after async state updates.
3. **RxJS Subscription Safety**: Always inject `DestroyRef` and append `.pipe(takeUntilDestroyed(this.destroyRef))` to subscriptions in components.
4. **Modern Control Flow**: Use `@if`, `@else`, `@for (item of items; track item.id)`, and `@empty` block syntax instead of legacy `*ngIf` / `*ngFor`.
5. **Dependency Injection**: Use `private readonly service = inject(ServiceName)` rather than standard constructor injection.
6. **Testing**: Use **Vitest** (`describe`, `it`, `expect`, `TestBed`) for component unit specs (`*.spec.ts`).

---

## 2. Step-by-Step Procedure: Creating a New Feature Component

Follow these exact steps when introducing a new feature or page component in `AutoReparos.Web`:

### Step 1: Create Data Model (`src/app/features/<feature>/models/<feature>.model.ts`)
Define TypeScript interfaces and request DTO types matching the backend endpoints.

### Step 2: Create HTTP Service (`src/app/features/<feature>/services/<feature>.service.ts`)
- Decorate with `@Injectable({ providedIn: 'root' })`.
- Use `HttpClient` to communicate with endpoints declared in `API_ENDPOINTS`.
- Implement `normalizePagedResult` for standard list endpoints to return `PagedResult<T>`.

### Step 3: Implement Page Component (`src/app/features/<feature>/pages/<feature>-page/<feature>-page.component.ts`)
- Configure `@Component` metadata: `standalone: true`, `changeDetection: ChangeDetectionStrategy.OnPush`, `imports: [...]`.
- Implement inline template or template file using `<app-page-container>` and `@for` / `@if` blocks.
- Inject `DestroyRef`, `ChangeDetectorRef`, `NotificationService`, and the feature service.
- Implement loading overlays and pagination methods (`onPageChange`, `onPageSizeChange`).

### Step 4: Register Route (`src/app/app.routes.ts`)
Add route entry lazy-loading the new page component:
```typescript
{
  path: 'recurso',
  loadComponent: () => import('./features/recurso/pages/recurso-page/recurso-page.component').then(m => m.RecursoPageComponent),
  canActivate: [AuthGuard]
}
```

### Step 5: Write Vitest Unit Test (`<component>.spec.ts`)
Create unit tests verifying component creation, service invocation, modal state toggling, and data display.

---

## 3. Code Templates

### Template A: Feature Model (`<feature>.model.ts`)
```typescript
export interface ItemFeature {
  id?: string;
  nome: string;
  descricao: string;
  ativo: boolean;
  criadoEm?: string;
}

export interface ItemFeatureCreateDto {
  nome: string;
  descricao: string;
}
```

### Template B: Feature Service (`<feature>.service.ts`)
```typescript
import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { ItemFeature, ItemFeatureCreateDto } from '../models/item-feature.model';
import { API_ENDPOINTS } from '../../../core/config/api-endpoints';
import { PagedResult } from '../../../shared/models/pagination.model';

@Injectable({
  providedIn: 'root'
})
export class ItemFeatureService {
  private readonly http = inject(HttpClient);

  getAll(pageNumber = 1, pageSize = 10, search = ''): Observable<PagedResult<ItemFeature>> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    if (search) {
      params = params.set('nome', search);
    }

    return this.http.get<any>(API_ENDPOINTS.CLIENTES.BASE, { params }).pipe(
      map(res => this.normalizePagedResult(res, pageNumber, pageSize))
    );
  }

  getById(id: string): Observable<ItemFeature> {
    return this.http.get<ItemFeature>(`${API_ENDPOINTS.CLIENTES.BASE}/${id}`);
  }

  criar(dto: ItemFeatureCreateDto): Observable<ItemFeature> {
    return this.http.post<ItemFeature>(API_ENDPOINTS.CLIENTES.BASE, dto);
  }

  excluir(id: string): Observable<void> {
    return this.http.delete<void>(`${API_ENDPOINTS.CLIENTES.BASE}/${id}`);
  }

  private normalizePagedResult(res: any, pageNumber: number, pageSize: number): PagedResult<ItemFeature> {
    if (res) {
      const items = res.items ?? res.Items ?? res.data ?? (Array.isArray(res) ? res : []);
      const total = res.totalItems ?? res.TotalItems ?? res.total ?? items.length;
      const totalPages = res.totalPages ?? Math.max(1, Math.ceil(total / pageSize));
      return { items, total, pageNumber, pageSize, totalPages };
    }
    return { items: [], total: 0, pageNumber, pageSize, totalPages: 1 };
  }
}
```

### Template C: Standalone Page Component (`<feature>-page.component.ts`)
```typescript
import { Component, OnInit, inject, ChangeDetectorRef, ChangeDetectionStrategy, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { ItemFeature, ItemFeatureCreateDto } from '../../models/item-feature.model';
import { ItemFeatureService } from '../../services/item-feature.service';
import { NotificationService } from '../../../../core/ui/notification.service';
import { PageContainerComponent } from '../../../../shared/components/page-container/page-container.component';
import { PaginationComponent } from '../../../../shared/components/pagination/pagination.component';

@Component({
  selector: 'app-item-feature-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    FormsModule,
    PageContainerComponent,
    PaginationComponent
  ],
  template: `
    <app-page-container>
      <div class="page-header">
        <h1 class="page-title">Item Feature</h1>
        <button (click)="abrirModalNovo()" class="btn btn-primary">+ Novo Item</button>
      </div>

      <div class="data-table-container table-loading-container">
        @if (loading) {
          <div class="table-loading-overlay">
            <span class="table-loading-text">Carregando...</span>
          </div>
        }
        <table class="data-table">
          <thead>
            <tr>
              <th>Nome</th>
              <th>Descrição</th>
              <th>Ações</th>
            </tr>
          </thead>
          <tbody>
            @for (item of items; track item.id) {
              <tr>
                <td>{{ item.nome }}</td>
                <td>{{ item.descricao }}</td>
                <td>
                  <button (click)="excluir(item.id!)" class="btn btn-danger btn-sm">Excluir</button>
                </td>
              </tr>
            } @empty {
              <tr>
                <td colspan="3" style="text-align: center; padding: 2rem;">Nenhum item cadastrado.</td>
              </tr>
            }
          </tbody>
        </table>
      </div>

      <app-pagination
        [pageNumber]="pageNumber"
        [pageSize]="pageSize"
        [totalItems]="totalItems"
        [totalPages]="totalPages"
        (pageChange)="onPageChange($event)"
        (pageSizeChange)="onPageSizeChange($event)">
      </app-pagination>
    </app-page-container>
  `,
  styles: [`
    .btn-sm { padding: 0.35rem 0.65rem; font-size: 0.8rem; }
  `]
})
export class ItemFeaturePageComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly service = inject(ItemFeatureService);
  private readonly notification = inject(NotificationService);
  private readonly cdr = inject(ChangeDetectorRef);

  items: ItemFeature[] = [];
  loading = false;
  pageNumber = 1;
  pageSize = 10;
  totalItems = 0;
  totalPages = 1;

  ngOnInit() {
    this.carregar();
  }

  carregar() {
    this.loading = true;
    this.service.getAll(this.pageNumber, this.pageSize)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: res => {
          this.items = res.items;
          this.totalItems = res.total;
          this.totalPages = res.totalPages;
          this.loading = false;
          this.cdr.detectChanges();
        },
        error: () => {
          this.loading = false;
          this.notification.error('Erro', 'Não foi possível carregar os itens.');
          this.cdr.detectChanges();
        }
      });
  }

  onPageChange(page: number) {
    this.pageNumber = page;
    this.carregar();
  }

  onPageSizeChange(size: number) {
    this.pageSize = size;
    this.pageNumber = 1;
    this.carregar();
  }

  abrirModalNovo() {
    // Open modal logic
  }

  excluir(id: string) {
    if (confirm('Deseja excluir este item?')) {
      this.service.excluir(id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
        next: () => {
          this.notification.info('Removido', 'Item excluído.');
          this.carregar();
        }
      });
    }
  }
}
```

### Template D: Vitest Unit Test (`<feature>-page.component.spec.ts`)
```typescript
import { describe, it, expect, beforeEach, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { ItemFeaturePageComponent } from './item-feature-page.component';
import { ItemFeatureService } from '../../services/item-feature.service';
import { NotificationService } from '../../../../core/ui/notification.service';

describe('ItemFeaturePageComponent', () => {
  let component: ItemFeaturePageComponent;
  let mockService: any;
  let mockNotification: any;

  beforeEach(async () => {
    mockService = {
      getAll: vi.fn().mockReturnValue(of({ items: [], total: 0, pageNumber: 1, pageSize: 10, totalPages: 1 })),
      excluir: vi.fn().mockReturnValue(of(undefined))
    };

    mockNotification = {
      info: vi.fn(),
      error: vi.fn(),
      success: vi.fn()
    };

    await TestBed.configureTestingModule({
      imports: [ItemFeaturePageComponent],
      providers: [
        { provide: ItemFeatureService, useValue: mockService },
        { provide: NotificationService, useValue: mockNotification }
      ]
    }).compileComponents();

    const fixture = TestBed.createComponent(ItemFeaturePageComponent);
    component = fixture.componentInstance;
  });

  it('should create the component', () => {
    expect(component).toBeTruthy();
  });

  it('should load items on init', () => {
    component.ngOnInit();
    expect(mockService.getAll).toHaveBeenCalledWith(1, 10);
  });
});
```

---

## 4. Verification Commands

Run unit tests and production builds using yarn:

```bash
# Run Vitest test suite
yarn --cwd AutoReparos.Web test

# Build Angular application
yarn --cwd AutoReparos.Web build
```
