"use client";

import { useEffect, useRef, useState } from "react";
import client from "../api/client";
import Modal from "../components/Modal";
import AuditPanel from "../components/AuditPanel";
import { useAuth } from "../context/AuthContext";

const stageLabels = {
  JEPending: "JE तपासणीसाठी प्रलंबित",
  OSPending: "OS तपासणीसाठी प्रलंबित",
  PaymentRequired: "पेमेंट लिंक पाठवली",
  PaymentVerificationPending:
    "पेमेंट पडताळणी प्रलंबित",
  AssistantCommissionerApprovalPending:
    "सहाय्यक आयुक्त मंजुरीसाठी प्रलंबित",
  Approved: "मंजूर",
  Rejected: "नाकारले",
};
const stageLabel = (stage) => stageLabels[stage] || stage || "-";
const paymentStatusLabel = (status) =>
  ({
    PaymentRequired: "Payment Pending",
    PaymentPending: "Payment Pending",
    PaymentVerificationPending: "Payment Done",
    PaymentVerified: "Payment Done",
    PaymentDone: "Payment Done",
  })[status] ||
  status ||
  "-";
const actionLabel = (action) =>
  ({
    "JE Verified": "Accepted",
    "JE Rejected": "Rejected",
    "Payment Request Sent": "Accepted",
    "OS Rejected": "Rejected",
    "Payment Verified": "Accepted",
    "Payment Rejected": "Rejected",
    "Payment status set to PaymentPending": "Payment Pending",
    "Payment status set to PaymentDone": "Payment Done",
    "Forwarded to Assistant Commissioner": "Forwarded",
    "Final Approved": "Approved",
    "Assistant Commissioner Rejected": "Rejected",
  })[action] ||
  action ||
  "-";
const documentTypeLabel = (type) =>
  ({
    IdentityProof: "ओळखपत्र",
    AddressProof: "पत्त्याचा पुरावा",
    PanGst: "PAN / GST",
    BusinessDocument: "व्यवसाय कागदपत्र",
  })[type] ||
  type ||
  "कागदपत्र";

function EyeIcon() {
  return (
    <svg
      aria-hidden="true"
      viewBox="0 0 24 24"
      width="15"
      height="15"
      fill="none"
      stroke="currentColor"
      strokeWidth="2"
    >
      <path d="M2 12s3.5-6 10-6 10 6 10 6-3.5 6-10 6S2 12 2 12Z" />
      <circle cx="12" cy="12" r="2.5" />
    </svg>
  );
}

