/**
 * Friendly aliases over the generated OpenAPI types. The backend owns the contract:
 * backend/openapi/claims-api.json → `npm run api:generate` → ./generated/schema.ts (never edit by hand).
 */
import type { components, paths } from './generated/schema';

type Schemas = components['schemas'];

export type SystemInfo = Schemas['SystemInfoResponse'];

export type ClaimStatus = Schemas['ClaimStatus'];
export type ClaimType = Schemas['ClaimType'];
export type DecisionOutcome = NonNullable<Schemas['DecisionOutcome']>;

export type Claim = Schemas['ClaimResponse'];
export type ClaimSummary = Schemas['ClaimSummaryResponse'];
export type ClaimStatusChange = Schemas['ClaimStatusChangeResponse'];
export type InformationRequest = Schemas['InformationRequestResponse'];
export type ClaimListFilter = NonNullable<paths['/api/claims']['get']['parameters']['query']>;
export type ClaimListSort = NonNullable<Schemas['ClaimListSort']>;

export type SubmitClaimRequest = Schemas['SubmitClaimRequest'];
export type PickUpClaimRequest = Schemas['PickUpClaimRequest'];
export type RequestInformationRequest = Schemas['RequestInformationRequest'];
export type ProvideInformationRequest = Schemas['ProvideInformationRequest'];
export type SettleClaimRequest = Schemas['SettleClaimRequest'];
export type RejectClaimRequest = Schemas['RejectClaimRequest'];

export type ProblemDetails = Schemas['ProblemDetails'];
export type ValidationProblemDetails = Schemas['ValidationProblemDetails'];

export type Notification = Schemas['NotificationResponse'];
export type MarkNotificationReadRequest = Schemas['MarkNotificationReadRequest'];

export type Workload = Schemas['WorkloadResponse'];
export type OfficerWorkload = Schemas['OfficerWorkloadResponse'];
export type Exposure = Schemas['ExposureResponse'];
