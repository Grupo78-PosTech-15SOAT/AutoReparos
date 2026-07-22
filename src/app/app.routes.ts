import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';
import { roleGuard } from './core/auth/role.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'ordens-servico/fila', pathMatch: 'full' },
  {
    path: 'login',
    loadComponent: () => import('./features/auth/pages/login-page/login-page.component').then(m => m.LoginPageComponent)
  },
  {
    path: 'ordens-servico/fila',
    loadComponent: () => import('./features/ordens-servico/pages/os-kanban-page/os-kanban-page.component').then(m => m.OsKanbanPageComponent),
    canActivate: [authGuard]
  },
  {
    path: 'ordens-servico/nova',
    loadComponent: () => import('./features/ordens-servico/pages/os-nova-page/os-nova-page.component').then(m => m.OsNovaPageComponent),
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Atendente', 'Administrador'] }
  },
  {
    path: 'ordens-servico/:id',
    loadComponent: () => import('./features/ordens-servico/pages/os-detalhe-page/os-detalhe-page.component').then(m => m.OsDetalhePageComponent),
    canActivate: [authGuard]
  },
  {
    path: 'ordens-servico',
    loadComponent: () => import('./features/ordens-servico/pages/os-lista-page/os-lista-page.component').then(m => m.OsListaPageComponent),
    canActivate: [authGuard]
  },
  {
    path: 'clientes',
    loadComponent: () => import('./features/clientes/pages/clientes-page/clientes-page.component').then(m => m.ClientesPageComponent),
    canActivate: [authGuard]
  },
  {
    path: 'veiculos',
    loadComponent: () => import('./features/veiculos/pages/veiculos-page/veiculos-page.component').then(m => m.VeiculosPageComponent),
    canActivate: [authGuard]
  },
  {
    path: 'insumos',
    loadComponent: () => import('./features/insumos/pages/insumos-page/insumos-page.component').then(m => m.InsumosPageComponent),
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Administrador'] }
  },
  {
    path: 'servicos',
    loadComponent: () => import('./features/servicos/pages/servicos-page/servicos-page.component').then(m => m.ServicosPageComponent),
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Administrador'] }
  },
  {
    path: 'usuarios',
    loadComponent: () => import('./features/usuarios/pages/usuarios-page/usuarios-page.component').then(m => m.UsuariosPageComponent),
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Administrador'] }
  },
  {
    path: 'dashboard',
    loadComponent: () => import('./features/dashboard/pages/dashboard-page/dashboard-page.component').then(m => m.DashboardPageComponent),
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Administrador'] }
  },
  {
    path: 'consulta-publica',
    loadComponent: () => import('./features/portal-publico/pages/consulta-publica-page/consulta-publica-page.component').then(m => m.ConsultaPublicaPageComponent)
  },
  {
    path: 'aprovar-orcamento',
    loadComponent: () => import('./features/portal-publico/pages/aprovacao-orcamento-page/aprovacao-orcamento-page.component').then(m => m.AprovacaoOrcamentoPageComponent)
  },
  { path: '**', redirectTo: 'ordens-servico/fila' }
];
