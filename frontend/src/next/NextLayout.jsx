"use client"

import Link from "next/link"
import { usePathname, useRouter } from "next/navigation"
import { useAuth } from "../context/AuthContext"

const menu = [
  { to: "/", label: "मुखपृष्ठ", icon: "🏠" },
  { to: "/dashboard", label: "डॅशबोर्ड", icon: "📊" },
  { to: "/malmatta", label: "मालमत्ता", icon: "🏢" },
  { to: "/karyapaddhati", label: "देण्याची कार्यपद्धती", icon: "🏛️" },
  { to: "/hastantaran", label: "हस्तांतरण", icon: "📜" },
  { to: "/calculation", label: "Calculation", icon: "🧮" },
  { to: "/vasuli", label: "वसुली प्रक्रिया", icon: "💰" },
  { to: "/audit", label: "Audit इतिहास", icon: "🕒" },
  { to: "/ahwal", label: "अहवाल", icon: "📈" },
  { to: "/upkram", label: "विविध उपक्रम", icon: "🎯" },
]

export default function NextLayout({ children }) {
  const { user, logout } = useAuth()
  const router = useRouter()
  const pathname = usePathname()
  const isPublicApplicantPage = pathname === "/demand-application" || pathname === "/application-status"
  const visibleMenu = isPublicApplicantPage ? menu.filter((item) => item.to === "/") : menu

  const doLogout = () => {
    logout()
    router.push("/demand-application")
  }

  return (
    <div className="app-shell">
      <aside className="sidebar">
        <Link href="/" className="sidebar-brand" aria-label="मुखपृष्ठावर जा">
          <div className="brand-emblem">सो</div>
          <div>
            <div className="brand-title">सोलापूर महानगरपालिका</div>
            <div className="brand-subtitle">भूमी व मालमत्ता व्यवस्थापन</div>
          </div>
        </Link>
        <nav className="sidebar-nav">
          {visibleMenu.map((item) => (
            <Link key={item.to} href={item.to} className={`nav-item${pathname === item.to ? " active" : ""}`}>
              <span className="nav-icon">{item.icon}</span> {item.label}
            </Link>
          ))}
        </nav>
      </aside>
      <div className="main-area">
        <header className="topbar">
          <div className="topbar-title">भूमी व मालमत्ता व्यवस्थापन प्रणाली</div>
          <div className="topbar-user">
            {user ? <><div className="user-badge">
              <div className="user-name">{user.fullName || user.username}</div>
              <div className="user-role">{user.role}</div>
            </div>
            <button className="btn btn-outline" onClick={doLogout}>लॉगआऊट</button></> : (pathname === "/" || isPublicApplicantPage) ? <Link className="btn btn-outline" href="/officer-login" target="_blank" rel="noopener noreferrer">अधिकारी Login</Link> : null}
          </div>
        </header>
        <main className="content-area">{children}</main>
      </div>
    </div>
  )
}
