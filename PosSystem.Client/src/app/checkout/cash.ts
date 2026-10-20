const BILLS_AGOROT = [2000, 5000, 10000, 20000];
const ROUNDING_STEPS_AGOROT = [1000, 5000, 10000];
const QUICK_AMOUNTS_COUNT = 4;
const AMOUNT_PATTERN = /^\d{1,6}(\.\d{1,2})?$/;

export function quickCashAmounts(totalAgorot: number): number[] {
  const candidates = [
    totalAgorot,
    ...ROUNDING_STEPS_AGOROT.map((step) => roundUp(totalAgorot, step)),
    ...BILLS_AGOROT.filter((bill) => bill >= totalAgorot),
  ];
  return [...new Set(candidates.sort((a, b) => a - b))].slice(0, QUICK_AMOUNTS_COUNT);
}

export function changeAgorot(totalAgorot: number, paidAgorot: number): number | null {
  return paidAgorot >= totalAgorot ? paidAgorot - totalAgorot : null;
}

export function parseShekels(text: string): number | null {
  const trimmed = text.trim();
  return AMOUNT_PATTERN.test(trimmed) ? Math.round(Number(trimmed) * 100) : null;
}

function roundUp(amount: number, step: number): number {
  return Math.ceil(amount / step) * step;
}
