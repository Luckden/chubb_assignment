import { HttpErrorResponse } from '@angular/common/http';

import type { ValidationProblemDetails } from './api-types';

/** A readable message from a failed API call: ProblemDetails detail/title, plus any validation errors. */
export function problemMessage(error: unknown): string {
  if (!(error instanceof HttpErrorResponse)) return 'Something went wrong. Please try again.';
  if (error.status === 0) return 'The API is unreachable. Please try again.';

  const problem = (error.error ?? {}) as ValidationProblemDetails;
  const validation = Object.values(problem.errors ?? {}).flat();
  if (validation.length > 0) return validation.join(' ');
  return problem.detail ?? problem.title ?? `Request failed (${error.status}).`;
}
