/**
 * How often open queries ask the API for changes made by the other party (RFC-0002 section 7). Polling pauses in
 * hidden tabs because `refetchIntervalInBackground` is left off. Push (SSE/SignalR) is "with more time".
 */
export const POLL_INTERVAL_MS = 10_000;
