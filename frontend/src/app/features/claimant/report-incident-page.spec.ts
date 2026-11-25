import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';

import { aClaim, ALICE } from '../../testing/claims';
import { button, nextTick, setUpPage } from '../../testing/setup';
import { ReportIncidentPage } from './report-incident-page';

function fill(element: HTMLElement, selector: string, value: string): void {
  const input = element.querySelector(selector) as HTMLInputElement | HTMLTextAreaElement;
  input.value = value;
  input.dispatchEvent(new Event('input'));
}

describe('ReportIncidentPage', () => {
  it('submits the report for the acting claimant and opens the claim (R1)', async () => {
    const { fixture, http, element } = await setUpPage(ReportIncidentPage, ALICE);
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
    fixture.detectChanges();

    // mat-select renders its options in an overlay outside the component; the first one is Motor.
    (element.querySelector('mat-select') as HTMLElement).click();
    fixture.detectChanges();
    (document.querySelector('mat-option') as HTMLElement).click();
    fill(element, 'input[type="date"]', '2026-11-20');
    fill(element, 'textarea', 'Rear-ended at a junction.');
    fill(element, 'input[type="number"]', '1500');
    button(element, 'Submit claim').click();
    await nextTick();

    const request = http.expectOne('/api/claims');
    expect(request.request.body).toEqual({
      claimantId: ALICE.id,
      type: 'Motor',
      incidentDate: '2026-11-20',
      description: 'Rear-ended at a junction.',
      claimedAmount: 1500,
    });
    const created = aClaim();
    request.flush(created, { status: 201, statusText: 'Created' });
    await fixture.whenStable();

    expect(navigate).toHaveBeenCalledWith(['/my-claims', created.id]);
  });

  it("doesn't submit an incomplete report", async () => {
    const { fixture, http, element } = await setUpPage(ReportIncidentPage, ALICE);
    fixture.detectChanges();

    button(element, 'Submit claim').click();
    await nextTick();
    fixture.detectChanges();

    http.expectNone('/api/claims');
    expect(element.textContent).toContain('Describe what happened.');
  });
});
