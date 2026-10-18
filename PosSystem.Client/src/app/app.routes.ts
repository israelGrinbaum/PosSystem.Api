import { Routes } from '@angular/router';
import { Cashier } from './pages/cashier/cashier';

export const routes: Routes = [
  { path: '', component: Cashier, title: 'קופה' },
  {
    path: 'products',
    loadComponent: () => import('./pages/products/products').then((m) => m.Products),
    title: 'מוצרים',
  },
  {
    path: 'inventory',
    loadComponent: () => import('./pages/inventory/inventory').then((m) => m.Inventory),
    title: 'מלאי',
  },
  {
    path: 'sales',
    loadComponent: () => import('./pages/sales/sales').then((m) => m.Sales),
    title: 'מכירות',
  },
  { path: '**', redirectTo: '' },
];
