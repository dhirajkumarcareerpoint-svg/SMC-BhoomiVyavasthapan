 import "../src/index.css"
import "../src/components/layout.css"
import "../src/legacy-pages/demand-application.css"
import { AuthProvider } from "../src/context/AuthContext"
import NextAuthGate from "../src/next/NextAuthGate"

export const metadata = {
  title: "सोलापूर महानगरपालिका | भूमी व मालमत्ता व्यवस्थापन प्रणाली",
}

export default function RootLayout({ children }) {
  return (
    <html lang="mr">
      <body>
        <AuthProvider>
          <NextAuthGate>{children}</NextAuthGate>
        </AuthProvider>
      </body>
    </html>
  )
}
