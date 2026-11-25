const MINUTE = 60_000;
const HOUR = 60 * MINUTE;
const DAY = 24 * HOUR;

/**
 * How long ago `since` was, for "in status for" and "oldest" columns: minutes, then hours, then calendar days.
 * Ages, not deadlines: working-day rules are out of scope (ADR-0012).
 */
export function formatAge(since: string, now: number): string {
  const elapsed = Math.max(0, now - Date.parse(since));
  if (elapsed < HOUR) {
    const minutes = Math.floor(elapsed / MINUTE);
    return minutes <= 1 ? '1 minute' : `${minutes} minutes`;
  }
  if (elapsed < 2 * DAY) {
    const hours = Math.floor(elapsed / HOUR);
    return hours === 1 ? '1 hour' : `${hours} hours`;
  }
  return `${Math.floor(elapsed / DAY)} days`;
}
