"use client";

import { Suspense, useEffect, useRef, useState } from "react";
import { useSearchParams } from "next/navigation";
import client from "../../src/api/client";
import { ApplicantPaymentPanel } from "../../src/legacy-pages/DemandPaymentPage";

const currentLabels = {
  JEPending: "JE तपासणीसाठी प्रलंबित",
  OSPending: "OS तपासणीसाठी प्रलंबित",
  PaymentRequired: "पेमेंट लिंक पाठवली",
  PaymentVerificationPending: "पेमेंट पडताळणी प्रलंबित",
  AssistantCommissionerApprovalPending: "सहाय्यक आयुक्त मंजुरीसाठी प्रलंबित",
  Approved: "मंजूर",
  Rejected: "नाकारले",
};
const levelLabels = {
  Pending: "प्रलंबित",
  Accepted: "स्वीकारले",
  Rejected: "नाकारले",
  Forwarded: "पुढे पाठवले",
  Approved: "मंजूर",
  "Payment Required": "पेमेंट आवश्यक",
};
const paymentStatusLabel = (paymentStatus) =>
  ({
    PaymentRequired: "प्रलंबित",
    PaymentPending: "प्रलंबित",
    PaymentDone: "Payment Done",
    PaymentVerificationPending: "Payment Submitted",
    PaymentVerified: "Payment Done",
  })[paymentStatus] || paymentStatus;

