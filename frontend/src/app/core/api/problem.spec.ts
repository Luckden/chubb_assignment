import { HttpErrorResponse } from '@angular/common/http';

import { problemMessage } from './problem';

describe('problemMessage', () => {
  it('joins validation errors', () => {
    const error = new HttpErrorResponse({
      status: 400,
      error: { title: 'Validation failed', errors: { Reason: ['Reason is required.'] } },
    });
    expect(problemMessage(error)).toBe('Reason is required.');
  });

  it('uses the ProblemDetails detail', () => {
    const error = new HttpErrorResponse({
      status: 409,
      error: { title: 'Conflict', detail: 'Nope.' },
    });
    expect(problemMessage(error)).toBe('Nope.');
  });

  it('reports an unreachable API', () => {
    expect(problemMessage(new HttpErrorResponse({ status: 0 }))).toContain('unreachable');
  });
});
