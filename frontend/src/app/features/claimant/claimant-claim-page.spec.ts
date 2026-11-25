import { aClaim, ALICE, BOB, CAROL } from '../../testing/claims';
import { button, nextTick, setUpPage } from '../../testing/setup';
import { ClaimantClaimPage } from './claimant-claim-page';

describe('ClaimantClaimPage', () => {
  const asked = aClaim({
    status: 'InfoRequested',
    assigneeId: CAROL.id,
    informationRequests: [
      {
        id: '22222222-2222-7222-8222-222222222222',
        requestedBy: CAROL.id,
        question: 'Do you have photos of the damage?',
        response: null,
        requestedAt: '2026-11-22T09:00:00Z',
        respondedAt: null,
      },
    ],
  });

  async function render(claim = aClaim(), user = ALICE) {
    const page = await setUpPage(ClaimantClaimPage, user);
    page.fixture.componentRef.setInput('id', claim.id);
    page.fixture.detectChanges();
    page.http.expectOne(`/api/claims/${claim.id}`).flush(claim);
    await page.fixture.whenStable();
    return page;
  }

  it('shows the claim and its history (R2)', async () => {
    const { element } = await render();

    expect(element.textContent).toContain('Claim CLM-0001');
    expect(element.textContent).toContain('Submitted');
    expect(element.textContent).toContain('Not yet assigned');
  });

  it('lets the claimant answer an open question (R3)', async () => {
    const { fixture, http, element } = await render(asked);
    expect(element.textContent).toContain('Do you have photos of the damage?');

    const answer = element.querySelector('textarea') as HTMLTextAreaElement;
    answer.value = 'Yes, sent by email.';
    answer.dispatchEvent(new Event('input'));
    button(element, 'Send answer').click();
    await nextTick();

    const request = http.expectOne(`/api/claims/${asked.id}/information-response`);
    expect(request.request.body).toEqual({ claimantId: ALICE.id, response: 'Yes, sent by email.' });
    request.flush({ ...asked, status: 'UnderReview' });
    await fixture.whenStable();

    expect(element.textContent).not.toContain('We need more information');
  });

  it('shows the decision and reason (R4)', async () => {
    const { element } = await render(
      aClaim({
        status: 'Rejected',
        decisionOutcome: 'Rejected',
        decisionReason: 'Not covered by the policy.',
        decidedAt: '2026-11-23T09:00:00Z',
      }),
    );

    expect(element.textContent).toContain('Decision: rejected');
    expect(element.textContent).toContain('Not covered by the policy.');
  });

  it("doesn't show another claimant's claim", async () => {
    const { element } = await render(aClaim(), BOB);

    expect(element.textContent).toContain('This claim belongs to another claimant.');
    expect(element.textContent).not.toContain('Rear-ended');
  });
});
