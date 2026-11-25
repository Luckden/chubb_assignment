import { aClaim, CAROL, DAVE } from '../../testing/claims';
import { button, nextTick, setUpPage } from '../../testing/setup';
import { StaffClaimPage } from './staff-claim-page';

describe('StaffClaimPage', () => {
  const claimUrl = `/api/claims/${aClaim().id}`;
  const assignedToCarol = aClaim({ status: 'UnderReview', assigneeId: CAROL.id });

  async function render(claim = aClaim(), user = CAROL) {
    const page = await setUpPage(StaffClaimPage, user);
    page.fixture.componentRef.setInput('id', claim.id);
    page.fixture.detectChanges();
    page.http.expectOne(claimUrl).flush(claim);
    await page.fixture.whenStable();
    return page;
  }

  it('lets an officer pick up an incoming claim (R5)', async () => {
    const { fixture, http, element } = await render();

    button(element, 'Pick up').click();
    await nextTick();
    const request = http.expectOne(`${claimUrl}/pick-up`);
    expect(request.request.body).toEqual({ officerId: CAROL.id });
    request.flush(assignedToCarol);
    await fixture.whenStable();

    expect(element.textContent).toContain('Ask the claimant');
    expect(element.textContent).toContain('Settle claim');
  });

  it('asks the claimant a question (R6)', async () => {
    const { fixture, http, element } = await render(assignedToCarol);

    const question = element.querySelector('textarea') as HTMLTextAreaElement;
    question.value = 'Do you have photos?';
    question.dispatchEvent(new Event('input'));
    button(element, 'Send question').click();
    await nextTick();

    const request = http.expectOne(`${claimUrl}/information-requests`);
    expect(request.request.body).toEqual({ officerId: CAROL.id, question: 'Do you have photos?' });
    request.flush({ ...assignedToCarol, status: 'InfoRequested' });
    await fixture.whenStable();

    expect(element.textContent).toContain('Waiting for the claimant to answer');
  });

  it('requires a reason before rejecting (R7)', async () => {
    const { fixture, http, element } = await render(assignedToCarol);

    element.querySelector<HTMLInputElement>('mat-radio-button[value="Rejected"] input')?.click();
    fixture.detectChanges();
    button(element, 'Reject claim').click();
    fixture.detectChanges();

    http.expectNone(`${claimUrl}/reject`);
    expect(element.textContent).toContain('A rejection needs a reason.');
  });

  it('settles with an optional reason (R7)', async () => {
    const { fixture, http, element } = await render(assignedToCarol);

    button(element, 'Settle claim').click();
    await nextTick();
    const request = http.expectOne(`${claimUrl}/settle`);
    expect(request.request.body).toEqual({ officerId: CAROL.id, reason: null });
    request.flush({
      ...assignedToCarol,
      status: 'Settled',
      decisionOutcome: 'Settled',
      decidedAt: '2026-11-22T09:00:00Z',
    });
    await fixture.whenStable();

    expect(element.textContent).toContain('This claim is closed.');
  });

  it("shows who is handling a claim that isn't yours", async () => {
    const { element } = await render(assignedToCarol, DAVE);

    expect(element.textContent).toContain('Carol Wong is handling this claim.');
    expect(element.textContent).not.toContain('Settle claim');
  });

  it('shows the API error when an action fails', async () => {
    const { fixture, http, element } = await render();

    button(element, 'Pick up').click();
    await nextTick();
    http
      .expectOne(`${claimUrl}/pick-up`)
      .flush(
        { title: 'Conflict', detail: 'Claim is already picked up.' },
        { status: 409, statusText: 'Conflict' },
      );
    await fixture.whenStable();

    expect(element.querySelector('[role="alert"]')?.textContent).toContain(
      'Claim is already picked up.',
    );
  });
});
