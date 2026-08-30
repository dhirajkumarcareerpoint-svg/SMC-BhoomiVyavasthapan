import { Routes, Route, Navigate } from 'react-router-dom'
import { AuthProvider } from './context/AuthContext'
import ProtectedRoute from './components/ProtectedRoute'
import Layout from './components/Layout'
import Login from './pages/Login'
import Dashboard from './pages/Dashboard'
import MalmattaPage from './pages/MalmattaPage'
import HastantaranPage from './pages/HastantaranPage'
import VasuliPage from './pages/VasuliPage'
import UpkramPage from './pages/UpkramPage'
import KaryapaddhatiPage from './pages/KaryapaddhatiPage'
import CalculationPage from './pages/CalculationPage'
import AhwalPage from './pages/AhwalPage'
import AuditPage from './pages/AuditPage'
import MasterDataPage from './pages/MasterDataPage'
import DemandApplicationPage from './pages/DemandApplicationPage'

export default function App() {
  return (
    <AuthProvider>
      <Routes>
        <Route path="/login" element={<Login />} />
        <Route path="/" element={<ProtectedRoute><Layout /></ProtectedRoute>}>
          <Route index element={<Navigate to="/dashboard" replace />} />
          <Route path="dashboard" element={<Dashboard />} />
          <Route path="malmatta" element={<MalmattaPage />} />
          <Route path="karyapaddhati" element={<KaryapaddhatiPage />} />
          <Route path="hastantaran" element={<HastantaranPage />} />
          <Route path="calculation" element={<CalculationPage />} />
          <Route path="vasuli" element={<VasuliPage />} />
          <Route path="audit" element={<AuditPage />} />
          <Route path="ahwal" element={<AhwalPage />} />
          <Route path="upkram" element={<UpkramPage />} />
          <Route path="master-data" element={<MasterDataPage />} />
          <Route path="demand-application" element={<DemandApplicationPage />} />
        </Route>
        <Route path="*" element={<Navigate to="/dashboard" replace />} />
      </Routes>
    </AuthProvider>
  )
}
