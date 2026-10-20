export type PaymentMethod = 'Card' | 'Cash';

export interface CreateSaleLineRequest {
  productId: number;
  quantity: number;
}

export interface CreateSaleRequest {
  lines: CreateSaleLineRequest[];
  paymentMethod: PaymentMethod;
  amountPaid: number | null;
  idempotencyKey: string;
}

export interface SaleLine {
  productId: number;
  productName: string;
  unitPrice: number;
  quantity: number;
  lineTotal: number;
}

export interface Sale {
  id: number;
  receiptNumber: number;
  createdAt: string;
  lines: SaleLine[];
  total: number;
  totalBeforeVat: number;
  vatRate: number;
  vatAmount: number;
  paymentMethod: PaymentMethod;
  amountPaid: number;
  change: number;
}
