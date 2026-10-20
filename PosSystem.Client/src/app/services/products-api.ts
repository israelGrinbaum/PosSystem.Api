import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Service } from '@angular/core';
import { Product } from '../models/product';
import { Observable } from 'rxjs';

@Service()
export class ProductsApi {
  private readonly http = inject(HttpClient);

  getProducts(search?: string): Observable<Product[]> {
    let params = new HttpParams();

    if (search) {
      params = params.set('search', search);
    }
    return this.http.get<Product[]>('/api/products', { params });
  }
}
