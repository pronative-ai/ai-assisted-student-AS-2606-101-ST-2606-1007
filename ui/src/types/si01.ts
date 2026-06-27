export interface Si01SummaryResponse {
  status: 'complete' | 'partial' | 'error'
  isComplete: boolean
  missingIntervals?: string[]
  message?: string
  data: {
    totalTokens: number
    tokenType: string
  }[]
}

export interface Si01TrendPoint {
  date: string
  input: number
  output: number
  reasoning: number
  cacheRead: number
  cacheCreation: number
}

export interface Si01TrendResponse {
  status: 'complete' | 'partial' | 'error'
  isComplete: boolean
  missingIntervals?: string[]
  message?: string
  data: Si01TrendPoint[]
}
