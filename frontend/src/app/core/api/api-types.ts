/**
 * Friendly aliases over the generated OpenAPI types. The backend owns the contract:
 * backend/openapi/claims-api.json → `npm run api:generate` → ./generated/schema.ts (never edit by hand).
 */
import type { components } from './generated/schema';

type Schemas = components['schemas'];

export type SystemInfo = Schemas['SystemInfoResponse'];
