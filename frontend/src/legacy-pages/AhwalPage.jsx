import client from '../api/client'

const reports = [
  { key: 'properties-excel', label: 'मालमत्ता अहवाल (Excel)', url: '/reports/properties/excel', icon: '📊' },
  { key: 'properties-pdf', label: 'मालमत्ता अहवाल (PDF)', url: '/reports/properties/pdf', icon: '📄' },
  { key: 'leases-excel', label: 'हस्तांतरण अहवाल (Excel)', url: '/reports/leases/excel', icon: '📊' },
  { key: 'recovery-excel', label: 'वसुली अहवाल (Excel)', url: '/reports/recovery/excel', icon: '📊' },
  { key: 'recovery-pdf', label: 'वसुली अहवाल (PDF)', url: '/reports/recovery/pdf', icon: '📄' },
  { key: 'audit-excel', label: 'Audit अहवाल (Excel)', url: '/reports/audit/excel', icon: '📊' },
]

export default function AhwalPage() {
  const download = async (r) => {
    const res = await client.get(r.url, { responseType: 'blob' })
    const url = window.URL.createObjectURL(new Blob([res.data]))
    const a = document.createElement('a')
    a.href = url
    a.download = r.label + (r.url.includes('pdf') ? '.pdf' : '.xlsx')
    a.click()
    window.URL.revokeObjectURL(url)
  }

  return (
    <div>
      <div className="page-header">
        <div>
          <div className="page-title">अहवाल</div>
          <div className="page-subtitle">मालमत्ता / हस्तांतरण / वसुली / Audit अहवाल Excel किंवा PDF स्वरूपात डाउनलोड करा</div>
        </div>
      </div>
      <div className="stat-grid">
        {reports.map((r) => (
          <div key={r.key} className="card" style={{ padding: 20, display: 'flex', flexDirection: 'column', gap: 12 }}>
            <div style={{ fontSize: 26 }}>{r.icon}</div>
            <div style={{ fontWeight: 700, color: '#0b3d91', fontSize: 14 }}>{r.label}</div>
            <button className="btn btn-primary" onClick={() => download(r)}>डाउनलोड करा</button>
          </div>
        ))}
      </div>
    </div>
  )
}
