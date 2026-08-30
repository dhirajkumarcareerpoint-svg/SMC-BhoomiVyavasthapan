export default function StatCard({ label, value, color, icon }) {
  return (
    <div className="stat-card" style={{ background: color }}>
      <div style={{ fontSize: 22 }}>{icon}</div>
      <div className="stat-value">{value}</div>
      <div className="stat-label">{label}</div>
    </div>
  )
}
