import { useState } from 'react'
import { TIME_RANGES, type TimeRangeValue } from '../constants/panels'
import TimeRangeSelector from '../components/TimeRangeSelector'
import TotalTokensPanel from '../components/TotalTokensPanel'
import TokenUsageOverTimePanel from '../components/TokenUsageOverTimePanel'

export default function DashboardPage() {
  const [selectedRange, setSelectedRange] = useState<TimeRangeValue>(
    TIME_RANGES.LAST_10_DAYS.value,
  )

  return (
    <div className="dashboard-page">
      <header className="dashboard-header">
        <h1>Token Usage Dashboard</h1>
        <TimeRangeSelector
          selectedRange={selectedRange}
          onRangeChange={setSelectedRange}
        />
      </header>

      <div className="dashboard-panels">
        <TotalTokensPanel timeRange={TIME_RANGES.LAST_10_DAYS.label} />
        <TokenUsageOverTimePanel timeRange={TIME_RANGES.LAST_10_DAYS.label} />
      </div>

      <footer className="dashboard-footer">
        <span className="scope-badge">Read-only</span>
        <span className="scope-badge">Single-student</span>
      </footer>
    </div>
  )
}
