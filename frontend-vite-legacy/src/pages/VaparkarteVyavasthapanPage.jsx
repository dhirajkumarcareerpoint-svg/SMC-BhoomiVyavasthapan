import { useEffect, useState } from 'react'
import client from '../api/client'
import Modal from '../components/Modal'
import { UserRole } from '../config/labels'

export default function VaparkarteVyavasthapanPage() {
  const [users, setUsers] = useState([])
  const [modalUser, setModalUser] = useState(null)
  const [form, setForm] = useState(null)
  const [error, setError] = useState('')
  const [fieldErrors, setFieldErrors] = useState({})
  const [saving, setSaving] = useState(false)

  const load = () => client.get('/users').then((r) => setUsers(r.data.data))
  useEffect(() => { load() }, [])

  const openCreate = () => {
    setForm({ username: '', password: '', fullName: '', designation: '', mobile: '', email: '', role: 'Staff' })
    setModalUser('create')
    setError('')
    setFieldErrors({})
  }
  const openEdit = (u) => {
    setForm({ ...u })
    setModalUser(u)
    setError('')
    setFieldErrors({})
  }
  const close = () => { setModalUser(null); setForm(null) }

  const save = async () => {
    const errors = {}
    if (modalUser === 'create' && !form.username.trim()) errors.username = 'वापरकर्तानाव आवश्यक आहे.'
    if (modalUser === 'create' && (!form.password || form.password.length < 8)) errors.password = 'पासवर्ड किमान 8 अक्षरांचा असावा.'
    if (!form.fullName.trim()) errors.fullName = 'पूर्ण नाव आवश्यक आहे.'
    if (form.mobile && !/^\d{10}$/.test(form.mobile.trim())) errors.mobile = 'भ्रमणध्वनी क्रमांक 10 अंकी असावा.'
    if (form.email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email.trim())) errors.email = 'कृपया वैध ई-मेल भरा.'
    if (!Object.prototype.hasOwnProperty.call(UserRole, form.role)) errors.role = 'कृपया वैध भूमिका निवडा.'
    setFieldErrors(errors)
    if (Object.keys(errors).length > 0) {
      setError('कृपया दाखवलेल्या त्रुटी दुरुस्त करा.')
      return
    }
    setSaving(true)
    setError('')
    try {
      if (modalUser === 'create') await client.post('/users', form)
      else await client.put(`/users/${modalUser.id}`, form)
      await load()
      close()
    } catch (err) {
      setError(err.response?.data?.messageMr || 'जतन करताना त्रुटी आली.')
    } finally { setSaving(false) }
  }

  const resetPassword = async (u) => {
    const pw = prompt(`${u.fullName} साठी नवीन पासवर्ड टाका:`)
    if (!pw) return
    await client.post(`/users/${u.id}/reset-password`, JSON.stringify(pw), { headers: { 'Content-Type': 'application/json' } })
    alert('पासवर्ड यशस्वीरित्या रीसेट झाला.')
  }

  const deactivate = async (u) => {
    if (!confirm(`${u.fullName} यांना निष्क्रिय करायचे आहे का?`)) return
    await client.delete(`/users/${u.id}`)
    await load()
  }

  return (
    <div>
      <div className="page-header">
        <div>
          <div className="page-title">वापरकर्ता व्यवस्थापन</div>
          <div className="page-subtitle">10 कर्मचाऱ्यांसाठी स्वतंत्र Login — Admin / अधिकारी / कर्मचारी</div>
        </div>
        <button className="btn btn-primary" onClick={openCreate}>+ नवीन वापरकर्ता</button>
      </div>

      <div className="card" style={{ padding: 16 }}>
        <div className="table-wrap">
          <table>
            <thead>
              <tr><th>वापरकर्तानाव</th><th>पूर्ण नाव</th><th>पदनाम</th><th>Role</th><th>स्थिती</th><th>क्रिया</th></tr>
            </thead>
            <tbody>
              {users.map((u) => (
                <tr key={u.id}>
                  <td>{u.username}</td>
                  <td>{u.fullName}</td>
                  <td>{u.designation || '-'}</td>
                  <td>{UserRole[u.role] || u.role}</td>
                  <td>{u.isActive ? '✅ सक्रिय' : '❌ निष्क्रिय'}</td>
                  <td style={{ display: 'flex', gap: 6, justifyContent: 'flex-end' }}>
                    <button className="btn btn-outline btn-sm" onClick={() => openEdit(u)}>संपादन</button>
                    <button className="btn btn-outline btn-sm" onClick={() => resetPassword(u)}>पासवर्ड रीसेट</button>
                    <button className="btn btn-danger btn-sm" onClick={() => deactivate(u)}>निष्क्रिय करा</button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {modalUser && form && (
        <Modal title={modalUser === 'create' ? 'नवीन वापरकर्ता' : 'वापरकर्ता संपादन'} onClose={close}
          footer={<>
            <button className="btn btn-outline" onClick={close}>बंद करा</button>
            <button className="btn btn-primary" onClick={save} disabled={saving}>{saving && <span className="spinner" />} जतन करा</button>
          </>}>
          {error && <div className="error-msg">{error}</div>}
          <div className="form-grid">
            <div className="form-field"><label>वापरकर्तानाव *</label>
              <input className="input" disabled={modalUser !== 'create'} value={form.username} onChange={(e) => setForm({ ...form, username: e.target.value })} />
              {fieldErrors.username && <div className="error-msg">{fieldErrors.username}</div>}
            </div>
            {modalUser === 'create' && (
              <div className="form-field"><label>पासवर्ड *</label>
                <input className="input" type="password" value={form.password} onChange={(e) => setForm({ ...form, password: e.target.value })} />
                {fieldErrors.password && <div className="error-msg">{fieldErrors.password}</div>}
              </div>
            )}
            <div className="form-field"><label>पूर्ण नाव *</label>
              <input className="input" value={form.fullName} onChange={(e) => setForm({ ...form, fullName: e.target.value })} />
              {fieldErrors.fullName && <div className="error-msg">{fieldErrors.fullName}</div>}
            </div>
            <div className="form-field"><label>पदनाम</label>
              <input className="input" value={form.designation || ''} onChange={(e) => setForm({ ...form, designation: e.target.value })} />
            </div>
            <div className="form-field"><label>भ्रमणध्वनी</label>
              <input className="input" value={form.mobile || ''} onChange={(e) => setForm({ ...form, mobile: e.target.value })} />
              {fieldErrors.mobile && <div className="error-msg">{fieldErrors.mobile}</div>}
            </div>
            <div className="form-field"><label>ईमेल</label>
              <input className="input" value={form.email || ''} onChange={(e) => setForm({ ...form, email: e.target.value })} />
              {fieldErrors.email && <div className="error-msg">{fieldErrors.email}</div>}
            </div>
            <div className="form-field"><label>Role *</label>
              <select value={form.role} onChange={(e) => setForm({ ...form, role: e.target.value })}>
                {Object.entries(UserRole).map(([k, v]) => <option key={k} value={k}>{v}</option>)}
              </select>
              {fieldErrors.role && <div className="error-msg">{fieldErrors.role}</div>}
            </div>
            {modalUser !== 'create' && (
              <div className="form-field"><label>सक्रिय आहे का?</label>
                <select value={form.isActive ? 'true' : 'false'} onChange={(e) => setForm({ ...form, isActive: e.target.value === 'true' })}>
                  <option value="true">होय</option>
                  <option value="false">नाही</option>
                </select>
              </div>
            )}
          </div>
        </Modal>
      )}
    </div>
  )
}
