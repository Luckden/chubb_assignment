import { formatAge } from './format-age';

describe('formatAge', () => {
  const now = Date.parse('2026-11-25T12:00:00Z');

  it.each([
    ['2026-11-25T12:00:00Z', '1 minute'],
    ['2026-11-25T11:45:00Z', '15 minutes'],
    ['2026-11-25T11:00:00Z', '1 hour'],
    ['2026-11-24T00:00:00Z', '36 hours'],
    ['2026-11-22T12:00:00Z', '3 days'],
  ])('%s is %s ago', (since, expected) => {
    expect(formatAge(since, now)).toBe(expected);
  });

  it('treats a future time as just now', () => {
    expect(formatAge('2026-11-25T12:05:00Z', now)).toBe('1 minute');
  });
});
