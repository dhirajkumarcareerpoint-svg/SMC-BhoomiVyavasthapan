import { useEffect, useState } from 'react'
import client from '../api/client'

export default function DocumentsPanel({ entityType, entityId, canUpload }) {
  const [docs, setDocs] = useState([])
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState('')

  const load = async () => {
    if (!entityId) return
    const res = await client.get('/documents', { params: { entityType, entityId } })
    setDocs(res.data.data || [])
  }

  useEffect(() => { load() }, [entityId])

  const onUpload = async (e) => {
    const file = e.target.files?.[0]
    if (!file) return
    setError('')
    const form = new FormData()
    form.append('entityType', entityType)
    form.append('entityId', entityId)
    form.append('file', file)
    setBusy(true)
    try {
      await client.post('/documents/upload', form, { headers: { 'Content-Type': 'multipart/form-data' } })
      await load()
    } catch (err) {
      setError(err.response?.data?.messageMr || 'फाईल अपलोड करताना त्रुटी आली.')
    } finally {
      setBusy(false)
      e.target.value = ''
    }
  }

  const onDelete = async (id) => {
    if (!confirm('हा दस्तऐवज हटवायचा आहे का?')) return
    await client.delete(`/documents/${id}`)
    await load()
  }

  const onDownload = async (doc) => {
    const res = await client.get(`/documents/${doc.id}/download`, { responseType: 'blob' })
    const url = window.URL.createObjectURL(new Blob([res.data]))
    const a = document.createElement('a')
    a.href = url
    a.download = doc.fileName
    a.click()
    window.URL.revokeObjectURL(url)
  }

  if (!entityId) return <div style={{ fontSize: 12.5, color: '#94a3b8' }}>प्रथम नोंद Save करा, त्यानंतर दस्तऐवज जोडता येतील.</div>

  return (
    <div>
      {canUpload && (
        <div>
          <input type="file" onChange={onUpload} disabled={busy} accept=".pdf,.jpg,.jpeg,.png,.docx,.doc,.xlsx" />
          {busy && <span className="spinner" style={{ marginRight: 8, borderTopColor: '#0b3d91' }} />}
        </div>
      )}
      {error && <div className="error-msg" style={{ marginTop: 8 }}>{error}</div>}
      <div className="doc-list">
        {docs.length === 0 && <div style={{ fontSize: 12.5, color: '#94a3b8' }}>कोणतेही दस्तऐवज अपलोड केलेले नाहीत.</div>}
        {docs.map((d) => (
          <div key={d.id} className="doc-item">
            <span>📎 {d.fileName} <small style={{ color: '#94a3b8' }}>({(d.fileSizeBytes / 1024).toFixed(0)} KB)</small></span>
            <span style={{ display: 'flex', gap: 8 }}>
              <button className="btn btn-outline btn-sm" onClick={() => onDownload(d)}>डाउनलोड</button>
              {canUpload && <button className="btn btn-danger btn-sm" onClick={() => onDelete(d.id)}>हटवा</button>}
            </span>
          </div>
        ))}
      </div>
    </div>
  )
}
