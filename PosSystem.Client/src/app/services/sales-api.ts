import { HttpClient } from '@angular/common/http';
import { inject, Service } from '@angular/core';
import { Observable } from 'rxjs';
import { CreateSaleRequest, Sale } from '../models/sale';

@Service()
export class SalesApi {
  private readonly http = inject(HttpClient);

  createSale(request: CreateSaleRequest): Observable<Sale> {
    return this.http.post<Sale>('/api/sales', request);
  }
}
