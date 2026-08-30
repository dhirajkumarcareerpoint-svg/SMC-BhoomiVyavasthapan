import { useEffect, useState } from 'react'
import client from '../api/client'
import StatCard from '../components/StatCard'
import { PropertyCategory } from '../config/labels'
import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, PieChart, Pie, Cell, Legend } from 'recharts'

const COLORS = ['#0b3d91', '#f59e0b', '#16a34a', '#dc2626', '#8b5cf6', '#0ea5e9', '#f97316', '#64748b', '#14b8a6']

export default function Dashboard() {
  const [data, setData] = useState(null)
  const [error, setError] = useState('')

  useEffect(() => {
    client.get('/dashboard/summary')
      .then((r) => setData(r.data.data))
      .catch((err) => setError(err.response?.data?.messageMr || 'डॅशबोर्ड माहिती लोड करता आली नाही.'))
  }, [])

  if (error) return <div className="error-msg">{error}</div>
  if (!data) return <div className="empty-state">माहिती लोड होत आहे...</div>

  const fmt = (n) => new Intl.NumberFormat('en-IN').format(Math.round(n || 0))

  return (
    <div>
      <div className="page-header">
        <div>
          <div className="page-title">डॅशबोर्ड</div>
          <div className="page-subtitle">भूमी व मालमत्ता विभाग - एकूण सारांश</div>
        </div>
      </div>

      <div className="stat-grid">
        <StatCard label="एकूण मालमत्ता" value={fmt(data.totalProperties)} color="#0b3d91" icon="🏢" />
        <StatCard label="एकूण गाळे" value={fmt(data.totalShops)} color="#1e5bb8" icon="🏬" />
        <StatCard label="रिक्त मालमत्ता" value={fmt(data.vacantProperties)} color="#0ea5e9" icon="🔓" />
        <StatCard label="भाडेतत्त्वावर दिलेल्या" value={fmt(data.leasedProperties)} color="#16a34a" icon="📜" />
        <StatCard label="सील केलेल्या मालमत्ता" value={fmt(data.sealedProperties)} color="#dc2626" icon="🔒" />
        <StatCard label="वार्षिक मागणी (₹)" value={fmt(data.annualDemand)} color="#f59e0b" icon="🧾" />
        <StatCard label="एकूण वसुली (₹)" value={fmt(data.totalCollection)} color="#15803d" icon="💰" />
        <StatCard label="एकूण थकबाकी (₹)" value={fmt(data.totalOutstanding)} color="#b91c1c" icon="⚠️" />
        <StatCard label="प्रलंबित वसुली प्रकरणे" value={fmt(data.pendingRecoveryCases)} color="#8b5cf6" icon="📁" />
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: '1.3fr 1fr', gap: 18 }}>
        <div className="card" style={{ padding: 18 }}>
          <h3 style={{ color: '#0b3d91', fontSize: 15, marginBottom: 14 }}>विभागनिहाय मालमत्ता संख्या</h3>
          <ResponsiveContainer width="100%" height={300}>
            <BarChart data={data.categoryBreakdown.map((c) => ({ name: PropertyCategory[c.category] || c.category, संख्या: c.count }))}>
              <XAxis dataKey="name" tick={{ fontSize: 10 }} interval={0} angle={-20} textAnchor="end" height={80} />
              <YAxis />
              <Tooltip />
              <Bar dataKey="संख्या" fill="#0b3d91" radius={[6, 6, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </div>

        <div className="card" style={{ padding: 18 }}>
          <h3 style={{ color: '#0b3d91', fontSize: 15, marginBottom: 14 }}>विभागनिहाय वाटप</h3>
          <ResponsiveContainer width="100%" height={300}>
            <PieChart>
              <Pie data={data.categoryBreakdown.map((c) => ({ name: PropertyCategory[c.category] || c.category, value: c.count }))}
                dataKey="value" nameKey="name" cx="50%" cy="50%" outerRadius={95} label={{ fontSize: 10 }}>
                {data.categoryBreakdown.map((_, i) => <Cell key={i} fill={COLORS[i % COLORS.length]} />)}
              </Pie>
              <Legend wrapperStyle={{ fontSize: 11 }} />
              <Tooltip />
            </PieChart>
          </ResponsiveContainer>
        </div>
      </div>

      {data.monthlyCollection?.length > 0 && (
        <div className="card" style={{ padding: 18, marginTop: 18 }}>
          <h3 style={{ color: '#0b3d91', fontSize: 15, marginBottom: 14 }}>मासिक वसुली प्रवृत्ती (गेले 12 महिने)</h3>
          <ResponsiveContainer width="100%" height={260}>
            <BarChart data={data.monthlyCollection}>
              <XAxis dataKey="month" tick={{ fontSize: 11 }} />
              <YAxis />
              <Tooltip />
              <Bar dataKey="amount" name="वसुली रक्कम" fill="#16a34a" radius={[6, 6, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </div>
      )}
    </div>
  )
}
