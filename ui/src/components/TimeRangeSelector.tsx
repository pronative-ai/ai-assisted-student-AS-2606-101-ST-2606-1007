import { TIME_RANGES, type TimeRangeValue } from '../constants/panels'

interface TimeRangeSelectorProps {
  selectedRange: TimeRangeValue
  onRangeChange: (range: TimeRangeValue) => void
}

export default function TimeRangeSelector({
  selectedRange,
  onRangeChange,
}: TimeRangeSelectorProps) {
  return (
    <div className="time-range-selector">
      <label htmlFor="time-range">Time Range:</label>
      <select
        id="time-range"
        value={selectedRange}
        onChange={(e) => onRangeChange(e.target.value as TimeRangeValue)}
      >
        <option value={TIME_RANGES.LAST_10_DAYS.value}>
          {TIME_RANGES.LAST_10_DAYS.label}
        </option>
      </select>
    </div>
  )
}
