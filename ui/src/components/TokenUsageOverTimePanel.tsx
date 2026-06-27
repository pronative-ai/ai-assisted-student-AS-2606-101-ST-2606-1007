import { PANELS } from '../constants/panels'

interface TokenUsageOverTimePanelProps {
  timeRange: string
}

export default function TokenUsageOverTimePanel({ timeRange }: TokenUsageOverTimePanelProps) {
  return (
    <div className="panel panel-trend">
      <h2 className="panel-title">{PANELS.TOKEN_USAGE_OVER_TIME}</h2>
      <p className="panel-range">{timeRange}</p>
      <div className="panel-body">
        <p className="panel-placeholder">Token usage trend will render here</p>
      </div>
    </div>
  )
}
