import { Component, inject, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { catchError, debounceTime, distinctUntilChanged, map, of, switchMap } from 'rxjs';
import { Product } from '../../models/product';
import { ProductsApi } from '../../services/products-api';

@Component({
  selector: 'app-cashier',
  imports: [DecimalPipe],
  templateUrl: './cashier.html',
  styleUrl: './cashier.scss',
})
export class Cashier {
  private readonly productsApi = inject(ProductsApi);

  protected readonly query = signal('');

  protected readonly results = toSignal(
    toObservable(this.query).pipe(
      map((query) => query.trim()),
      debounceTime(250),
      distinctUntilChanged(),
      switchMap((query) =>
        query ? this.productsApi.getProducts(query).pipe(catchError(() => of([]))) : of([]),
      ),
    ),
    { initialValue: [] as Product[] },
  );

  protected onSearchInput(event: Event): void {
    this.query.set((event.target as HTMLInputElement).value);
  }
}