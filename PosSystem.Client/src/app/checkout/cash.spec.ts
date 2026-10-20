import { changeAgorot, parseShekels, quickCashAmounts } from './cash';

describe('quickCashAmounts', () => {
  it('offers exact, round and bills for a typical total', () => {
    expect(quickCashAmounts(5800)).toEqual([5800, 6000, 10000, 20000]);
  });
  it('includes small bills for a small total', () => {
    expect(quickCashAmounts(690)).toEqual([690, 1000, 2000, 5000]);
  });
  it('removes duplicates when the total is already round', () => {
    expect(quickCashAmounts(6000)).toEqual([6000, 10000, 20000]);
  });
  it('uses rounding steps when the total is above every bill', () => {
    expect(quickCashAmounts(43700)).toEqual([43700, 44000, 45000, 50000]);
  });
});

describe('changeAgorot', () => {
  it('returns the change when overpaid', () => {
    expect(changeAgorot(690, 1000)).toBe(310);
  });
  it('returns zero when paid exactly', () => {
    expect(changeAgorot(690, 690)).toBe(0);
  });
  it('returns null when underpaid by one agora', () => {
    expect(changeAgorot(690, 689)).toBeNull();
  });
});

describe('parseShekels', () => {
  it('parses a whole amount', () => {
    expect(parseShekels('100')).toBe(10000);
  });
  it('parses one decimal digit', () => {
    expect(parseShekels('6.9')).toBe(690);
  });
  it('avoids floating point errors', () => {
    expect(parseShekels('1.15')).toBe(115);
  });
  it('ignores surrounding spaces', () => {
    expect(parseShekels('  12.50  ')).toBe(1250);
  });
  it('returns null for empty text', () => {
    expect(parseShekels('')).toBeNull();
  });
  it('returns null for trailing garbage', () => {
    expect(parseShekels('12abc')).toBeNull();
  });
  it('returns null for a negative amount', () => {
    expect(parseShekels('-5')).toBeNull();
  });
  it('returns null for more than two decimal digits', () => {
    expect(parseShekels('1.234')).toBeNull();
  });
});
