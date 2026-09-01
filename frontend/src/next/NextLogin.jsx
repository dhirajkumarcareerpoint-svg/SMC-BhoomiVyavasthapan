"use client"

import { useEffect, useState } from "react"
import Link from "next/link"
import { useRouter } from "next/navigation"
import { useAuth } from "../context/AuthContext"

const requestedModuleLabels = {
  "/dashboard": "डॅशबोर्ड",
  "/malmatta": "मालमत्ता",
  "/properties": "मालमत्ता",
  "/karyapaddhati": "देण्याची कार्यपद्धती",
  "/allocation": "देण्याची कार्यपद्धती",
  "/hastantaran": "हस्तांतरण",
  "/leases": "हस्तांतरण",
  "/transfer": "हस्तांतरण",
  "/calculation": "Calculation",
  "/vasuli": "वसुली प्रक्रिया",
  "/recovery": "वसुली प्रक्रिया",
  "/audit": "Audit इतिहास",
  "/ahwal": "अहवाल",
  "/reports": "अहवाल",
  "/upkram": "विविध उपक्रम",
  "/schemes": "विविध उपक्रम",
}

export default function NextLogin({ managementOnly = false, heading = "", officerLogin = false }) {
  const { login, logout } = useAuth()
  const router = useRouter()
  const [requestedPath, setRequestedPath] = useState("")
  const requestedModuleLabel = requestedModuleLabels[requestedPath]
  const [username, setUsername] = useState("")
  const [password, setPassword] = useState("")
  const [error, setError] = useState("")
  const [busy, setBusy] = useState(false)

  useEffect(() => {
    setRequestedPath(new URLSearchParams(window.location.search).get("next") || "")
  }, [])

  const submit = async (event) => {
    event.preventDefault()
    setError("")
    setBusy(true)
    try {
      const user = await login(username, password)
      if (managementOnly && !["Admin", "Officer"].includes(user.role)) {
        logout()
        setError("या प्रवेशासाठी प्रशासक किंवा अधिकारी खाते आवश्यक आहे.")
        return
      }
      if (!["Admin", "Officer", "JE", "OS", "AssistantCommissioner"].includes(user.role)) {
        logout()
        setError("हे लॉगिन फक्त अधिकाऱ्यांसाठी आहे.")
        return
      }
      const safeRequestedPath = requestedPath?.startsWith("/")
        && !requestedPath.startsWith("//")
        && requestedPath !== "/login"
        && requestedPath !== "/officer-login"
        ? requestedPath
        : null
      router.replace(managementOnly
        ? (safeRequestedPath || "/dashboard")
        : ["JE", "OS", "AssistantCommissioner"].includes(user.role)
          ? "/demand-application/officer"
          : "/dashboard")
    } catch (err) {
      setError(err.response?.data?.messageMr || "लॉगिन अयशस्वी झाले.")
    } finally {
      setBusy(false)
    }
  }

  return (
    <div className={`login-page${officerLogin ? " officer-login-page" : ""}`}>
      {officerLogin && (
        <aside className="sidebar officer-login-sidebar">
          <Link href="/" className="sidebar-brand" aria-label="मुखपृष्ठावर जा">
            <div className="brand-emblem">सो</div>
            <div>
              <div className="brand-title">सोलापूर महानगरपालिका</div>
              <div className="brand-subtitle">भूमी व मालमत्ता व्यवस्थापन</div>
            </div>
          </Link>
          <nav className="sidebar-nav">
            <Link className="nav-item active" href="/">
              <span className="nav-icon">🏠</span> मुखपृष्ठ
            </Link>
          </nav>
        </aside>
      )}
      <div className="login-stack">
      {(heading || requestedModuleLabel) && <div className="login-requested-module">{heading || requestedModuleLabel}</div>}
      <div className="login-card">
        <div className="login-emblem">सो</div>
        <div className="login-title">सोलापूर महानगरपालिका</div>
        <div className="login-subtitle">भूमी व मालमत्ता व्यवस्थापन प्रणाली</div>
        {error && <div className="error-msg">{error}</div>}
        <form onSubmit={submit} style={{ display: "flex", flexDirection: "column", gap: 12 }}>
          <div className="form-field">
            <label>वापरकर्तानाव</label>
            <input className="input" value={username} onChange={(event) => setUsername(event.target.value)} required autoFocus />
          </div>
          <div className="form-field">
            <label>पासवर्ड</label>
            <input className="input" type="password" value={password} onChange={(event) => setPassword(event.target.value)} required />
          </div>
          <button className="btn btn-primary" style={{ justifyContent: "center", marginTop: 6 }} disabled={busy}>
            {busy && <span className="spinner" />} लॉगिन करा
          </button>
        </form>
        <div style={{ marginTop: 18, fontSize: 11.5, color: "#94a3b8", textAlign: "center" }}>
          अधिकृत अधिकारी प्रवेश
        </div>
      </div>
      </div>
    </div>
  )
}
