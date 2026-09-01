"use client"

import { useEffect, useState } from "react"
import { useParams } from "next/navigation"
import client from "../api/client"

const paymentStatusText = (status) => ({
  PaymentRequired: "पेमेंट प्रलंबित",
  PaymentPending: "पेमेंट प्रलंबित",
  PaymentDone: "पेमेंट पूर्ण झाले आहे",
  PaymentVerificationPending: "पेमेंट पूर्ण झाले आहे",
  PaymentVerified: "पेमेंट पूर्ण झाले आहे",
}[status] || status || "पेमेंट प्रलंबित")

const isCompleted = (status) => ["PaymentDone", "PaymentVerificationPending", "PaymentVerified"].includes(status)

export function ApplicantPaymentPanel({ applicationNumber, token, onSubmitted, embedded = false }) {
  const [payment, setPayment] = useState(null)
  const [utr, setUtr] = useState("")
  const [paymentDate, setPaymentDate] = useState(new Date().toISOString().slice(0, 10))
  const [screenshot, setScreenshot] = useState(null)
  const [message, setMessage] = useState("")
  const [saving, setSaving] = useState(false)

  useEffect(() => {
    if (!token) {
      setMessage("अवैध पेमेंट लिंक.")
      return
    }

    client
      .get(`/demand-workflow/payment/${encodeURIComponent(applicationNumber)}`, { params: { token } })
      .then((response) => setPayment(response.data.data))
      .catch((error) => setMessage(error.response?.data?.messageMr || "पेमेंट माहिती लोड करता आली नाही."))
  }, [applicationNumber, token])

  const submit = async (event) => {
    event.preventDefault()
    if (!screenshot || !utr.trim()) {
      setMessage("पेमेंट पूर्ण झाल्यानंतर UTR आणि पेमेंट पावती निवडा.")
      return
    }

    const data = new FormData()
    data.append("utr", utr.trim())
    data.append("paymentDate", paymentDate)
    data.append("screenshot", screenshot)
    data.append("token", token)
    setSaving(true)
    setMessage("")

    try {
      const response = await client.post(`/demand-workflow/${payment.demandApplicationId}/payment`, data)
      setPayment((current) => ({ ...current, ...response.data.data }))
      onSubmitted?.(response.data.data)
      setMessage("पेमेंट पूर्ण झाले आहे. OS स्क्रीनवर स्थिती आपोआप अद्ययावत होईल.")
    } catch (error) {
      setMessage(error.response?.data?.messageMr || error.response?.data?.message || "पेमेंट पुष्टीकरण सादर करता आले नाही.")
    } finally {
      setSaving(false)
    }
  }

  if (!payment) return <div className="empty-state">{message || "पेमेंट माहिती लोड होत आहे..."}</div>

  const completed = isCompleted(payment.paymentStatus)
  const downloadUrl = (kind) => `/api/demand-workflow/payment/${encodeURIComponent(payment.applicationNumber)}/${kind}?token=${encodeURIComponent(token)}`

  return (
    <section style={{ maxWidth: 820, margin: embedded ? "16px 0 0" : "0 auto" }} aria-labelledby="payment-title">
      {!embedded && <div className="page-header">
        <div>
          <div className="page-title" id="payment-title">सोलापूर महानगरपालिका</div>
          <div className="page-subtitle">भूमी व मालमत्ता व्यवस्थापन — ऑनलाइन पेमेंट</div>
        </div>
      </div>}

      <div className="card" style={{ overflow: "hidden" }}>
        <div style={{ padding: "20px 24px", background: "linear-gradient(110deg, #062b50, #0f5d78)", color: "#fff" }}>
          <div style={{ fontSize: 13, opacity: 0.9 }}>अर्ज क्रमांक</div>
          <div style={{ marginTop: 3, fontSize: 22, fontWeight: 800 }}>{payment.applicationNumber}</div>
        </div>

        <div style={{ display: "grid", gridTemplateColumns: "minmax(0, 1fr) minmax(240px, 320px)", gap: 28, padding: 24 }}>
          <div>
            <div className="property-info-grid" style={{ gridTemplateColumns: "1fr", gap: 12 }}>
              <div>अर्जदाराचे नाव: <b>{payment.applicantName}</b></div>
              <label className="form-field">
                <span>देय रक्कम</span>
                <input className="input" value={`₹${Number(payment.payableAmount || 0).toLocaleString("en-IN", { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`} readOnly aria-readonly="true" />
              </label>
              <div>पेमेंट स्थिती: <b style={{ color: completed ? "#157347" : "#a15c00" }}>{paymentStatusText(payment.paymentStatus)}</b></div>
            </div>

            {payment.stage === "Approved" ? (
              <div className="success-msg" style={{ marginTop: 20 }}>
                अर्ज मंजूर झाला आहे.
                <div style={{ display: "flex", gap: 8, flexWrap: "wrap", marginTop: 12 }}>
                  <a className="btn btn-outline" href={downloadUrl("application-pdf")}>Application PDF</a>
                  <a className="btn btn-primary" href={downloadUrl("certificate-pdf")}>Certificate PDF</a>
                </div>
              </div>
            ) : completed ? (
              <div className="success-msg" style={{ marginTop: 20 }}>
                पेमेंट पूर्ण झाले आहे. पुढील कार्यवाही OS कडून केली जाईल.
              </div>
            ) : (
              <form onSubmit={submit} style={{ display: "grid", gap: 12, marginTop: 20 }}>
                <div style={{ fontSize: 13, color: "#557084" }}>पेमेंट केल्यानंतर खालील तपशील सादर करा. रक्कम OS ने निश्चित केलेली असून ती बदलता येत नाही.</div>
                <label className="form-field">
                  <span>UTR / Transaction ID *</span>
                  <input className="input" value={utr} onChange={(event) => setUtr(event.target.value)} required />
                </label>
                <label className="form-field">
                  <span>पेमेंट दिनांक *</span>
                  <input className="input" type="date" value={paymentDate} onChange={(event) => setPaymentDate(event.target.value)} required />
                </label>
                <label className="form-field">
                  <span>पेमेंट पावती / स्क्रीनशॉट *</span>
                  <input className="input" type="file" accept=".pdf,.jpg,.jpeg,.png,.doc,.docx,.xlsx" onChange={(event) => setScreenshot(event.target.files?.[0] || null)} required />
                </label>
                {message && <div className="error-msg" role="status">{message}</div>}
                <button className="btn btn-primary" disabled={saving}>{saving ? "सादर करत आहे..." : "पेमेंट पूर्ण झाल्याची पुष्टी करा"}</button>
              </form>
            )}
          </div>

          <aside style={{ alignSelf: "start", padding: 18, border: "1px solid #cfe4ec", borderRadius: 12, background: "#f4f8fb", textAlign: "center" }}>
            <img src="/Payment/Payment-qr.png" alt="SMC अधिकृत पेमेंट QR कोड" style={{ display: "block", width: "min(100%, 250px)", height: "auto", margin: "0 auto" }} />
            <p style={{ margin: "15px 0 0", color: "#102b42", fontWeight: 650 }}>खालील QR कोड स्कॅन करून शुल्क भरा.</p>
            <p style={{ margin: "7px 0 0", color: "#557084", fontSize: 12.5 }}>पेमेंट रक्कम: ₹{Number(payment.payableAmount || 0).toLocaleString("en-IN", { minimumFractionDigits: 2, maximumFractionDigits: 2 })}</p>
          </aside>
        </div>
      </div>
    </section>
  )
}

export default function DemandPaymentPage() {
  const { applicationNumber } = useParams()
  const token = new URLSearchParams(typeof window === "undefined" ? "" : window.location.search).get("token") || ""
  return <ApplicantPaymentPanel applicationNumber={applicationNumber} token={token} />
}
