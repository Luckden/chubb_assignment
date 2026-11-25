import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Type } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { QueryClient, provideTanStackQuery } from '@tanstack/angular-query';

import { CurrentUser, type DemoUser } from '../core/session/current-user';

/** TestBed for a page that uses TanStack Query and HttpClient, acting as `user`. */
export async function setUpPage<T>(
  component: Type<T>,
  user: DemoUser,
): Promise<{ fixture: ComponentFixture<T>; http: HttpTestingController; element: HTMLElement }> {
  sessionStorage.clear();
  await TestBed.configureTestingModule({
    imports: [component],
    providers: [
      provideRouter([]),
      provideHttpClient(),
      provideHttpClientTesting(),
      provideTanStackQuery(
        () => new QueryClient({ defaultOptions: { queries: { retry: false } } }),
      ),
    ],
  }).compileComponents();
  TestBed.inject(CurrentUser).select(user.id);

  const fixture = TestBed.createComponent(component);
  return {
    fixture,
    http: TestBed.inject(HttpTestingController),
    element: fixture.nativeElement as HTMLElement,
  };
}

/** Buttons by visible text, like `getByRole('button', { name })` in Playwright. */
export function button(element: HTMLElement, name: string): HTMLButtonElement {
  const match = [...element.querySelectorAll('button')].find((b) => b.textContent?.trim() === name);
  if (!match) throw new Error(`No button "${name}"`);
  return match;
}

/** TanStack mutations send their request after a microtask, so wait a tick before `expectOne`. */
export function nextTick(): Promise<void> {
  return new Promise((resolve) => setTimeout(resolve));
}
