import { NavLink, Outlet, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import './layout.css'

const menu = [
  { to: '/demand-application', label: 'मागणी अर्ज', icon: '📝' },
  { to: '/dashboard', label: 'डॅशबोर्ड', icon: '📊' },
  { to: '/malmatta', label: 'मालमत्ता', icon: '🏢' },
  { to: '/karyapaddhati', label: 'देण्याची कार्यपद्धती', icon: '🏛️' },
  { to: '/hastantaran', label: 'हस्तांतरण', icon: '📜' },
  { to: '/calculation', label: 'Calculation', icon: '🧮' },
  { to: '/vasuli', label: 'वसुली प्रक्रिया', icon: '💰' },
  { to: '/audit', label: 'Audit इतिहास', icon: '🕒' },
  { to: '/ahwal', label: 'अहवाल', icon: '📈' },
  { to: '/upkram', label: 'विविध उपक्रम', icon: '🎯' },
  { to: '/master-data', label: 'Master Data', icon: '📁' },
]

export default function Layout() {
  const { user, logout } = useAuth()
  const navigate = useNavigate()

  const doLogout = () => {
    logout()
    navigate('/login')
  }

  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div className="sidebar-brand">
          <div className="brand-emblem">सो</div>
          <div>
            <div className="brand-title">सोलापूर महानगरपालिका</div>
            <div className="brand-subtitle">भूमी व मालमत्ता व्यवस्थापन</div>
          </div>
        </div>
        <nav className="sidebar-nav">
          {menu.map((m) => (
            <NavLink key={m.to} to={m.to} className={({ isActive }) => 'nav-item' + (isActive ? ' active' : '')}>
              <span className="nav-icon">{m.icon}</span> {m.label}
            </NavLink>
          ))}
        </nav>
      </aside>
      <div className="main-area">
        <header className="topbar">
          <div className="topbar-title">भूमी व मालमत्ता व्यवस्थापन प्रणाली</div>
          <div className="topbar-user">
            <div className="user-badge">
              <div className="user-name">{user?.fullName}</div>
              <div className="user-role">{user?.role === 'Admin' ? 'प्रशासक' : user?.role === 'Officer' ? 'अधिकारी' : 'कर्मचारी'}</div>
            </div>
            <button className="btn btn-outline" onClick={doLogout}>लॉगआऊट</button>
          </div>
        </header>
        <main className="content-area">
          <Outlet />
        </main>
      </div>
    </div>
  )
}
