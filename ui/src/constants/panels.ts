export const PANELS = {
  TOTAL_TOKENS: 'Total Tokens',
  TOKEN_USAGE_OVER_TIME: 'Token Usage Over Time',
} as const

export const SUPPORTED_TOKEN_TYPES = [
  'input',
  'output',
  'reasoning',
  'cacheRead',
  'cacheCreation',
] as const

export type SupportedTokenType = typeof SUPPORTED_TOKEN_TYPES[number]

export const SI_01_ENDPOINTS = {
  SUMMARY: '/api/dashboard/tokens/summary',
  TREND: '/api/dashboard/tokens/trend',
} as const

export const TIME_RANGES = {
  LAST_10_DAYS: {
    label: 'Last 10 days',
    value: 'last_10_days',
  },
} as const

export type TimeRangeValue = typeof TIME_RANGES[keyof typeof TIME_RANGES]['value']
