import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

export default function Login() {
  const { login } = useAuth()
  const navigate = useNavigate()
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)

  const submit = async (e) => {
    e.preventDefault()
    setError('')
    setBusy(true)
    try {
      await login(username, password)
      navigate('/dashboard')
    } catch (err) {
      setError(err.response?.data?.messageMr || 'लॉगिन अयशस्वी झाले.')
    } finally {
      setBusy(false)
    }
  }

  return (
    <div className="login-page">
      <div className="login-card">
        <div className="login-emblem">सो</div>
        <div className="login-title">सोलापूर महानगरपालिका</div>
        <div className="login-subtitle">भूमी व मालमत्ता व्यवस्थापन प्रणाली</div>
        {error && <div className="error-msg">{error}</div>}
        <form onSubmit={submit} style={{ display: 'flex', flexDirection: 'column', gap: 12 }}>
          <div className="form-field">
            <label>वापरकर्तानाव</label>
            <input className="input" value={username} onChange={(e) => setUsername(e.target.value)} required autoFocus />
          </div>
          <div className="form-field">
            <label>पासवर्ड</label>
            <input className="input" type="password" value={password} onChange={(e) => setPassword(e.target.value)} required />
          </div>
          <button className="btn btn-primary" style={{ justifyContent: 'center', marginTop: 6 }} disabled={busy}>
            {busy && <span className="spinner" />} लॉगिन करा
          </button>
        </form>
        <div style={{ marginTop: 18, fontSize: 11.5, color: '#94a3b8', textAlign: 'center' }}>
          डेमो: admin / Admin@123 &nbsp;|&nbsp; officer1 / Officer@123 &nbsp;|&nbsp; staff1 / Staff@123
        </div>
      </div>
    </div>
  )
}
