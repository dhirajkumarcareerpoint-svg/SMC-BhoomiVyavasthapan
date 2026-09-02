"use client";

import { useEffect, useMemo, useRef, useState } from "react";
import client from "../api/client";
import Modal from "../components/Modal";
import { useAuth } from "../context/AuthContext";
import "./demand-officer-modal.css";

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
const pageSize = 8;
const emptyFilters = {
  search: "",
  status: "",
  service: "",
  dateFrom: "",
  dateTo: "",
};

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

const statusTone = (stage) =>
  stage === "Approved"
    ? "success"
    : stage === "Rejected"
      ? "danger"
      : stage === "AssistantCommissionerApprovalPending"
        ? "purple"
        : "warning";

function OfficerPagination({ page, pages, total, onChange }) {
  if (total === 0) return null;
  const start = (page - 1) * pageSize + 1;
  const end = Math.min(page * pageSize, total);
  return (
    <div className="officer-pagination">
      <span>{start} ते {end} पैकी {total} नोंदी</span>
      <div>
        <button type="button" onClick={() => onChange(1)} disabled={page === 1}>«</button>
        <button type="button" onClick={() => onChange(page - 1)} disabled={page === 1}>‹</button>
        <span className="officer-page-current">{page}</span>
        <span>/ {pages}</span>
        <button type="button" onClick={() => onChange(page + 1)} disabled={page === pages}>›</button>
        <button type="button" onClick={() => onChange(pages)} disabled={page === pages}>»</button>
      </div>
    </div>
  );
}

const majorWorkflowEvents = {
  "Final Submission": { role: "अर्जदार", label: "अर्ज सादर" },
  "Application Submitted": { role: "अर्जदार", label: "अर्ज सादर" },
  "JE Verified": { role: "JE", label: "अर्ज स्वीकारून OS कडे पाठवला" },
  "JE Rejected": { role: "JE", label: "अर्ज नाकारला" },
  "Payment Request Sent": { role: "OS", label: "पेमेंट विनंती पाठवली" },
  "OS Rejected": { role: "OS", label: "अर्ज नाकारला" },
  "Payment Submitted": { role: "अर्जदार", label: "पेमेंट पूर्ण केले" },
  "Payment status set to PaymentDone": { role: "OS", label: "पेमेंट पूर्ण नोंदवले" },
  "Payment Verified": { role: "OS", label: "पेमेंट पडताळले" },
  "Payment Rejected": { role: "OS", label: "पेमेंट नाकारले" },
  "Forwarded to Assistant Commissioner": { role: "OS", label: "सहाय्यक आयुक्तांकडे पाठवला" },
  "Final Approved": { role: "सहाय्यक आयुक्त", label: "अर्ज मंजूर केला" },
  "Assistant Commissioner Rejected": { role: "सहाय्यक आयुक्त", label: "अर्ज नाकारला" },
};

function MajorWorkflowHistory({ applicationId }) {
  const [logs, setLogs] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let active = true;
    setLoading(true);
    client
      .get("/auditlogs/entity", {
        params: { entityName: "DemandApplication", entityId: applicationId },
      })
      .then((response) => {
        if (active) setLogs(response.data.data || []);
      })
      .catch(() => {
        if (active) setLogs([]);
      })
      .finally(() => {
        if (active) setLoading(false);
      });
    return () => {
      active = false;
    };
  }, [applicationId]);

  const seen = new Set();
  const events = logs
    .filter((log) => majorWorkflowEvents[log.action])
    .filter((log) => {
      const event = majorWorkflowEvents[log.action];
      const key = event.label;
      if (seen.has(key)) return false;
      seen.add(key);
      return true;
    })
    .reverse();

  if (loading) return <div className="empty-state">कार्यवाही इतिहास लोड होत आहे...</div>;
  if (!events.length) return <div className="empty-state">मुख्य कार्यवाही इतिहास उपलब्ध नाही.</div>;

  return (
    <div className="officer-workflow-timeline">
      {events.map((log) => {
        const event = majorWorkflowEvents[log.action];
        const remark = log.action.includes("Rejected") ? log.newValue : null;
        return (
          <div className="officer-workflow-event" key={log.id}>
            <span className="officer-workflow-dot" aria-hidden="true" />
            <div>
              <strong>{event.label}</strong>
              <p>
                {event.role}
                {event.role !== "अर्जदार" && log.userName
                  ? ` — ${log.userName}`
                  : ""}
              </p>
              {remark && <small>शेरा: {remark}</small>}
            </div>
            <time>{new Date(log.timestamp).toLocaleString("mr-IN")}</time>
          </div>
        );
      })}
    </div>
  );
}

