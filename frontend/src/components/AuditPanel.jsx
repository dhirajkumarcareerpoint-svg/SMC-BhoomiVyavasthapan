import { useEffect, useState } from 'react'
import client from '../api/client'

export default function AuditPanel({ entityName, entityId }) {
  const [logs, setLogs] = useState([])

  useEffect(() => {
    if (!entityId) return
    client.get('/auditlogs/entity', { params: { entityName, entityId } }).then((r) => setLogs(r.data.data || []))
  }, [entityId])

  if (!entityId) return null
  if (logs.length === 0) return <div style={{ fontSize: 12.5, color: '#94a3b8' }}>या नोंदीसाठी अद्याप कोणताही बदल इतिहास नाही.</div>

  return (
    <div className="table-wrap" style={{ marginTop: 6 }}>
      <table>
        <thead>
          <tr>
            <th>User</th><th>कृती</th><th>Field</th><th>जुनी value</th><th>नवीन value</th><th>तारीख-वेळ</th>
          </tr>
        </thead>
        <tbody>
          {logs.map((l) => (
            <tr key={l.id}>
              <td>{l.userName}</td>
              <td>{l.action}</td>
              <td>{l.fieldName || '-'}</td>
              <td>{l.oldValue || '-'}</td>
              <td>{l.newValue || '-'}</td>
              <td>{new Date(l.timestamp).toLocaleString('mr-IN')}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
