"use client"

import { useEffect } from "react"
import { usePathname, useRouter } from "next/navigation"
import { useAuth } from "../context/AuthContext"
import NextLayout from "./NextLayout"

export default function NextAuthGate({ children }) {
  const { user, hydrated } = useAuth()
  const router = useRouter()
  const pathname = usePathname()
  const isOfficerRoute = pathname === "/demand-application/officer"
  // Applicant entry and its payment return route remain public.  All other
  // management routes use the existing Admin / legacy Officer session.
  const isDemandPublicRoute = pathname === "/demand-application"
    || pathname === "/application-status"
    || pathname.startsWith("/demand-application/payment/")
  // `/` immediately redirects to the public Demand Application landing page.
  // It must remain public during hydration so that redirect is not raced by
  // the management-route login guard.
  const isRootRedirectRoute = pathname === "/"
  const isPublicRoute = isDemandPublicRoute || isRootRedirectRoute
  const isLoginRoute = pathname === "/login" || pathname === "/officer-login"
  const hasManagementAccess = ["Admin", "Officer"].includes(user?.role)
  const isManagementRoute = !isLoginRoute && !isPublicRoute && !isOfficerRoute

  useEffect(() => {
    if (hydrated && !user && isOfficerRoute) router.replace("/officer-login")
    if (hydrated && isManagementRoute && !hasManagementAccess) {
      router.replace(`/login?next=${encodeURIComponent(pathname)}`)
    }
  }, [hydrated, user, isOfficerRoute, isManagementRoute, hasManagementAccess, router])

  if (isLoginRoute) return children
  if (isPublicRoute) return <NextLayout>{children}</NextLayout>
  if (!hydrated) return <RouteLoading message="Loading..." />
  if (isManagementRoute && !hasManagementAccess) return <RouteLoading message="Redirecting to login..." />
  if (!isOfficerRoute) return <NextLayout>{children}</NextLayout>
  if (!hydrated) return <RouteLoading message="सत्र तपासत आहे..." />
  if (!user) return <RouteLoading message="अधिकारी लॉगिन पृष्ठाकडे जात आहे..." />
  return <NextLayout>{children}</NextLayout>
}

function RouteLoading({ message }) {
  return (
    <main className="route-loading" role="status" aria-live="polite">
      <div className="route-loading-card">
        <span className="route-loading-spinner" aria-hidden="true" />
        <span>{message}</span>
      </div>
    </main>
  )
}