function ApplicationStatusContent() {
  const searchParams = useSearchParams();
  const paymentToken = searchParams.get("token") || "";
  const requestToken = searchParams.get("requestToken") || "";
  const applicationNumberRef = useRef(null);
  const resubmitInputRef = useRef(null);
  const resubmitInFlight = useRef(false);
  const [number, setNumber] = useState("");
  const [result, setResult] = useState(null);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);
  const [paymentOpen, setPaymentOpen] = useState(false);
  const lookup = async (applicationNumber) => {
    setError("");
    setResult(null);
    setLoading(true);
    try {
      const response = await client.get(
        `/demand-workflow/public-status/${encodeURIComponent(applicationNumber)}`,
        {
          params: {
            ...(paymentToken ? { token: paymentToken } : {}),
            ...(requestToken ? { requestToken } : {}),
          },
        },
      );
      setResult(response.data.data);
      setPaymentOpen(Boolean(response.data.data.paymentAccessGranted));
    } catch {
      setError(
        "दिलेल्या अर्ज क्रमांकाची माहिती उपलब्ध नाही.",
      );
    } finally {
      setLoading(false);
    }
  };
  const search = async (event) => {
    event.preventDefault();
    await lookup(number.trim());
  };
  const clear = () => {
    setNumber("");
    setResult(null);
    setError("");
    setLoading(false);
    setPaymentOpen(false);
    setResubmitFile(null);
    setResubmitMessage("");
    applicationNumberRef.current?.focus();
  };
  useEffect(() => {
    const applicationNumber = searchParams.get("applicationNumber")?.trim();
    if (applicationNumber) {
      setNumber(applicationNumber);
      lookup(applicationNumber);
    }
  }, []);
  const Level = ({ title, data }) => (
    <article className="card" style={{ padding: 16, marginTop: 12 }}>
      <h3 style={{ margin: 0 }}>{title}</h3>
      <p>
        <b>स्थिती:</b> {levelLabels[data.status] || data.status}
      </p>
      {data.paymentStatus && (
        <p>
          <b>पेमेंट स्थिती:</b> {data.paymentStatus}
        </p>
      )}
      {data.actionAt && (
        <p>
          <b>कार्यवाही दिनांक:</b>{" "}
          {new Date(data.actionAt).toLocaleString("mr-IN")}
        </p>
      )}
      {data.rejectionReason && (
        <p className="error-msg">
          <b>नकाराचे कारण:</b> {data.rejectionReason}
        </p>
      )}
    </article>
  );
  const [resubmitFile, setResubmitFile] = useState(null);
  const [resubmitting, setResubmitting] = useState(false);
  const [resubmitMessage, setResubmitMessage] = useState("");
  const chooseResubmit = (event) => {
    const file = event.target.files?.[0];
    if (!file) return;
    if (
      file.name.split(".").pop()?.toLowerCase() !== "pdf" ||
      (file.type && file.type !== "application/pdf")
    )
      return setResubmitMessage("फक्त PDF स्वरूपातील कागदपत्र अपलोड करा.");
    if (file.size > 20 * 1024 * 1024)
      return setResubmitMessage(
        "कागदपत्राचा आकार 20 MB पेक्षा जास्त असू शकत नाही.",
      );
    setResubmitFile(file);
    setResubmitMessage("");
  };
  const resubmitRequestedDocument = async () => {
    if (
      !result?.canResubmitRequestedDocument ||
      !requestToken ||
      !resubmitFile ||
      resubmitInFlight.current
    )
      return;
    resubmitInFlight.current = true;
    setResubmitting(true);
    setResubmitMessage("");
    try {
      const data = new FormData();
      data.append("file", resubmitFile);
      await client.post(
        `/demand-applications/public/${result.demandApplicationId}/documents/${result.requestedDocumentId}/resubmit`,
        data,
        { headers: { "X-Demand-Document-Request-Token": requestToken } },
      );
      setResubmitFile(null);
      if (resubmitInputRef.current) resubmitInputRef.current.value = "";
      await lookup(result.applicationNumber);
      setResubmitMessage("कागदपत्र यशस्वीरित्या सादर झाले.");
    } catch (requestError) {
      setResubmitMessage(
        requestError.response?.data?.messageMr ||
          requestError.response?.data?.message ||
          "कागदपत्र सादर करता आले नाही.",
      );
    } finally {
      setResubmitting(false);
      resubmitInFlight.current = false;
    }
  };
  const paymentPending =
    result?.currentStatus === "PaymentRequired" &&
    result.paymentStatus === "PaymentRequired";
  const paymentCompleted = [
    "PaymentDone",
    "PaymentVerificationPending",
    "PaymentVerified",
  ].includes(result?.paymentStatus);
  return (
    <div style={{ maxWidth: 760, margin: "0 auto" }}>
      <div className="page-header">
        <div>
          <div className="page-title">
            अर्जाची स्थिती तपासा
          </div>
          <div className="page-subtitle">
            अर्ज क्रमांक टाकून आपल्या अर्जाची सद्यस्थिती तपासा
          </div>
        </div>
      </div>
      <form
        className="card"
        onSubmit={search}
        style={{ padding: 20, display: "flex", gap: 10, alignItems: "end" }}
      >
        <div className="form-field" style={{ flex: 1 }}>
          <label>अर्ज क्रमांक</label>
          <input
            ref={applicationNumberRef}
            className="form-input"
            value={number}
            onChange={(event) => setNumber(event.target.value)}
            required
          />
        </div>
        <button className="btn btn-primary" disabled={loading}>
          {loading
            ? "शोधत आहे..."
            : "स्थिती तपासा"}
        </button>
        <button type="button" className="btn btn-outline" onClick={clear}>
          Clear
        </button>
      </form>
      {error && (
        <div className="error-msg" role="status">
          {error}
        </div>
      )}
      {result && (
        <section style={{ marginTop: 18 }}>
          <div className="card" style={{ padding: 20 }}>
            <h2 style={{ marginTop: 0 }}>
              अर्ज क्रमांक: {result.applicationNumber}
            </h2>
            <p>
              <b>अर्जदाराचे नाव:</b>{" "}
              {result.applicantName}
            </p>
            <p>
              <b>अर्ज सादर दिनांक:</b>{" "}
              {result.submittedAt
                ? new Date(result.submittedAt).toLocaleString("mr-IN")
                : "-"}
            </p>
            <p>
              <b>सद्यस्थिती:</b>{" "}
              <strong>
                {currentLabels[result.currentStatus] || result.currentStatus}
              </strong>
            </p>
            {paymentPending && (
              <div
                className="card"
                style={{
                  marginTop: 16,
                  padding: 16,
                  border: "1px solid #f0b36a",
                  background: "#fffaf2",
                }}
              >
                <p style={{ marginTop: 0 }}>
                  <b>Payment Status:</b> प्रलंबित
                </p>
                <p>
                  <b>Amount:</b> ₹
                  {Number(result.payableAmount || 0).toLocaleString("en-IN", {
                    minimumFractionDigits: 2,
                    maximumFractionDigits: 2,
                  })}
                </p>
                {result.paymentAccessGranted ? (
                  <>
                    <button
                      type="button"
                      className="btn btn-primary"
                      onClick={() => setPaymentOpen(true)}
                    >
                      पेमेंट करा
                    </button>
                    {paymentOpen && (
                      <ApplicantPaymentPanel
                        applicationNumber={result.applicationNumber}
                        token={paymentToken}
                        embedded
                        onSubmitted={() => lookup(result.applicationNumber)}
                      />
                    )}
                  </>
                ) : (
                  <p style={{ marginBottom: 0, color: "#557084" }}>
                    पेमेंट करण्यासाठी मिळालेली सुरक्षित लिंक वापरा.
                  </p>
                )}
              </div>
            )}
            {paymentCompleted && (
              <p className="success-msg" style={{ marginTop: 16 }}>
                <b>Payment Status:</b>{" "}
                {paymentStatusLabel(result.paymentStatus)}
              </p>
            )}
          </div>
          {result.hasDocumentRequest && (
            <article className="card" style={{ padding: 16, marginTop: 12 }}>
              <h3 style={{ marginTop: 0 }}>कागदपत्र पुन्हा सादर करण्याची विनंती</h3>
              <p><b>विनंती केलेले कागदपत्र:</b> {result.requestedDocumentName || result.requestedDocumentType || "-"}</p>
              <p><b>Level 2 शेरा / विनंती:</b> {result.requestRemark || "-"}</p>
              <p><b>विनंती दिनांक:</b> {result.requestDate ? new Date(result.requestDate).toLocaleString("mr-IN") : "-"}</p>
              <p><b>विनंती स्थिती:</b> {result.requestStatus || "-"}</p>
              {result.canResubmitRequestedDocument && requestToken && (
                <div style={{ display: "grid", gap: 10, marginTop: 14 }}>
                  <label className="form-field"><span>PDF कागदपत्र निवडा (कमाल 20 MB)</span><input ref={resubmitInputRef} type="file" accept=".pdf,application/pdf" disabled={resubmitting} onChange={chooseResubmit} /></label>
                  <button type="button" className="btn btn-primary" disabled={resubmitting || !resubmitFile} onClick={resubmitRequestedDocument}>{resubmitting ? "सादर करत आहे..." : "कागदपत्र सादर करा"}</button>
                </div>
              )}
              {resubmitMessage && <div className={result.canResubmitRequestedDocument ? "error-msg" : "success-msg"} role="status" style={{ marginTop: 10 }}>{resubmitMessage}</div>}
            </article>
          )}
          <Level title="Level 1 / JE" data={result.je} />
          <Level title="Level 2 / OS" data={result.os} />
          <Level
            title="Level 3 / Assistant Commissioner"
            data={result.assistantCommissioner}
          />
        </section>
      )}
    </div>
  );
}

export default function ApplicationStatusPage() {
  return (
    <Suspense
      fallback={
        <div className="card" style={{ padding: 20 }}>
          लोड करत आहे...
        </div>
      }
    >
      <ApplicationStatusContent />
    </Suspense>
  );
}
