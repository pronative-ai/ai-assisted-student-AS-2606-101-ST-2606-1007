import { PANELS } from '../constants/panels'

interface TotalTokensPanelProps {
  timeRange: string
}

export default function TotalTokensPanel({ timeRange }: TotalTokensPanelProps) {
  return (
    <div className="panel panel-total-tokens">
      <h2 className="panel-title">{PANELS.TOTAL_TOKENS}</h2>
      <p className="panel-range">{timeRange}</p>
      <div className="panel-body">
        <p className="panel-placeholder">Total tokens will render here</p>
      </div>
    </div>
  )
}
