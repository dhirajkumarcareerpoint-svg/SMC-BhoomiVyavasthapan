import { statusColor } from '../config/labels'

export default function StatusBadge({ status, label }) {
  return <span className="badge" style={{ background: statusColor(status) }}>{label || status}</span>
}
