import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { QueryClient, provideTanStackQuery } from '@tanstack/angular-query';

import { App } from './app';

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        // Fresh cache per test, no retries so failures surface immediately.
        provideTanStackQuery(
          () => new QueryClient({ defaultOptions: { queries: { retry: false } } }),
        ),
      ],
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should show API status from the backend', async () => {
    const fixture = TestBed.createComponent(App);
    const http = TestBed.inject(HttpTestingController);
    // Start the query, but don't await whenStable() yet: TanStack Query counts the in-flight request as a
    // pending task, so whenStable() would wait for a response we haven't flushed.
    fixture.detectChanges();

    http
      .expectOne('/api/system/info')
      .flush({ service: 'claims-api', environment: 'Test', version: '1.0.0' });
    await fixture.whenStable();

    const status = (fixture.nativeElement as HTMLElement).querySelector('[role="status"]');
    expect(status?.textContent).toContain('connected (Test)');
  });
});
