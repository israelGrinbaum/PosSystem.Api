import { computed, Service, signal } from '@angular/core';
import { CartLine } from '../models/cart-line';
import { Product } from '../models/product';

@Service()
export class CartStore {
  private readonly state = signal<CartLine[]>([]);
  readonly lines = this.state.asReadonly();

  readonly totalAgorot = computed(() => {
    return this.state().reduce(
      (total, line) => total + line.productPriceAsAgorot * line.quantity,
      0,
    );
  });

  readonly isEmpty = computed(() => {
    return this.state().length === 0;
  });

  add(product: Product): void {
    this.state.update((lines) => {
      const exists = lines.some((line) => line.productId === product.id);

      if (exists) {
        return lines.map((line) =>
          line.productId === product.id ? { ...line, quantity: line.quantity + 1 } : line,
        );
      }

      return [
        ...lines,
        {
          productId: product.id,
          productName: product.name,
          productPriceAsAgorot: toAgorot(product.price),
          quantity: 1,
        },
      ];
    });
  }
  increment(productId: number): void {
    this.state.update((lines) =>
      lines.map((line) => {
        if (line.productId === productId) {
          return { ...line, quantity: line.quantity + 1 };
        }
        return line;
      }),
    );
  }

  decrement(productId: number): void {
    this.state.update((lines) =>
      lines.map((line) => {
        if (line.productId === productId && line.quantity > 1) {
          return { ...line, quantity: line.quantity - 1 };
        }
        return line;
      }),
    );
  }

  remove(productId: number): void {
    this.state.update((lines) => {
      return lines.filter((line) => line.productId !== productId);
    });
  }

  clear(): void {
    this.state.set([]);
  }
}

function toAgorot(price: number): number {
  return Math.round(price * 100);
}
