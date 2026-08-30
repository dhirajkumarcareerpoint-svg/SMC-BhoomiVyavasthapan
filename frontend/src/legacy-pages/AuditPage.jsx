import { useEffect, useState } from 'react'
import client from '../api/client'
import Pagination from '../components/Pagination'

export default function AuditPage() {
  const [items, setItems] = useState([])
  const [page, setPage] = useState(1)
  const [totalPages, setTotalPages] = useState(1)
  const [search, setSearch] = useState('')

  useEffect(() => {
    client.get('/auditlogs', { params: { pageNumber: page, pageSize: 15, searchTerm: search || undefined } })
      .then((r) => { setItems(r.data.data.items); setTotalPages(r.data.data.totalPages || 1) })
  }, [page, search])

  return (
    <div>
      <div className="page-header">
        <div>
          <div className="page-title">Audit इतिहास</div>
          <div className="page-subtitle">कोणत्या वापरकर्त्याने काय, केव्हा बदलले — संपूर्ण activity history</div>
        </div>
      </div>
      <div className="card" style={{ padding: 16 }}>
        <div className="toolbar">
          <input className="input" placeholder="🔍 User / Entity शोधा" style={{ minWidth: 260 }}
            value={search} onChange={(e) => { setPage(1); setSearch(e.target.value) }} />
        </div>
        <div className="table-wrap">
          <table>
            <thead>
              <tr><th>User</th><th>कृती</th><th>Entity</th><th>Id</th><th>Field</th><th>जुनी value</th><th>नवीन value</th><th>तारीख-वेळ</th></tr>
            </thead>
            <tbody>
              {items.map((l) => (
                <tr key={l.id}>
                  <td>{l.userName}</td><td>{l.action}</td><td>{l.entityName}</td><td>{l.entityId}</td>
                  <td>{l.fieldName || '-'}</td><td>{l.oldValue || '-'}</td><td>{l.newValue || '-'}</td>
                  <td>{new Date(l.timestamp).toLocaleString('mr-IN')}</td>
                </tr>
              ))}
              {items.length === 0 && <tr><td colSpan={8}><div className="empty-state">कोणतीही नोंद सापडली नाही.</div></td></tr>}
            </tbody>
          </table>
        </div>
        <Pagination page={page} totalPages={totalPages} onChange={setPage} />
      </div>
    </div>
  )
}
