export const SCOPE_BOUNDARIES = {
  ALLOWED_PANEL_COUNT: 2,
  ALLOWED_ENDPOINTS: [
    '/api/dashboard/tokens/summary',
    '/api/dashboard/tokens/trend',
  ] as readonly string[],
  IS_READ_ONLY: true,
  SINGLE_STUDENT_ONLY: true,
  NO_DIRECT_DATASTORE_ACCESS: true,
  NO_MULTI_STUDENT_COMPARISON: true,
  ALLOWED_AZURE_SERVICES: ['Static Web Apps'] as readonly string[],
} as const