export default function DemandOfficerPage() {
  const { user } = useAuth();
  const [items, setItems] = useState([]);
  const [history, setHistory] = useState([]);
  const [documentActions, setDocumentActions] = useState({});
  const [message, setMessage] = useState("");
  const [processingId, setProcessingId] = useState(null);
  const [selected, setSelected] = useState(null);
  const [decisions, setDecisions] = useState({});
  const actionInFlight = useRef(false);
  const documentActionInFlight = useRef(false);
  const load = async (quiet = false) => {
    try {
      const [queueResponse, historyResponse] = await Promise.all([
        client.get("/demand-workflow/queue"),
        client.get("/demand-workflow/processed-history"),
      ]);
      setItems(queueResponse.data.data || []);
      setHistory(historyResponse.data.data || []);
      if (!quiet) setMessage("");
    } catch (error) {
      if (!quiet)
        setMessage(
          error.response?.data?.messageMr ||
            "अधिकारी अर्ज लोड करता आले नाहीत.",
        );
    }
  };
  useEffect(() => {
    load();
    const timer = window.setInterval(() => load(true), 20000);
    return () => window.clearInterval(timer);
  }, []);
  const setDecision = (id, values) =>
    setDecisions((current) => ({
      ...current,
      [id]: { ...current[id], ...values },
    }));
  const runAction = async (item, endpoint, body, successMessage) => {
    if (actionInFlight.current) return;
    actionInFlight.current = true;
    setProcessingId(item.id);
    setMessage("");
    try {
      await client.post(
        `/demand-workflow/${item.demandApplicationId}/${endpoint}`,
        body,
      );
      setDecisions((current) => {
        const next = { ...current };
        delete next[item.id];
        return next;
      });
      await load(true);
      setMessage(successMessage);
    } catch (error) {
      setMessage(
        error.response?.data?.messageMr ||
          error.response?.data?.message ||
          "कार्यवाही करता आली नाही. कृपया पुन्हा प्रयत्न करा.",
      );
    } finally {
      setProcessingId(null);
      actionInFlight.current = false;
    }
  };
  const submitDecision = (item) => {
    const decision = decisions[item.id] || {};
    const finalApproval = item.stage === "AssistantCommissionerApprovalPending";
    const approve = finalApproval
      ? decision.choice === "approve"
      : decision.choice === "accept";
    if (!decision.choice)
      return setMessage(
        finalApproval
          ? "कृपया मंजुरी निवडा."
          : "कृपया स्वीकार किंवा नकार निवडा.",
      );
    if (!approve && !decision.reason?.trim())
      return setMessage(
        "नकारासाठी कारण / शेरा आवश्यक आहे.",
      );
    const paymentRequest = item.stage === "OSPending" && approve;
    const amount = Number(String(decision.amount ?? "").trim());
    if (paymentRequest && (!Number.isFinite(amount) || amount <= 0))
      return setMessage(
        "कृपया वैध शुल्क रक्कम प्रविष्ट करा.",
      );
    const endpoint = paymentRequest
      ? "payment-request"
      : item.stage === "JEPending"
        ? "je"
        : item.stage === "OSPending"
          ? "os"
          : finalApproval
            ? "approve"
            : "payment/verify";
    return runAction(
      item,
      endpoint,
      paymentRequest
        ? { payableAmount: amount }
        : finalApproval
          ? {}
          : { approve, reason: decision.reason?.trim() || undefined },
      paymentRequest
        ? "पेमेंट लिंक अर्जदाराला पाठवली आहे."
        : approve
          ? "अर्जावरील कार्यवाही यशस्वी झाली आहे."
          : "अर्ज नाकारला आहे.",
    );
  };
  const openDetails = async (item) => {
    try {
      const response = await client.get(
        `/demand-applications/${item.demandApplicationId}`,
      );
      setSelected({ workflow: item, application: response.data.data });
    } catch {
      setMessage(
        "अर्जाची माहिती लोड करता आली नाही.",
      );
    }
  };
  const openOfficerDocument = async (fileDocument, download = false) => {
    const previewTab = download ? null : window.open("", "_blank");
    try {
      const response = await client.get(
        `/demand-applications/documents/${fileDocument.id}/download`,
        { responseType: "blob" },
      );
      const url = window.URL.createObjectURL(response.data);
      if (download) {
        const anchor = document.createElement("a");
        anchor.href = url;
        anchor.download = fileDocument.fileName || "document.pdf";
        document.body.appendChild(anchor);
        anchor.click();
        anchor.remove();
      } else if (previewTab) {
        previewTab.opener = null;
        previewTab.location.href = url;
      } else {
        window.open(url, "_blank", "noopener,noreferrer");
      }
      window.setTimeout(() => window.URL.revokeObjectURL(url), 60_000);
    } catch (error) {
      if (previewTab) previewTab.close();
      setMessage(
        error.response?.data?.messageMr ||
          "कागदपत्र उघडता आले नाही.",
      );
    }
  };
  const uploadSitePhoto = async (event) => {
    const file = event.target.files?.[0];
    if (!file || !selected) return;
    const extension = file.name.split(".").pop()?.toLowerCase();
    const header = new Uint8Array(await file.slice(0, 8).arrayBuffer());
    const jpeg =
      header.length >= 3 &&
      header[0] === 0xff &&
      header[1] === 0xd8 &&
      header[2] === 0xff;
    const png =
      header.length === 8 &&
      [0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a].every(
        (value, index) => header[index] === value,
      );
    if (
      !(
        ["jpg", "jpeg", "png"].includes(extension) &&
        ["image/jpeg", "image/png"].includes(file.type) &&
        (jpeg || png)
      )
    ) {
      setMessage(
        "फक्त वैध JPG किंवा PNG प्रतिमा अपलोड करा.",
      );
      event.target.value = "";
      return;
    }
    if (file.size > 10 * 1024 * 1024) {
      setMessage(
        "प्रतिमेचा आकार 10 MB पेक्षा जास्त असू शकत नाही.",
      );
      event.target.value = "";
      return;
    }
    setProcessingId(selected.workflow.id);
    setMessage("");
    try {
      const data = new FormData();
      data.append("file", file);
      const response = await client.post(
        `/demand-applications/${selected.application.id}/site-photo`,
        data,
      );
      const photo = response.data.data;
      setSelected((current) =>
        current
          ? {
              ...current,
              application: {
                ...current.application,
                documents: [
                  ...(current.application.documents || []).filter(
                    (document) =>
                      document.documentType !== "SiteInspectionPhoto",
                  ),
                  photo,
                ],
              },
            }
          : current,
      );
      setMessage(
        "प्रत्यक्ष जागेचा फोटो जतन केला आहे.",
      );
    } catch (error) {
      setMessage(
        error.response?.data?.messageMr ||
          "प्रत्यक्ष जागेचा फोटो अपलोड करता आला नाही.",
      );
    } finally {
      setProcessingId(null);
      event.target.value = "";
    }
  };
  const selectDocumentAction = (documentId, status) =>
    setDocumentActions((current) => ({
      ...current,
      [documentId]: {
        status,
        remark: current[documentId]?.remark || "",
        submitting: false,
      },
    }));
  const setDocumentRemark = (documentId, remark) =>
    setDocumentActions((current) => ({
      ...current,
      [documentId]: { ...current[documentId], status: "Requested", remark },
    }));
  const setDocumentStatus = async (fileDocument, status, explicitRemark) => {
    if (!selected || documentActionInFlight.current) return;
    const remark =
      status === "Requested"
        ? (
            explicitRemark ??
            documentActions[fileDocument.id]?.remark ??
            fileDocument.requestRemark ??
            ""
          ).trim()
        : undefined;
    if (status === "Requested" && !remark)
      return setMessage("Request Remark / Status is required.");
    const applicationId = selected.application.id;
    documentActionInFlight.current = true;
    setDocumentActions((current) => ({
      ...current,
      [fileDocument.id]: {
        ...current[fileDocument.id],
        status,
        remark: remark || "",
        submitting: true,
      },
    }));
    setMessage("");
    try {
      await client.post(
        `/demand-applications/${applicationId}/documents/${fileDocument.id}/verification`,
        { status, remark },
      );
      const response = await client.get(
        `/demand-applications/${applicationId}`,
      );
      setSelected((current) =>
        current && current.application.id === applicationId
          ? { ...current, application: response.data.data }
          : current,
      );
      setDocumentActions((current) => {
        const next = { ...current };
        delete next[fileDocument.id];
        return next;
      });
      setMessage(
        status === "Requested"
          ? "Document request sent to applicant."
          : `Document marked ${status}.`,
      );
    } catch (error) {
      setMessage(
        error.response?.data?.messageMr ||
          error.response?.data?.message ||
          "Document status could not be saved.",
      );
      setDocumentActions((current) => ({
        ...current,
        [fileDocument.id]: { ...current[fileDocument.id], submitting: false },
      }));
    } finally {
      documentActionInFlight.current = false;
    }
  };
  const documentControl = (fileDocument) => {
    const action = documentActions[fileDocument.id] || {};
    const selectedStatus =
      action.status || fileDocument.verificationStatus || "";
    const submitting = Boolean(action.submitting);
    const requestAlreadyPending =
      fileDocument.verificationStatus === "Requested" && !action.status;
    return (
      <div
        key={fileDocument.id}
        className="card"
        style={{ padding: 12, display: "grid", gap: 10 }}
      >
        <div>
          <b>Document Name/Type:</b>{" "}
          {documentTypeLabel(fileDocument.documentType)}
          <br />
          <small>{fileDocument.fileName}</small>
        </div>
        <div style={{ display: "flex", gap: 8, flexWrap: "wrap" }}>
          <button
            type="button"
            className="btn btn-outline btn-sm"
            onClick={() => openOfficerDocument(fileDocument)}
          >
            Preview
          </button>
          <button
            type="button"
            className="btn btn-primary btn-sm"
            onClick={() => openOfficerDocument(fileDocument, true)}
          >
            Download
          </button>
        </div>
        {user?.role === "OS" && (
          <div style={{ display: "flex", gap: 14, flexWrap: "wrap" }}>
            {["Checked", "Unchecked", "Requested"].map((status) => (
              <label
                key={status}
                style={{ display: "inline-flex", gap: 5, alignItems: "center" }}
              >
                <input
                  type="radio"
                  name={`document-status-${fileDocument.id}`}
                  checked={selectedStatus === status}
                  disabled={submitting || requestAlreadyPending}
                  onChange={() => {
                    selectDocumentAction(fileDocument.id, status);
                    if (status !== "Requested")
                      setDocumentStatus(fileDocument, status);
                  }}
                />
                {status === "Requested" ? "Request" : status}
              </label>
            ))}
          </div>
        )}
        {selectedStatus === "Requested" && user?.role === "OS" && (
          <div style={{ display: "grid", gap: 8 }}>
            <label style={{ display: "grid", gap: 5 }}>
              <b>Request Remark/Status</b>
              <textarea
                className="form-input"
                rows={3}
                value={action.remark ?? fileDocument.requestRemark ?? ""}
                disabled={submitting || requestAlreadyPending}
                onChange={(event) =>
                  setDocumentRemark(fileDocument.id, event.target.value)
                }
              />
            </label>
            <button
              type="button"
              className="btn btn-primary btn-sm"
              disabled={
                submitting ||
                requestAlreadyPending ||
                !(action.remark ?? fileDocument.requestRemark ?? "").trim()
              }
              onClick={() => setDocumentStatus(fileDocument, "Requested")}
            >
              {submitting ? "Submitting..." : "Return to Applicant"}
            </button>
          </div>
        )}
        {(fileDocument.requestRemark ||
          fileDocument.verificationStatus ||
          fileDocument.requestedAt ||
          fileDocument.respondedAt) && (
          <div style={{ fontSize: 13, color: "#475569" }}>
            <b>Request Remark/Status:</b> {fileDocument.requestRemark || "-"} /{" "}
            {fileDocument.verificationStatus || "-"}
            {fileDocument.requestedAt && (
              <>
                <br />
                <small>
                  Requested:{" "}
                  {new Date(fileDocument.requestedAt).toLocaleString("mr-IN")}
                </small>
              </>
            )}
            {fileDocument.respondedAt && (
              <>
                <br />
                <small>
                  Responded:{" "}
                  {new Date(fileDocument.respondedAt).toLocaleString("mr-IN")}
                </small>
              </>
            )}
          </div>
        )}
      </div>
    );
  };
  const title =
    user?.role === "JE"
      ? "अधिकारी मागणी अर्ज — JE"
      : user?.role === "OS"
        ? "अधिकारी मागणी अर्ज — OS"
        : "सहाय्यक आयुक्त — मागणी अर्ज";
  const canResumeOsAction = (item) =>
    user?.role === "OS" &&
    ["OSPending", "PaymentRequired", "PaymentVerificationPending"].includes(
      item.stage,
    );
  useEffect(() => {
    const historyHeading = [...document.querySelectorAll("h2")].find(
      (heading) =>
        heading.textContent.includes(
          "माझे प्रक्रिया केलेले अर्ज",
        ),
    );
    const historyTable =
      historyHeading?.nextElementSibling?.querySelector("table");
    const historyRows = [...(historyTable?.querySelectorAll("tbody tr") || [])];
    const inserted = [];
    history.forEach((entry, index) => {
      const item = entry.workflow;
      if (!canResumeOsAction(item)) return;
      const historyRow = historyRows[index];
      const viewButton = [
        ...(historyRow?.querySelectorAll("button") || []),
      ].find((button) => button.textContent.trim() === "View");
      const activeQueueRow = [
        ...document.querySelectorAll("table tbody tr"),
      ].find(
        (row) =>
          row.closest("table") !== historyTable &&
          row.textContent.includes(item.applicationNumber),
      );
      if (!viewButton || !activeQueueRow) return;
      const actionButton = document.createElement("button");
      actionButton.type = "button";
      actionButton.className = "btn btn-primary btn-sm";
      actionButton.textContent = "Action";
      actionButton.style.marginLeft = "8px";
      actionButton.addEventListener("click", () => {
        activeQueueRow.scrollIntoView({ behavior: "smooth", block: "center" });
        activeQueueRow
          .querySelector("input, select, textarea, button")
          ?.focus({ preventScroll: true });
      });
      viewButton.insertAdjacentElement("afterend", actionButton);
      inserted.push(actionButton);
    });
    return () => inserted.forEach((button) => button.remove());
  }, [history, items, user?.role]);
  const paymentControl = (item, processing) => {
    const persistedStatus = [
      "PaymentDone",
      "PaymentVerified",
      "PaymentVerificationPending",
    ].includes(item.paymentStatus)
      ? "PaymentDone"
      : "PaymentPending";
    const selectedStatus = decisions[item.id]?.paymentStatus ?? persistedStatus;
    const selectPaymentStatus = (status, checked) =>
      setDecision(item.id, { paymentStatus: checked ? status : "" });
    const hasChangedStatus =
      Boolean(selectedStatus) && selectedStatus !== persistedStatus;
    return (
      <>
        <span style={{ fontSize: 12, fontWeight: 600, color: "#475569" }}>
          Payment Status: {paymentStatusLabel(item.paymentStatus)}
        </span>
        <label style={{ display: "inline-flex", gap: 6, fontSize: 13 }}>
          <input
            type="checkbox"
            checked={selectedStatus === "PaymentPending"}
            disabled={processing}
            onChange={(event) =>
              selectPaymentStatus("PaymentPending", event.target.checked)
            }
          />{" "}
          Payment Pending
        </label>
        <label style={{ display: "inline-flex", gap: 6, fontSize: 13 }}>
          <input
            type="checkbox"
            checked={selectedStatus === "PaymentDone"}
            disabled={processing}
            onChange={(event) =>
              selectPaymentStatus("PaymentDone", event.target.checked)
            }
          />{" "}
          Payment Done
        </label>
        <button
          className="btn btn-primary btn-sm"
          disabled={processing || !hasChangedStatus}
          onClick={() =>
            runAction(
              item,
              "payment-status",
              { status: selectedStatus },
              `${selectedStatus === "PaymentDone" ? "Payment Done" : "Payment Pending"} जतन केले आहे.`,
            )
          }
        >
          {processing ? "सादर करत आहे..." : "Submit"}
        </button>
        {persistedStatus === "PaymentDone" && (
          <button
            className="btn btn-primary btn-sm"
            disabled={processing}
            onClick={() =>
              runAction(
                item,
                "forward-to-assistant-commissioner",
                {},
                "अर्ज सहाय्यक आयुक्तांकडे पाठवला आहे.",
              )
            }
          >
            {processing
              ? "पाठवत आहे..."
              : "सहाय्यक आयुक्तांकडे पाठवा"}
          </button>
        )}
      </>
    );
  };
  const decisionControl = (item) => {
    const decision = decisions[item.id] || {};
    const processing = processingId === item.id;
    const paymentStage =
      user?.role === "OS" &&
      ["PaymentRequired", "PaymentVerificationPending"].includes(item.stage);
    if (paymentStage)
      return (
        <div style={{ minWidth: 214, display: "grid", gap: 6 }}>
          <button
            type="button"
            className="btn btn-outline btn-sm"
            onClick={() => openDetails(item)}
          >
            <EyeIcon /> View
          </button>
          {paymentControl(item, processing)}
        </div>
      );
    const finalApproval = item.stage === "AssistantCommissionerApprovalPending";
    return (
      <div style={{ minWidth: 214, display: "grid", gap: 6 }}>
        <button
          type="button"
          className="btn btn-outline btn-sm"
          onClick={() => openDetails(item)}
        >
          <EyeIcon /> View
        </button>
        <span style={{ fontSize: 12, fontWeight: 600 }}>
          निर्णय:
        </span>
        {finalApproval ? (
          <label>
            <input
              type="radio"
              name={`decision-${item.id}`}
              checked={decision.choice === "approve"}
              onChange={() => setDecision(item.id, { choice: "approve" })}
              disabled={processing}
            />{" "}
            मंजूर
          </label>
        ) : (
          <>
            <label>
              <input
                type="checkbox"
                checked={decision.choice === "accept"}
                onChange={(e) =>
                  setDecision(item.id, {
                    choice: e.target.checked ? "accept" : "",
                  })
                }
                disabled={processing}
              />{" "}
              Accept
            </label>
            <label>
              <input
                type="checkbox"
                checked={decision.choice === "reject"}
                onChange={(e) =>
                  setDecision(item.id, {
                    choice: e.target.checked ? "reject" : "",
                  })
                }
                disabled={processing}
              />{" "}
              Reject
            </label>
            {item.stage === "OSPending" && decision.choice === "accept" && (
              <label style={{ display: "grid", gap: 4, fontSize: 12 }}>
                शुल्क रक्कम{" "}
                <span>
                  ₹
                  <input
                    className="form-input"
                    type="number"
                    min="0.01"
                    step="0.01"
                    style={{ minHeight: 34, width: 142 }}
                    value={decision.amount || ""}
                    onChange={(e) =>
                      setDecision(item.id, { amount: e.target.value })
                    }
                    disabled={processing}
                  />
                </span>
              </label>
            )}
            {decision.choice === "reject" && (
              <input
                className="form-input"
                placeholder="नकाराचे कारण / शेरा *"
                value={decision.reason || ""}
                onChange={(e) =>
                  setDecision(item.id, { reason: e.target.value })
                }
                disabled={processing}
              />
            )}
          </>
        )}
        <button
          className="btn btn-primary btn-sm"
          disabled={
            processing ||
            !decision.choice ||
            (item.stage === "OSPending" &&
              decision.choice === "accept" &&
              !String(decision.amount ?? "").trim())
          }
          onClick={() => submitDecision(item)}
        >
          {processing
            ? "सादर करत आहे..."
            : item.stage === "OSPending" && decision.choice === "accept"
              ? "Send Payment Request to Applicant"
              : "Submit"}
        </button>
      </div>
    );
  };
  const showArea = user?.role === "OS";
  const sitePhoto = selected?.application.documents?.find(
    (document) => document.documentType === "SiteInspectionPhoto",
  );
  const sitePhotoSection =
    selected && (user?.role === "OS" || sitePhoto) ? (
      <>
        <h3 style={{ margin: "18px 0 6px" }}>
          प्रत्यक्ष जागेचा फोटो
        </h3>
        <div
          className="card"
          style={{
            padding: 12,
            display: "flex",
            alignItems: "center",
            gap: 10,
            flexWrap: "wrap",
          }}
        >
          {sitePhoto ? (
            <>
              <div style={{ flex: "1 1 240px" }}>
                <b>{sitePhoto.fileName}</b>
                <br />
                <small>
                  प्रत्यक्ष जागेचा फोटो
                </small>
              </div>
              <button
                type="button"
                className="btn btn-outline btn-sm"
                onClick={() => openOfficerDocument(sitePhoto)}
              >
                Preview
              </button>
            </>
          ) : (
            <span style={{ flex: "1 1 240px" }}>
              प्रत्यक्ष जागेचा फोटो
              उपलब्ध नाही.
            </span>
          )}
          {user?.role === "OS" && (
            <label
              className="btn btn-primary btn-sm"
              style={{
                cursor:
                  processingId === selected.workflow.id ? "wait" : "pointer",
              }}
            >
              {sitePhoto ? "Replace Image" : "Upload Image"}
              <input
                type="file"
                accept=".jpg,.jpeg,.png,image/jpeg,image/png"
                hidden
                disabled={processingId === selected.workflow.id}
                onChange={uploadSitePhoto}
              />
            </label>
          )}
        </div>
      </>
    ) : null;
  return (
    <div>
      <div className="page-header">
        <div>
          <div className="page-title">{title}</div>
          <div className="page-subtitle">
            अर्ज पडताळणी व कार्यवाही
          </div>
        </div>
        <button className="btn btn-outline" onClick={() => load()}>
          Refresh
        </button>
      </div>
      {message && (
        <div className="error-msg" role="status">
          {message}
        </div>
      )}
      {items.length > 0 ? (
        <>
          <h2>प्रलंबित अर्ज: {items.length}</h2>
          <div className="card" style={{ padding: 16, overflowX: "auto" }}>
            <table>
              <thead>
                <tr>
                  <th>अर्ज क्रमांक</th>
                  <th>अर्जदार</th>
                  <th>सेवा</th>
                  {showArea && <th>क्षेत्रफळ</th>}
                  <th>शुल्क</th>
                  <th>स्थिती</th>
                  <th>Payment Status</th>
                  <th>कार्यवाही</th>
                </tr>
              </thead>
              <tbody>
                {items.map((item) => (
                  <tr key={item.id}>
                    <td>{item.applicationNumber}</td>
                    <td>
                      <b>{item.applicantName || "-"}</b>
                      <br />
                      <small>{item.mobile || ""}</small>
                    </td>
                    <td>{item.serviceDescription || "-"}</td>
                    {showArea && <td>{item.spaceRequirement || "-"}</td>}
                    <td>
                      ₹
                      {Number(item.payableAmount || 0).toLocaleString("en-IN", {
                        minimumFractionDigits: 2,
                      })}
                    </td>
                    <td>{stageLabel(item.stage)}</td>
                    <td>{paymentStatusLabel(item.paymentStatus)}</td>
                    <td>{decisionControl(item)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </>
      ) : (
        !message && (
          <div className="card empty-state">
            कोणताही अर्ज प्रलंबित
            नाही.
          </div>
        )
      )}
      <h2 style={{ marginTop: 28 }}>
        माझे प्रक्रिया केलेले अर्ज:{" "}
        {history.length}
      </h2>
      {history.length > 0 ? (
        <div className="card" style={{ padding: 16, overflowX: "auto" }}>
          <table>
            <thead>
              <tr>
                <th>अर्ज क्रमांक</th>
                <th>अर्जदार</th>
                <th>कार्यवाही</th>
                <th>कार्यवाही दिनांक</th>
                <th>सध्याची स्थिती</th>
                <th>View</th>
              </tr>
            </thead>
            <tbody>
              {history.map((entry) => {
                const item = entry.workflow;
                return (
                  <tr key={`${item.id}-${entry.actionAt}`}>
                    <td>{item.applicationNumber}</td>
                    <td>
                      <b>{item.applicantName || "-"}</b>
                      <br />
                      <small>{item.mobile || ""}</small>
                    </td>
                    <td>{actionLabel(entry.action)}</td>
                    <td>
                      {entry.actionAt
                        ? new Date(entry.actionAt).toLocaleString("mr-IN")
                        : "-"}
                    </td>
                    <td>{stageLabel(item.stage)}</td>
                    <td>
                      <button
                        type="button"
                        className="btn btn-outline btn-sm"
                        onClick={() => openDetails(item)}
                      >
                        <EyeIcon /> View
                      </button>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      ) : (
        <div className="card empty-state">
          आपण प्रक्रिया केलेला
          कोणताही अर्ज नाही.
        </div>
      )}
      {selected && (
        <Modal
          title={`अर्ज तपशील — ${selected.application.applicationNumber}`}
          onClose={() => setSelected(null)}
          footer={
            <button
              className="btn btn-primary"
              onClick={() => setSelected(null)}
            >
              ठीक आहे
            </button>
          }
        >
          <div className="property-info-grid">
            <span>
              अर्जदार: <b>{selected.application.applicantName}</b>
            </span>
            <span>
              मोबाईल: <b>{selected.application.mobile}</b>
            </span>
            <span>
              ई-मेल: <b>{selected.application.email || "-"}</b>
            </span>
            <span>
              स्थिती: <b>{stageLabel(selected.workflow.stage)}</b>
            </span>
            <span>
              देयक स्थिती:{" "}
              <b>{paymentStatusLabel(selected.workflow.paymentStatus)}</b>
            </span>
            <span>
              शुल्क: <b>₹{selected.workflow.payableAmount || 0}</b>
            </span>
          </div>
          {sitePhotoSection}
          <h3 style={{ margin: "18px 0 6px" }}>कागदपत्रे</h3>
          {selected.application.documents?.filter(
            (document) => document.documentType !== "SiteInspectionPhoto",
          ).length ? (
            <div style={{ display: "grid", gap: 8 }}>
              {selected.application.documents
                .filter(
                  (document) => document.documentType !== "SiteInspectionPhoto",
                )
                .map(documentControl)}
            </div>
          ) : (
            <div className="empty-state">
              कोणतेही कागदपत्र उपलब्ध
              नाही.
            </div>
          )}
          <h3 style={{ margin: "18px 0 6px" }}>
            कार्यवाही इतिहास
          </h3>
          <AuditPanel
            entityName="DemandApplication"
            entityId={selected.application.id}
          />
        </Modal>
      )}
    </div>
  );
}