export default function DemandOfficerPage() {
  const { user } = useAuth();
  const [items, setItems] = useState([]);
  const [history, setHistory] = useState([]);
  const [documentActions, setDocumentActions] = useState({});
  const [message, setMessage] = useState("");
  const [deleteFeedback, setDeleteFeedback] = useState(null);
  const [processingId, setProcessingId] = useState(null);
  const [deletingId, setDeletingId] = useState(null);
  const [selected, setSelected] = useState(null);
  const [decisions, setDecisions] = useState({});
  const [filterDraft, setFilterDraft] = useState(emptyFilters);
  const [filters, setFilters] = useState(emptyFilters);
  const [pendingPage, setPendingPage] = useState(1);
  const [historyPage, setHistoryPage] = useState(1);
  const actionInFlight = useRef(false);
  const documentActionInFlight = useRef(false);
  const deleteInFlight = useRef(false);
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
  const deleteApplication = async (item) => {
    if (deleteInFlight.current) return;
    if (!window.confirm("हा अर्ज कायमचा हटवायचा आहे का?")) return;

    deleteInFlight.current = true;
    setDeletingId(item.id);
    setDeleteFeedback(null);
    try {
      await client.delete(`/demand-applications/${item.demandApplicationId}`);
      setItems((current) => current.filter((row) => row.id !== item.id));
      setHistory((current) =>
        current.filter(
          (entry) => entry.workflow.demandApplicationId !== item.demandApplicationId,
        ),
      );
      setSelected((current) =>
        current?.application.id === item.demandApplicationId ? null : current,
      );
      await load(true);
      setDeleteFeedback({
        type: "success",
        text: "अर्ज यशस्वीरीत्या हटवला आहे.",
      });
    } catch (error) {
      setDeleteFeedback({
        type: "error",
        text:
          error.response?.data?.messageMr ||
          error.response?.data?.message ||
          "अर्ज हटवता आला नाही. कृपया पुन्हा प्रयत्न करा.",
      });
    } finally {
      setDeletingId(null);
      deleteInFlight.current = false;
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
    if (paymentRequest && (!Number.isFinite(Number(item.payableAmount)) || Number(item.payableAmount) <= 0))
      return setMessage(
        "अर्जाची गणना केलेली शुल्क रक्कम उपलब्ध किंवा वैध नाही. पेमेंट विनंती पाठवता येणार नाही.",
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
        ? {}
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
  const allWorkflows = useMemo(() => {
    const unique = new Map();
    items.forEach((item) => unique.set(item.demandApplicationId, item));
    history.forEach((entry) => {
      if (!unique.has(entry.workflow.demandApplicationId))
        unique.set(entry.workflow.demandApplicationId, entry.workflow);
    });
    return [...unique.values()];
  }, [items, history]);
  const services = useMemo(
    () => [...new Set(allWorkflows.map((item) => item.serviceDescription).filter(Boolean))].sort(),
    [allWorkflows],
  );
  const matchesFilters = (item) => {
    const term = filters.search.trim().toLocaleLowerCase("mr-IN");
    const searchable = `${item.applicationNumber || ""} ${item.applicantName || ""} ${item.mobile || ""}`.toLocaleLowerCase("mr-IN");
    const submittedDate = item.submittedAt?.slice(0, 10) || "";
    return (
      (!term || searchable.includes(term)) &&
      (!filters.status || item.stage === filters.status) &&
      (!filters.service || item.serviceDescription === filters.service) &&
      (!filters.dateFrom || (submittedDate && submittedDate >= filters.dateFrom)) &&
      (!filters.dateTo || (submittedDate && submittedDate <= filters.dateTo))
    );
  };
  const filteredItems = items.filter(matchesFilters);
  const filteredHistory = history.filter((entry) => matchesFilters(entry.workflow));
  const pendingPages = Math.max(1, Math.ceil(filteredItems.length / pageSize));
  const historyPages = Math.max(1, Math.ceil(filteredHistory.length / pageSize));
  const visibleItems = filteredItems.slice((pendingPage - 1) * pageSize, pendingPage * pageSize);
  const visibleHistory = filteredHistory.slice((historyPage - 1) * pageSize, historyPage * pageSize);
  useEffect(() => {
    setPendingPage((current) => Math.min(current, pendingPages));
  }, [pendingPages]);
  useEffect(() => {
    setHistoryPage((current) => Math.min(current, historyPages));
  }, [historyPages]);
  const summary = {
    total: allWorkflows.length,
    pending: items.length,
    processed: new Set(history.map((entry) => entry.workflow.demandApplicationId)).size,
    approved: allWorkflows.filter((item) => item.stage === "Approved").length,
    rejected: allWorkflows.filter((item) => item.stage === "Rejected").length,
  };
  const applyFilters = (event) => {
    event.preventDefault();
    setFilters(filterDraft);
    setPendingPage(1);
    setHistoryPage(1);
  };
  const resetFilters = () => {
    setFilterDraft(emptyFilters);
    setFilters(emptyFilters);
    setPendingPage(1);
    setHistoryPage(1);
  };
  const resumeOsAction = (item) => {
    const row = document.querySelector(`[data-workflow-id="${item.id}"]`);
    row?.scrollIntoView({ behavior: "smooth", block: "center" });
    row?.querySelector("button, input, select, textarea")?.focus({ preventScroll: true });
  };
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
          <button
            type="button"
            className="btn btn-danger btn-sm"
            disabled={processing || deletingId === item.id}
            onClick={() => deleteApplication(item)}
          >
            {deletingId === item.id ? "हटवत आहे..." : "Delete"}
          </button>
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
              <div
                style={{
                  display: "grid",
                  gap: 3,
                  padding: "8px 10px",
                  border: "1px solid #bfdbfe",
                  borderRadius: 6,
                  background: "#eff6ff",
                  color: "#475569",
                  fontSize: 11,
                }}
              >
                <span>गणना केलेली शुल्क रक्कम</span>
                <strong style={{ color: "#164e8a", fontSize: 15 }}>
                  ₹{Number(item.payableAmount || 0).toLocaleString("en-IN", {
                    minimumFractionDigits: 2,
                    maximumFractionDigits: 2,
                  })}
                </strong>
              </div>
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
              Number(item.payableAmount) <= 0)
          }
          onClick={() => submitDecision(item)}
        >
          {processing
            ? "सादर करत आहे..."
            : item.stage === "OSPending" && decision.choice === "accept"
              ? "Send Payment Request to Applicant"
              : "Submit"}
        </button>
        <button
          type="button"
          className="btn btn-danger btn-sm"
          disabled={processing || deletingId === item.id}
          onClick={() => deleteApplication(item)}
        >
          {deletingId === item.id ? "हटवत आहे..." : "Delete"}
        </button>
      </div>
    );
  };
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
    <div className="officer-dashboard">
      <header className="officer-page-head">
        <div>
          <div className="officer-breadcrumb">Dashboard / सर्व मागणी अर्ज</div>
          <h1>{title}</h1>
          <p>प्राप्त अर्जांची पडताळणी, स्थिती आणि कार्यवाही</p>
        </div>
        <div className="officer-profile-chip">
          <span>{user?.role || "Officer"}</span>
          <b>{user?.fullName || user?.username || "अधिकारी"}</b>
          <button className="btn btn-outline btn-sm" onClick={() => load()}>
            Refresh
          </button>
        </div>
      </header>

      <section className="officer-summary-grid" aria-label="अर्जांचा सारांश">
        <article className="officer-summary-card blue">
          <span className="officer-summary-icon">▤</span>
          <div><small>एकूण अर्ज</small><strong>{summary.total}</strong><p>सर्व संबंधित अर्ज</p></div>
        </article>
        <article className="officer-summary-card orange">
          <span className="officer-summary-icon">◷</span>
          <div><small>प्रलंबित अर्ज</small><strong>{summary.pending}</strong><p>कार्यवाही प्रलंबित</p></div>
        </article>
        <article className="officer-summary-card purple">
          <span className="officer-summary-icon">➜</span>
          <div><small>प्रक्रिया केलेले / पुढे पाठवलेले</small><strong>{summary.processed}</strong><p>आपली कार्यवाही पूर्ण</p></div>
        </article>
        <article className="officer-summary-card green">
          <span className="officer-summary-icon">✓</span>
          <div><small>मंजूर अर्ज</small><strong>{summary.approved}</strong><p>अंतिम मंजूर अर्ज</p></div>
        </article>
        <article className="officer-summary-card red">
          <span className="officer-summary-icon">×</span>
          <div><small>नाकारलेले अर्ज</small><strong>{summary.rejected}</strong><p>नाकारलेले अर्ज</p></div>
        </article>
      </section>

      <form className="officer-filter-bar" onSubmit={applyFilters}>
        <label className="officer-search-field">
          शोधा
          <input
            className="form-input"
            value={filterDraft.search}
            onChange={(event) => setFilterDraft((current) => ({ ...current, search: event.target.value }))}
            placeholder="अर्ज क्रमांक / अर्जदाराचे नाव / मोबाईल"
          />
        </label>
        <label>
          स्थिती
          <select className="form-input" value={filterDraft.status} onChange={(event) => setFilterDraft((current) => ({ ...current, status: event.target.value }))}>
            <option value="">सर्व स्थिती</option>
            {Object.entries(stageLabels).map(([value, label]) => <option key={value} value={value}>{label}</option>)}
          </select>
        </label>
        <label>
          सेवा
          <select className="form-input" value={filterDraft.service} onChange={(event) => setFilterDraft((current) => ({ ...current, service: event.target.value }))}>
            <option value="">सर्व सेवा</option>
            {services.map((service) => <option key={service} value={service}>{service}</option>)}
          </select>
        </label>
        <label>
          दिनांक पासून
          <input className="form-input" type="date" value={filterDraft.dateFrom} onChange={(event) => setFilterDraft((current) => ({ ...current, dateFrom: event.target.value }))} />
        </label>
        <label>
          दिनांक पर्यंत
          <input className="form-input" type="date" value={filterDraft.dateTo} onChange={(event) => setFilterDraft((current) => ({ ...current, dateTo: event.target.value }))} />
        </label>
        <div className="officer-filter-actions">
          <button type="button" className="btn btn-outline btn-sm" onClick={resetFilters}>रीसेट</button>
          <button type="submit" className="btn btn-primary btn-sm">शोधा</button>
        </div>
      </form>

      {message && (
        <div className="error-msg" role="status">
          {message}
        </div>
      )}
      {deleteFeedback && (
        <div
          className={deleteFeedback.type === "success" ? "success-msg" : "error-msg"}
          role="status"
        >
          {deleteFeedback.text}
        </div>
      )}
      <section className="officer-table-card">
        <div className="officer-table-head">
          <div><h2>प्रलंबित अर्ज</h2><span>{filteredItems.length} अर्ज</span></div>
        </div>
        {visibleItems.length > 0 ? (
          <>
          <div className="officer-table-wrap">
            <table className="officer-table">
              <thead>
                <tr>
                  <th>अर्ज क्रमांक</th>
                  <th>अर्जदाराचे नाव</th>
                  <th>सेवा</th>
                  <th>मोबाईल</th>
                  <th>अर्ज दिनांक</th>
                  <th>स्थिती</th>
                  <th>Payment Status</th>
                  <th>कार्यवाही</th>
                </tr>
              </thead>
              <tbody>
                {visibleItems.map((item) => (
                  <tr key={item.id} data-workflow-id={item.id}>
                    <td>{item.applicationNumber}</td>
                    <td><b>{item.applicantName || "-"}</b></td>
                    <td>{item.serviceDescription || "-"}</td>
                    <td>{item.mobile || "-"}</td>
                    <td>{item.submittedAt ? new Date(item.submittedAt).toLocaleDateString("mr-IN") : "-"}</td>
                    <td><span className={`officer-status-badge ${statusTone(item.stage)}`}>{stageLabel(item.stage)}</span></td>
                    <td><span className={`officer-status-badge ${item.paymentStatus?.includes("Done") || item.paymentStatus === "PaymentVerified" ? "success" : "neutral"}`}>{paymentStatusLabel(item.paymentStatus)}</span></td>
                    <td className="officer-action-cell">{decisionControl(item)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          <OfficerPagination page={pendingPage} pages={pendingPages} total={filteredItems.length} onChange={setPendingPage} />
          </>
        ) : <div className="empty-state">शोध निकषांनुसार कोणताही प्रलंबित अर्ज नाही.</div>}
      </section>

      <section className="officer-table-card">
        <div className="officer-table-head"><div><h2>माझे प्रक्रिया केलेले अर्ज</h2><span>{filteredHistory.length} अर्ज</span></div></div>
        {visibleHistory.length > 0 ? (
          <>
          <div className="officer-table-wrap">
          <table className="officer-table">
            <thead>
              <tr>
                <th>अर्ज क्रमांक</th>
                <th>अर्जदाराचे नाव</th>
                <th>सेवा</th>
                <th>मोबाईल</th>
                <th>कार्यवाही</th>
                <th>कार्यवाही दिनांक</th>
                <th>सध्याची स्थिती</th>
                <th>Action</th>
              </tr>
            </thead>
            <tbody>
              {visibleHistory.map((entry) => {
                const item = entry.workflow;
                return (
                  <tr key={`${item.id}-${entry.actionAt}`}>
                    <td>{item.applicationNumber}</td>
                    <td><b>{item.applicantName || "-"}</b></td>
                    <td>{item.serviceDescription || "-"}</td>
                    <td>{item.mobile || "-"}</td>
                    <td>{actionLabel(entry.action)}</td>
                    <td>
                      {entry.actionAt
                        ? new Date(entry.actionAt).toLocaleString("mr-IN")
                        : "-"}
                    </td>
                    <td><span className={`officer-status-badge ${statusTone(item.stage)}`}>{stageLabel(item.stage)}</span></td>
                    <td className="officer-history-actions">
                      <button
                        type="button"
                        className="btn btn-outline btn-sm"
                        onClick={() => openDetails(item)}
                      >
                        <EyeIcon /> View
                      </button>
                      <button
                        type="button"
                        className="btn btn-danger btn-sm"
                        style={{ marginLeft: 8 }}
                        disabled={deletingId === item.id}
                        onClick={() => deleteApplication(item)}
                      >
                        {deletingId === item.id ? "हटवत आहे..." : "Delete"}
                      </button>
                      {canResumeOsAction(item) && (
                        <button type="button" className="btn btn-primary btn-sm" onClick={() => resumeOsAction(item)}>Action</button>
                      )}
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
          </div>
          <OfficerPagination page={historyPage} pages={historyPages} total={filteredHistory.length} onChange={setHistoryPage} />
          </>
        ) : <div className="empty-state">शोध निकषांनुसार प्रक्रिया केलेला कोणताही अर्ज नाही.</div>}
      </section>
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
          <h3 className="officer-detail-heading">जागेच्या मागणीचा तपशील</h3>
          <div className="officer-space-details">
            <div><span>सेवा / जागेचा प्रकार</span><b>{selected.application.serviceDescription || selected.application.spaceRequirement || "-"}</b></div>
            <div><span>Length (ft)</span><b>{selected.application.lengthFt ?? "-"}</b></div>
            <div><span>Width (ft)</span><b>{selected.application.widthFt ?? "-"}</b></div>
            <div><span>Total Area (sq ft)</span><b>{selected.application.areaSqFt ?? "-"}</b></div>
            <div><span>Calculated Rate / Amount</span><b>{selected.application.calculatedRate != null ? `₹${Number(selected.application.calculatedRate).toLocaleString("en-IN", { minimumFractionDigits: 2, maximumFractionDigits: 2 })}` : "-"}</b></div>
            <div><span>जागेची आवश्यकता</span><b>{selected.application.spaceRequirement || "-"}</b></div>
            <div><span>ठिकाण</span><b>{selected.application.location || "-"}</b></div>
            <div><span>उपलब्ध जागा</span><b>{selected.application.availableSpace || "-"}</b></div>
            <div><span>मागणीचा कालावधी</span><b>{selected.application.requiredDuration || "-"}</b></div>
            <div><span>सुविधा</span><b>{[selected.application.electricityRequired && "वीज", selected.application.waterRequired && "पाणी", selected.application.otherFacilities].filter(Boolean).join(", ") || "-"}</b></div>
            {selected.application.otherInformation && (
              <div className="wide"><span>इतर माहिती</span><b>{selected.application.otherInformation}</b></div>
            )}
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
          <MajorWorkflowHistory applicationId={selected.application.id} />
        </Modal>
      )}
    </div>
  );
}
