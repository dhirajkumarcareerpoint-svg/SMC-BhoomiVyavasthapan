"use client"

import { createContext, useContext, useState, useCallback, useEffect } from 'react'
import client from '../api/client'

const AuthContext = createContext(null)

function getStoredUser() {
  if (typeof window === 'undefined') return null
  const raw = localStorage.getItem('smc_user')
  if (!raw) return null
  try {
    return JSON.parse(raw)
  } catch {
    // A stale/corrupted browser value must not crash the complete client tree.
    localStorage.removeItem('smc_user')
    localStorage.removeItem('smc_token')
    return null
  }
}

export function AuthProvider({ children }) {
  const [hydrated, setHydrated] = useState(false)
  // Keep the SSR markup and the browser's first render identical. Reading
  // localStorage here would make an existing officer session render a different
  // header before React has hydrated the server HTML.
  const [user, setUser] = useState(null)

  useEffect(() => {
    setUser(getStoredUser())
    setHydrated(true)
    const syncStoredSession = (event) => {
      if (event.key === 'smc_user' || event.key === 'smc_token') setUser(getStoredUser())
    }
    window.addEventListener('storage', syncStoredSession)
    return () => window.removeEventListener('storage', syncStoredSession)
  }, [])

  const login = useCallback(async (username, password) => {
    const res = await client.post('/auth/login', { username, password })
    const data = res.data.data
    localStorage.setItem('smc_token', data.token)
    localStorage.setItem('smc_user', JSON.stringify(data))
    setUser(data)
    return data
  }, [])

  const logout = useCallback(() => {
    localStorage.removeItem('smc_token')
    localStorage.removeItem('smc_user')
    setUser(null)
  }, [])

  const hasRole = useCallback((...roles) => {
    if (!user) return false
    return roles.includes(user.role)
  }, [user])

  return (
    <AuthContext.Provider value={{ user, login, logout, hasRole, hydrated }}>
      {children}
    </AuthContext.Provider>
  )
}

export const useAuth = () => useContext(AuthContext)
