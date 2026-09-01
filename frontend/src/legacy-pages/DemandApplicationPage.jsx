"use client";

import { useEffect, useRef, useState } from "react";
import { useRouter } from "next/navigation";
import client from "../api/client";
import Modal from "../components/Modal";
import { useAuth } from "../context/AuthContext";

const TrashIcon = () => (
  <svg aria-hidden="true" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <path d="M3 6h18" />
    <path d="M8 6V4h8v2" />
    <path d="M19 6l-1 14H6L5 6" />
    <path d="M10 11v5M14 11v5" />
  </svg>
);

const DownloadIcon = () => (
  <svg aria-hidden="true" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <path d="M12 3v12" />
    <path d="m7 10 5 5 5-5" />
    <path d="M5 21h14" />
  </svg>
);

const serviceTypes = [
  { value: 3, label: "विविध व्यवसायासाठी जागा मागणी" },
  { value: 2, label: "मेजर गाळे" },
  { value: 1, label: "मिनी गाळे" },
  { value: 4, label: "Land Fee (भुई भाडे)" },
  { value: 5, label: "समाज मंदिर" },
  { value: 6, label: "अभ्यासिका" },
  { value: 7, label: "256 गाळे" },
  { value: 8, label: "TP-3/23" },
  { value: 9, label: "अधिकृत खोके" },
  { value: 10, label: "इतर भाडेतत्त्वावरील मनपा मालमत्ता" },
];
const businessTypes = [
  { value: 1, label: "फटाके स्टॉल" },
  { value: 2, label: "गणपती मूर्ती स्टॉल" },
  { value: 3, label: "रंगपंचमी स्टॉल" },
  { value: 4, label: "रक्षाबंधन स्टॉल" },
  { value: 5, label: "दिवाळी फराळ स्टॉल" },
  { value: 7, label: "दिवाळी फटाके स्टॉल" },
  { value: 6, label: "Other" },
];
const serviceDescriptionOptions = businessTypes.map(({ label }) => ({ value: label, label }));
const prabhags = Array.from({ length: 26 }, (_, i) => ({
  value: `Prabhag ${i + 1}`,
  label: `प्रभाग ${String(i + 1).replace(/[0-9]/g, (n) => "०१२३४५६७८९"[n])}`,
}));
const talukas = [
  { value: "सोलापूर शहर मध्य", label: "सोलापूर शहर मध्य" },
  { value: "उत्तर सोलापूर", label: "उत्तर सोलापूर" },
  { value: "दक्षिण सोलापूर", label: "दक्षिण सोलापूर" },
];
const today = new Date().toISOString().slice(0, 10);
const getDuration = (startDate, endDate) => {
  if (!startDate || !endDate) return "";
  const start = new Date(`${startDate}T00:00:00Z`);
  const end = new Date(`${endDate}T00:00:00Z`);
  const days = Math.floor((end - start) / 86400000) + 1;
  return days > 0 ? String(days) : "";
};
const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
const emailValidationMessage = "कृपया वैध ई-मेल आयडी प्रविष्ट करा.";
const temporaryApplicationStorageKey = "smc_demand_temporary_application";
const temporaryApplicationLifetimeMs = 5 * 60 * 1000;

const readTemporaryApplication = () => {
  try {
    const raw = window.localStorage.getItem(temporaryApplicationStorageKey);
    if (!raw) return null;

    const item = JSON.parse(raw);
    if (!item || !Number.isInteger(item.id) || !item.applicationNumber || !item.accessToken || !Number.isFinite(item.submittedAt)) {
      window.localStorage.removeItem(temporaryApplicationStorageKey);
      return null;
    }

    if (item.submittedAt + temporaryApplicationLifetimeMs <= Date.now()) {
      window.localStorage.removeItem(temporaryApplicationStorageKey);
      return null;
    }

    return item;
  } catch {
    window.localStorage.removeItem(temporaryApplicationStorageKey);
    return null;
  }
};
const getEmailError = (value) => {
  const email = String(value ?? "").trim();
  return email && !emailPattern.test(email) ? emailValidationMessage : "";
};
const initialForm = {
  serviceType: "",
  businessType: "",
  lengthFt: "",
  widthFt: "",
  areaSqFt: "",
  calculatedRate: "",
  otherBusinessType: "",
  applicantType: "Individual",
  applicantName: "",
  mobile: "",
  email: "",
  identityNumber: "",
  panNumber: "",
  gstNumber: "",
  permanentAddress: "",
  correspondenceAddress: "",
  sameAddress: false,
  state: "महाराष्ट्र",
  district: "सोलापूर",
  city: "सोलापूर",
  taluka: "",
  pinCode: "",
  prabhag: "",
  serviceDescription: "",
  spaceRequirement: "",
  otherInformation: "",
  startDate: today,
  endDate: today,
  requiredDuration: getDuration(today, today),
  electricityRequired: false,
  waterRequired: false,
  otherFacilities: "",
  wasteManagement: "",
  declarationAccepted: false,
  feeAmount: "",
};

const statusLabels = {
  1: "सादर",
  2: "छाननी सुरू",
  3: "कागदपत्रांची कमतरता",
  4: "पुन्हा सादर",
  5: "स्थळ पडताळणी प्रलंबित",
  6: "स्थळ पडताळणी पूर्ण",
  7: "शुल्क प्रलंबित",
  8: "मंजुरी प्रलंबित",
  9: "मंजूर",
  10: "नाकारले",
  11: "परवानगी जारी",
  12: "रद्द",
};
const workflowStatusLabels = {
  JEPending: "JE तपासणीसाठी प्रलंबित",
  OSPending: "OS तपासणीसाठी प्रलंबित",
  PaymentRequired: "शुल्क भरणे आवश्यक",
  PaymentVerificationPending: "देयक भरले - पडताळणी प्रलंबित",
  AssistantCommissionerApprovalPending: "सहाय्यक आयुक्त मंजुरीसाठी प्रलंबित",
  Approved: "मंजूर",
  Rejected: "नाकारले",
};
const stepFields = {
  1: ["serviceType"],
  2: ["applicantName", "mobile"],
  3: [
    "permanentAddress",
    "correspondenceAddress",
    "state",
    "district",
    "city",
    "taluka",
    "pinCode",
    "prabhag",
    "serviceDescription",
    "spaceRequirement",
  ],
  4: ["startDate", "endDate", "requiredDuration"],
};
const allRequiredFields = Object.values(stepFields).flat();
const fixedAddressValues = {
  state: "महाराष्ट्र",
  district: "सोलापूर",
  city: "सोलापूर",
};

export default function DemandApplicationPage() {
  const router = useRouter();
  const { user, hydrated } = useAuth();
  const [form, setForm] = useState(initialForm);
  const [step, setStep] = useState(1);
  const [applications, setApplications] = useState([]);
  const [workflows, setWorkflows] = useState({});
  const [current, setCurrent] = useState(null);
  const [errors, setErrors] = useState({});
  const [message, setMessage] = useState("");
  const [saving, setSaving] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [uploading, setUploading] = useState(false);
  const [submissionSuccess, setSubmissionSuccess] = useState(null);
  const [temporaryApplication, setTemporaryApplication] = useState(null);
  const [applicantAccessToken, setApplicantAccessToken] = useState("");
  const saveInFlight = useRef(false);
  const uploadInFlight = useRef(false);
  // All authenticated staff roles are officer sessions. The explicit applicant
  // roles remain available for a future applicant-authenticated flow.
  const isOfficerSession = Boolean(user) && !["Applicant", "Citizen", "Public"].includes(user.role);

  const clearTemporaryApplication = () => {
    window.localStorage.removeItem(temporaryApplicationStorageKey);
    setTemporaryApplication(null);
  };

  const storeTemporaryApplication = (application, accessToken = applicantAccessToken) => {
    const submittedAt = new Date(application.submittedAt || Date.now()).getTime();
    const item = {
      id: application.id,
      applicationNumber: application.applicationNumber,
      accessToken,
      submittedAt: Number.isFinite(submittedAt) ? submittedAt : Date.now(),
    };
    window.localStorage.setItem(temporaryApplicationStorageKey, JSON.stringify(item));
    setTemporaryApplication(item);
  };

  const set = (name, value) => {
    if (Object.prototype.hasOwnProperty.call(fixedAddressValues, name)) {
      value = fixedAddressValues[name];
    }
    setForm((previous) => {
      const next = { ...previous, [name]: value };
      if (name === "lengthFt" || name === "widthFt") {
        const length = Number(name === "lengthFt" ? value : previous.lengthFt);
        const width = Number(name === "widthFt" ? value : previous.widthFt);
        const calculated = Number.isFinite(length) && Number.isFinite(width) && length > 0 && width > 0 ? Math.round(length * width * 100) / 100 : "";
        next.areaSqFt = calculated;
        next.calculatedRate = calculated;
      }
      return next;
    });
    if (name === "email") {
      const error = getEmailError(value);
      setErrors((previous) => {
        const next = { ...previous };
        if (error) next.email = error;
        else delete next.email;
        return next;
      });
    }
  };
  const setDate = (name, value) =>
    setForm((previous) => ({
      ...previous,
      [name]: value,
      requiredDuration: getDuration(
        name === "startDate" ? value : previous.startDate,
        name === "endDate" ? value : previous.endDate,
      ),
    }));
  const setService = (value) =>
    setForm((previous) => ({
      ...previous,
      serviceType: value,
      businessType: Number(value) === 3 ? previous.businessType : "",
      otherBusinessType: Number(value) === 3 ? previous.otherBusinessType : "",
    }));
  const setBusiness = (value) =>
    setForm((previous) => ({
      ...previous,
      businessType: value,
      otherBusinessType: Number(value) === 6 ? previous.otherBusinessType : "",
    }));
  const setSameAddress = (checked) =>
    setForm((previous) => ({
      ...previous,
      sameAddress: checked,
      correspondenceAddress: checked
        ? previous.permanentAddress
        : previous.correspondenceAddress,
    }));
  const validate = (fields = allRequiredFields) => {
    const next = {};

    // Step 1: Service Information validation
    if (fields.includes("serviceType")) {
      if (!form.serviceType) {
        next.serviceType = "सेवेचा प्रकार निवडणे आवश्यक आहे.";
      } else if (Number(form.serviceType) === 3) {
        // Conditional: If "विविध व्यवसायासाठी जागा मागणी" is selected, business type is required
        if (!form.businessType) {
          next.businessType = "व्यवसायाचा प्रकार निवडणे आवश्यक आहे.";
        } else if (
          Number(form.businessType) === 6 &&
          !form.otherBusinessType.trim()
        ) {
          // If "Other" is selected, other business type is required and must be non-empty after trim
          next.otherBusinessType = "कृपया व्यवसायाचा प्रकार प्रविष्ट करा.";
        }
      }
    }

    // Step 2: Applicant Information validation
    if (fields.includes("applicantName")) {
      const nameValue = String(form.applicantName ?? "").trim();
      if (!nameValue) {
        next.applicantName = "कृपया अर्जदाराचे पूर्ण नाव प्रविष्ट करा.";
      } else if (nameValue.length > 100) {
        next.applicantName = "नाव 100 अक्षरांपेक्षा जास्त असू शकत नाही.";
      }
    }

    if (fields.includes("mobile")) {
      if (!form.mobile) {
        next.mobile = "कृपया 10 अंकी मोबाईल क्रमांक प्रविष्ट करा.";
      } else if (!/^\d{10}$/.test(form.mobile)) {
        next.mobile = "कृपया 10 अंकी मोबाईल क्रमांक प्रविष्ट करा.";
      }
    }

    // Optional email validation (only if entered)
    if (
      fields.includes("mobile") &&
      getEmailError(form.email)
    ) {
      next.email = emailValidationMessage;
    }

    // Optional PAN validation (only if entered)
    if (
      fields.includes("mobile") &&
      form.panNumber &&
      !/^[A-Z]{5}[0-9]{4}[A-Z]$/.test(form.panNumber)
    ) {
      next.panNumber = "कृपया वैध PAN क्रमांक प्रविष्ट करा.";
    }

    // Optional GST validation (only if entered)
    if (
      fields.includes("mobile") &&
      form.gstNumber &&
      !/^[0-9A-Z]{15}$/.test(form.gstNumber)
    ) {
      next.gstNumber = "कृपया वैध GST क्रमांक प्रविष्ट करा.";
    }

    // Optional Aadhaar/Identity validation (only if entered, basic numeric check)
    if (
      fields.includes("mobile") &&
      form.identityNumber &&
      !/^\d{12,16}$/.test(form.identityNumber)
    ) {
      next.identityNumber = "कृपया वैध आधार/ओळखपत्र क्रमांक प्रविष्ट करा.";
    }

    // Step 3: Address and Location validation
    if (fields.includes("permanentAddress")) {
      if (!String(form.permanentAddress ?? "").trim()) {
        next.permanentAddress = "कृपया कायमचा पत्ता प्रविष्ट करा.";
      }
    }

    if (fields.includes("correspondenceAddress")) {
      if (!String(form.correspondenceAddress ?? "").trim()) {
        next.correspondenceAddress = "कृपया पत्रव्यवहाराचा पत्ता प्रविष्ट करा.";
      }
    }

    if (fields.includes("taluka")) {
      if (!form.taluka) {
        next.taluka = "कृपया तालुका निवडा.";
      }
    }

    if (fields.includes("pinCode")) {
      if (!form.pinCode) {
        next.pinCode = "कृपया पिनकोड प्रविष्ट करा.";
      } else if (!/^\d{6}$/.test(form.pinCode)) {
        next.pinCode = "कृपया वैध 6 अंकी पिनकोड प्रविष्ट करा.";
      }
    }

    if (fields.includes("prabhag") && !form.prabhag) {
      next.prabhag = "कृपया प्रभाग निवडा.";
    }

    if (fields.includes("serviceDescription")) {
      if (!String(form.serviceDescription ?? "").trim()) {
        next.serviceDescription = "कृपया सेवा/विक्रीचा प्रकार प्रविष्ट करा.";
      }
    }

    if (fields.includes("spaceRequirement")) {
      if (!String(form.spaceRequirement ?? "").trim()) {
        next.spaceRequirement = "कृपया स्टॉल/जागेची आवश्यकता प्रविष्ट करा.";
      }
    }

    // Default validations for other required fields
    fields.forEach((name) => {
      if (
        ![
          "serviceType",
          "applicantName",
          "mobile",
          "permanentAddress",
          "correspondenceAddress",
          "taluka",
          "pinCode",
          "serviceDescription",
          "spaceRequirement",
          "prabhag",
        ].includes(name)
      ) {
        if (!String(form[name] ?? "").trim() && !next[name]) {
          next[name] = "हे क्षेत्र आवश्यक आहे.";
        }
      }
    });

    // Date and numeric validations
    if (fields.includes("startDate") && !form.startDate)
      next.startDate = "कृपया प्रारंभ तारीख निवडा.";
    if (fields.includes("endDate") && !form.endDate)
      next.endDate = "कृपया समाप्ती तारीख निवडा.";
    if (
      fields.includes("startDate") &&
      fields.includes("endDate") &&
      form.startDate &&
      form.endDate &&
      form.endDate < form.startDate
    )
      next.endDate = "समाप्ती तारीख प्रारंभ तारीखेपेक्षा आधी असू शकत नाही.";
    if (
      fields.includes("requiredDuration") &&
      form.startDate &&
      form.endDate &&
      form.endDate >= form.startDate &&
      form.requiredDuration !== getDuration(form.startDate, form.endDate)
    )
      next.requiredDuration = "आवश्यक कालावधी निवडलेल्या तारखांशी जुळत नाही.";

    setErrors(next);
    return Object.keys(next).length === 0;
  };
  const goToStep = (targetStep) => {
    const target = Math.max(1, Math.min(4, targetStep));
    if (target <= step) {
      setStep(target);
      return;
    }
    for (let requiredStep = 1; requiredStep < target; requiredStep += 1) {
      if (!validate(stepFields[requiredStep])) {
        setStep(requiredStep);
        return;
      }
    }
    setStep(target);
  };
  const nextStep = () => goToStep(step + 1);
  const previousStep = () => setStep((value) => Math.max(1, value - 1));
  const save = async (submit) => {
    if (saveInFlight.current) return;
    if (!validate() || (submit && !form.declarationAccepted)) {
      if (submit && !form.declarationAccepted)
        setErrors((previous) => ({
          ...previous,
          declarationAccepted: "अंतिम घोषणा स्वीकारणे आवश्यक आहे.",
        }));
      return;
    }
    saveInFlight.current = true;
    setSaving(true);
    setSubmitting(submit);
    setMessage("");
    try {
      const { location, availableSpace, feeAmount, ...application } = form;
      const payload = {
        ...application,
        state: "Maharashtra",
        district: "Solapur",
        city: "Solapur",
        // Keep historical values intact when an older draft is edited; these fields
        // are no longer exposed or collected for new applications.
        ...(current
          ? {
              location: current.location ?? "",
              availableSpace: current.availableSpace ?? "",
              areaSqFt: current.areaSqFt ?? null,
            }
          : {}),
        serviceType: Number(form.serviceType),
        businessType: form.businessType ? Number(form.businessType) : null,
        feeAmount: feeAmount === "" ? null : Number(feeAmount),
      };
      const response = current
        ? await client.put(
            isOfficerSession ? `/demand-applications/${current.id}` : `/demand-applications/public/${current.id}`,
            payload,
            isOfficerSession ? undefined : { headers: { "X-Demand-Application-Token": applicantAccessToken } },
          )
        : await client.post(isOfficerSession ? "/demand-applications" : "/demand-applications/public", payload);
      let saved = isOfficerSession ? response.data.data : (current ? response.data.data : response.data.data.application);
      const accessToken = isOfficerSession ? "" : (current ? applicantAccessToken : response.data.data.accessToken);
      if (!isOfficerSession && accessToken) setApplicantAccessToken(accessToken);
      if (submit)
        saved = (await client.post(
          isOfficerSession ? `/demand-applications/${saved.id}/submit` : `/demand-applications/public/${saved.id}/submit`,
          null,
          isOfficerSession ? undefined : { headers: { "X-Demand-Application-Token": accessToken } },
        ))
          .data.data;
      setCurrent(saved);
      if (submit) {
        setSubmissionSuccess({
          applicationNumber: saved.applicationNumber,
          submittedAt: saved.submittedAt,
        });
        if (!isOfficerSession) storeTemporaryApplication(saved, accessToken);
      } else {
        setMessage("अर्जाचा मसुदा जतन झाला आहे.");
      }
      await load();
    } catch (error) {
      setMessage(
        error.response?.data?.messageMr ||
          error.response?.data?.errors?.join(" ") ||
          "अर्ज जतन करता आला नाही.",
      );
    } finally {
      setSaving(false);
      setSubmitting(false);
      saveInFlight.current = false;
    }
  };
  const load = async () => {
    if (!user) {
      setApplications([]);
      setWorkflows({});
      return;
    }
    // This is an applicant-only list. Officer sessions must not request or
    // render applicant application cards on this page.
    setApplications([]);
    setWorkflows({});
  };
  useEffect(() => {
    load();
  }, [user]);
  useEffect(() => {
    if (!hydrated) return undefined;
    if (isOfficerSession) {
      setTemporaryApplication(null);
      return undefined;
    }

    const item = readTemporaryApplication();
    setTemporaryApplication(item);
    setApplicantAccessToken(item?.accessToken || "");
    if (!item) return undefined;

    const remaining = item.submittedAt + temporaryApplicationLifetimeMs - Date.now();
    const timer = window.setTimeout(clearTemporaryApplication, Math.max(0, remaining));
    return () => window.clearTimeout(timer);
  }, [hydrated, isOfficerSession]);
  const startNew = () => {
    setForm(initialForm);
    setCurrent(null);
    setApplicantAccessToken("");
    setErrors({});
    setMessage("");
    setStep(1);
  };
  const createDraftForDocumentUpload = async () => {
    if (current) return { application: current, accessToken: applicantAccessToken };
    if (!validate()) throw new Error("Complete required fields before uploading a document.");

    const { location, availableSpace, feeAmount, ...application } = form;
    const payload = {
      ...application,
      state: "Maharashtra",
      district: "Solapur",
      city: "Solapur",
      serviceType: Number(form.serviceType),
      businessType: form.businessType ? Number(form.businessType) : null,
      feeAmount: feeAmount === "" ? null : Number(feeAmount),
    };
    const response = await client.post(
      isOfficerSession ? "/demand-applications" : "/demand-applications/public",
      payload,
    );
    const saved = isOfficerSession ? response.data.data : response.data.data.application;
    const accessToken = isOfficerSession ? "" : response.data.data.accessToken;
    if (!isOfficerSession) setApplicantAccessToken(accessToken);
    setCurrent(saved);
    return { application: saved, accessToken };
  };
  const edit = (item) => {
    const { location, availableSpace, ...editableItem } = item;
    setForm({
      ...initialForm,
      ...editableItem,
      ...fixedAddressValues,
      serviceType: String(item.serviceType),
      businessType: item.businessType ? String(item.businessType) : "",
      feeAmount: item.feeAmount ?? "",
      requiredDuration: getDuration(item.startDate, item.endDate),
    });
    setCurrent(item);
    setErrors({});
    setMessage("");
    setStep(1);
  };
  const upload = async (event) => {
    const input = event.currentTarget;
    const file = input.files?.[0];
    if (!file || uploadInFlight.current) return;
    const extension = file.name.split(".").pop()?.toLowerCase();
    if (extension !== "pdf" || (file.type && file.type !== "application/pdf")) {
      setMessage("फक्त PDF स्वरूपातील कागदपत्र अपलोड करा.");
      input.value = "";
      return;
    }
    if (file.size > 20 * 1024 * 1024) {
      setMessage("कागदपत्राचा आकार 20 MB पेक्षा जास्त असू शकत नाही.");
      input.value = "";
      return;
    }

    uploadInFlight.current = true;
    setUploading(true);
    setMessage(`फाईल निवडली: ${file.name}`);
    const data = new FormData();
    data.append("documentType", input.dataset.type || "इतर कागदपत्र");
    data.append("file", file);
    try {
      const draft = await createDraftForDocumentUpload();
      const response = await client.post(
        isOfficerSession ? `/demand-applications/${draft.application.id}/documents` : `/demand-applications/public/${draft.application.id}/documents`,
        data,
        isOfficerSession ? undefined : { headers: { "X-Demand-Application-Token": draft.accessToken } },
      );
      setCurrent((previous) => ({
        ...previous,
        documents: [
          ...(previous.documents || []).filter(
            (item) => item.documentType !== response.data.data.documentType,
          ),
          response.data.data,
        ],
      }));
      setMessage("कागदपत्र यशस्वीरित्या अपलोड झाले.");
    } catch (error) {
      setMessage(
        error.response?.data?.messageMr || error.message || "कागदपत्र अपलोड करता आले नाही.",
      );
    } finally {
      uploadInFlight.current = false;
      setUploading(false);
      input.value = "";
    }
  };
  const downloadDocument = async (fileDoc) => {
    const response = await client.get(
      isOfficerSession ? `/demand-applications/documents/${fileDoc.id}/download` : `/demand-applications/public/${current.id}/documents/${fileDoc.id}/download`,
      isOfficerSession ? { responseType: "blob" } : { responseType: "blob", headers: { "X-Demand-Application-Token": applicantAccessToken } },
    );
    const url = window.URL.createObjectURL(response.data);
    const link = document.createElement("a");
    link.href = url;
    link.download = fileDoc.fileName;
    link.click();
    window.URL.revokeObjectURL(url);
  };
  const downloadApplication = async (application, accessToken = applicantAccessToken) => {
    const response = await client.get(
      isOfficerSession
        ? `/demand-workflow/${application.id}/application-pdf`
        : `/demand-applications/public/${application.id}/application-pdf`,
      isOfficerSession
        ? { responseType: "blob" }
        : { responseType: "blob", headers: { "X-Demand-Application-Token": accessToken } },
    );
    const downloadUrl = window.URL.createObjectURL(response.data);
    const link = document.createElement("a");
    link.href = downloadUrl;
    link.download = `Demand-Application-${application.applicationNumber || application.id}.pdf`;
    document.body.appendChild(link);
    link.click();
    link.remove();
    window.setTimeout(() => window.URL.revokeObjectURL(downloadUrl), 1000);
    return;
    const html2pdf = (await import("html2pdf.js")).default;
    const serviceLabel =
      serviceTypes.find((item) => item.value === Number(application.serviceType))
        ?.label || application.serviceType;
    const businessLabel =
      businessTypes.find((item) => item.value === Number(application.businessType))
        ?.label || application.businessType;
    const statusLabel = statusLabels[application.status] || application.status || "-";
    const value = (field) => application[field] || "-";
    const display = (field) => Object.prototype.hasOwnProperty.call(application, field) ? value(field) : field || "-";
    const sectionHeadings = { "सेवेची माहिती": "मागणीची माहिती", "पत्ता / सेवा माहिती": "अर्जदाराची माहिती", "कालावधी / सुविधा / आवश्यकता": "मागणीची माहिती", "अर्जाची स्थिती": "मागणीची माहिती" };
    const section = (title, rows) => `<section class="pdf-section"><h2>${sectionHeadings[title] || title}</h2><div class="pdf-grid">${rows.map(([label, field]) => `<div class="pdf-label">${label}</div><div class="pdf-value">${display(field)}</div>`).join("")}</div></section>`;
    const documents = application.documents?.length
      ? application.documents.map((item) => `<li>${item.documentType || "कागदपत्र"}: ${item.fileName || "-"}</li>`).join("")
      : "<li>-</li>";
    const node = document.createElement("div");
    // html2canvas does not capture a print surface outside the viewport. Keep
    // it on-screen only while the canvas is rendered, then remove it.
    node.style.cssText = "position:fixed;left:0;top:0;width:794px;background:#fff;z-index:2147483647;pointer-events:none";
    node.innerHTML = `<style>.pdf-section{margin-top:16px;break-inside:avoid}.pdf-section h2{margin:0 0 7px;padding:6px 9px;background:#eaf3fb;border-left:3px solid #0b3d91;color:#0b3d91;font-size:13px}.pdf-grid{display:grid;grid-template-columns:35% 65%;border:1px solid #cbd5e1}.pdf-label,.pdf-value{padding:6px 8px;border-bottom:1px solid #e2e8f0}.pdf-label{font-weight:700;background:#f8fafc;border-right:1px solid #e2e8f0}.pdf-section li{margin:3px 0}</style><div style="font-family:'Noto Sans Devanagari','Segoe UI',sans-serif;color:#1e293b;padding:34px 42px;font-size:12px;line-height:1.55"><header style="text-align:center;border-bottom:3px solid #0b3d91;padding-bottom:14px;margin-bottom:18px"><div style="font-size:21px;font-weight:700;color:#0b3d91">सोलापूर महानगरपालिका</div><div style="font-size:14px;color:#475569">भूमी व मालमत्ता व्यवस्थापन विभाग</div><div style="font-size:19px;font-weight:700;color:#0b3d91;margin-top:12px">मागणी अर्ज</div><div style="display:inline-block;margin-top:10px;padding:7px 16px;border:1px solid #0b3d91;font-weight:700;color:#0b3d91">अर्ज क्रमांक: ${value("applicationNumber")}</div></header>${section("सेवेची माहिती", [["सेवेचा प्रकार", serviceLabel], ["व्यवसायाचा प्रकार", businessLabel], ["इतर व्यवसायाचा प्रकार", "otherBusinessType"]])}${section("अर्जदाराची माहिती", [["अर्ज दिनांक", application.createdAt ? new Date(application.createdAt).toLocaleDateString("mr-IN") : "-"], ["अर्जदाराचा प्रकार", value("applicantType")], ["अर्जदाराचे पूर्ण नाव", "applicantName"], ["मोबाईल क्रमांक", "mobile"], ["ई-मेल आयडी", "email"], ["आधार/ओळखपत्र क्रमांक", "identityNumber"], ["PAN क्रमांक", "panNumber"], ["GST क्रमांक", "gstNumber"]])}${section("पत्ता / सेवा माहिती", [["कायमचा पत्ता", "permanentAddress"], ["पत्रव्यवहाराचा पत्ता", "correspondenceAddress"], ["राज्य", "state"], ["जिल्हा", "district"], ["शहर", "city"], ["तालुका", "taluka"], ["पिनकोड", "pinCode"], ["प्रभाग", "prabhag"]])}${section("कालावधी / सुविधा / आवश्यकता", [["सेवा/विक्रीचा प्रकार", "serviceDescription"], ["स्टॉल/जागेची आवश्यकता (क्षेत्रफळामध्ये)", "spaceRequirement"], ["इतर आवश्यक माहिती", "otherInformation"], ["प्रारंभ तारीख", "startDate"], ["समाप्ती तारीख", "endDate"], ["आवश्यक कालावधी (दिवस)", "requiredDuration"], ["वीज सुविधा", application.electricityRequired ? "होय" : "नाही"], ["पाणी सुविधा", application.waterRequired ? "होय" : "नाही"], ["इतर आवश्यक सुविधा", "otherFacilities"], ["कचरा व्यवस्थापन / संबंधित आवश्यकता", "wasteManagement"]])}<section class="pdf-section"><h2>कागदपत्रे</h2><ul>${documents}</ul></section><section class="pdf-section"><h2>घोषणा</h2><p style="border:1px solid #cbd5e1;padding:10px">मी दिलेली माहिती खरी असून नियम व अटी मान्य आहेत. घोषणा स्वीकारली: <strong>${application.declarationAccepted ? "होय" : "नाही"}</strong></p></section>${section("अर्जाची स्थिती", [["स्थिती", statusLabel], ["अर्ज क्रमांक", "applicationNumber"]])}<section class="pdf-section"><h2>कार्यालयीन वापरासाठी</h2><div style="height:48px;border:1px solid #cbd5e1;padding:8px">नोंद / शेरा: ________________________________________________</div></section><div style="display:flex;justify-content:space-between;margin-top:34px"><span>अर्जदाराची सही: ____________________</span><span>दिनांक: ____________________</span></div><footer style="margin-top:24px;border-top:1px solid #cbd5e1;padding-top:10px;color:#64748b;font-size:10px">हा दस्तऐवज मागणी अर्ज प्रणालीतून तयार करण्यात आला आहे.</footer></div>`;
    node.querySelector("footer")?.remove();
    document.body.appendChild(node);
    try {
      const pdfBlob = await html2pdf().set({
        margin: 0,
        image: { type: "jpeg", quality: 0.98 },
        html2canvas: { scale: 2, useCORS: true, backgroundColor: "#ffffff" },
        jsPDF: { unit: "pt", format: "a4", orientation: "portrait" },
      }).from(node).outputPdf("blob");
      const downloadUrl = window.URL.createObjectURL(pdfBlob);
      const link = document.createElement("a");
      link.href = downloadUrl;
      link.download = `Demand-Application-${application.applicationNumber || application.id}.pdf`;
      document.body.appendChild(link);
      link.click();
      link.remove();
      window.setTimeout(() => window.URL.revokeObjectURL(downloadUrl), 1000);
    } catch (error) {
      setMessage("अर्जाची PDF प्रत तयार करता आली नाही.");
      throw error;
    } finally {
      node.remove();
    }
  };
  const downloadTemporaryApplication = async () => {
    if (!temporaryApplication) return;
    try {
      const application = current?.id === temporaryApplication.id
        ? current
        : (await client.get(`/demand-applications/public/${temporaryApplication.id}`, { headers: { "X-Demand-Application-Token": temporaryApplication.accessToken } })).data.data;
      await downloadApplication(application, temporaryApplication.accessToken);
      clearTemporaryApplication();
    } catch (error) {
      setMessage(
        error.response?.data?.messageMr ||
          "अर्जाची PDF प्रत तयार करता आली नाही.",
      );
    }
  };
  const deleteDocument = async (fileDoc) => {
    if (!confirm("हा दस्तऐवज हटवायचा आहे का?")) return;
    await client.delete(
      isOfficerSession ? `/demand-applications/${current.id}/documents/${fileDoc.id}` : `/demand-applications/public/${current.id}/documents/${fileDoc.id}`,
      isOfficerSession ? undefined : { headers: { "X-Demand-Application-Token": applicantAccessToken } },
    );
    setCurrent((previous) => ({
      ...previous,
      documents: previous.documents.filter((item) => item.id !== fileDoc.id),
    }));
    setMessage("कागदपत्र हटवले आहे.");
  };
  const deleteApplication = async (item) => {
    if (!confirm("हा अर्ज हटवायचा आहे का?")) return;
    await client.delete(`/demand-applications/${item.id}`);
    if (current?.id === item.id) startNew();
    await load();
    setMessage("अर्ज हटवला आहे.");
  };
  const downloadCertificate = async (application) => {
    const html2pdf = (await import("html2pdf.js")).default;
    const node = document.createElement("div");
    node.style.cssText = "position:fixed;left:-10000px;top:0;width:794px;background:#fff";
    node.innerHTML = `<div style="font-family:'Noto Sans Devanagari','Segoe UI',sans-serif;text-align:center;padding:80px 55px;color:#0b3d91"><h1>सोलापूर महानगरपालिका</h1><h2>मागणी अर्ज मंजुरी प्रमाणपत्र</h2><hr/><p style="margin-top:45px;font-size:18px;color:#1e293b">अर्ज क्रमांक: <b>${application.applicationNumber}</b></p><p style="font-size:18px;color:#1e293b">अर्जदार: <b>${application.applicantName}</b></p><p style="margin-top:45px;font-size:16px;color:#1e293b">सदर मागणी अर्ज मंजूर करण्यात आला आहे.</p><p style="margin-top:70px;color:#475569">दिनांक: ${new Date().toLocaleDateString("mr-IN")}</p></div>`;
    document.body.appendChild(node);
    await html2pdf().set({ margin: 0, html2canvas: { scale: 2 }, jsPDF: { unit: "pt", format: "a4" } }).from(node).save(`Certificate-${application.applicationNumber}.pdf`);
    node.remove();
  };

  const input = (
    name,
    label,
    type = "text",
    required = false,
    wide = false,
  ) => {
    if (name === "requiredTime") return null;
    if (name === "serviceDescription") return select(name, label, serviceDescriptionOptions, required);
    const isDate = name === "startDate" || name === "endDate";
    const isDuration = name === "requiredDuration";
    const isFixedAddress = Object.prototype.hasOwnProperty.call(fixedAddressValues, name);
    return (
      <label className={wide ? "demand-field wide" : "demand-field"}>
        {isDuration ? "आवश्यक कालावधी (दिवस)" : label}
        {required && " *"}
        <input
          className={errors[name] ? "invalid" : ""}
          type={isDuration ? "text" : type}
          placeholder={name === "spaceRequirement" ? "उदा. 25 sq.ft. किंवा 10 × 50" : undefined}
          value={form[name] ?? ""}
          readOnly={isDuration || isFixedAddress}
          onChange={
            isFixedAddress
              ? undefined
              : isDate
              ? (event) => setDate(name, event.target.value)
              : (event) => set(name, event.target.value)
          }
        />
        {errors[name] && <small>{errors[name]}</small>}
      </label>
    );
  };
  const select = (name, label, options, required = false) => (
    <label className="demand-field">
      {label}
      {required && " *"}
      <select
        className={errors[name] ? "invalid" : ""}
        value={form[name] ?? ""}
        onChange={(event) => set(name, event.target.value)}
      >
        <option value="">-- निवडा --</option>
        {options.map((option) => (
          <option key={option.value} value={option.value}>
            {option.label}
          </option>
        ))}
      </select>
      {errors[name] && <small>{errors[name]}</small>}
    </label>
  );

  return (
    <div className="demand-page">
      <div className="page-header">
        <div>
          <div className="page-title">मागणी अर्ज</div>
          <div className="page-subtitle">
            सेवा, जागा आणि अर्जदाराची माहिती एकाच अर्जातून सादर करा
          </div>
        </div>
      </div>
      <div className="demand-layout">
        <section className="card demand-card">
          <div className="demand-steps">
            {[
              "सेवेची माहिती",
              "अर्जदाराची माहिती",
              "पत्ता व जागा",
              "कागदपत्रे व सादरीकरण",
            ].map((label, index) => (
              <button
                key={label}
                className={step === index + 1 ? "active" : ""}
                onClick={() => goToStep(index + 1)}
              >
                <b>{index + 1}</b>
                {label}
              </button>
            ))}
          </div>
          {message && (
            <div
              className={
                message.includes("यशस्वी") || message.includes("जतन")
                  ? "success-msg"
                  : "error-msg"
              }
            >
              {message}
            </div>
          )}
          {step === 1 && (
            <div>
              <h2>सेवेची माहिती</h2>
              <div className="demand-grid">
                <label className="demand-field">
                  सेवेचा प्रकार *
                  <select
                    className={errors.serviceType ? "invalid" : ""}
                    value={form.serviceType}
                    onChange={(event) => setService(event.target.value)}
                  >
                    <option value="">-- निवडा --</option>
                    {serviceTypes.map((option) => (
                      <option key={option.value} value={option.value}>
                        {option.label}
                      </option>
                    ))}
                  </select>
                  {errors.serviceType && <small>{errors.serviceType}</small>}
                </label>
                {Number(form.serviceType) === 3 && (
                  <>
                    <label className="demand-field">
                      व्यवसायाचा प्रकार *
                      <select
                        className={errors.businessType ? "invalid" : ""}
                        value={form.businessType}
                        onChange={(event) => setBusiness(event.target.value)}
                      >
                        <option value="">-- निवडा --</option>
                        {businessTypes.map((option) => (
                          <option key={option.value} value={option.value}>
                            {option.label}
                          </option>
                        ))}
                      </select>
                      {errors.businessType && (
                        <small>{errors.businessType}</small>
                      )}
                    </label>
                    {Number(form.businessType) === 6 &&
                      input(
                        "otherBusinessType",
                        "इतर व्यवसायाचा प्रकार",
                        "text",
                        true,
                      )}
                  </>
                )}
              </div>
            </div>
          )}
          {step === 2 && (
            <div>
              <h2>अर्जदाराची माहिती</h2>
              <div className="demand-grid">
                {select("applicantType", "अर्जदाराचा प्रकार", [
                  { value: "Individual", label: "वैयक्तिक" },
                  { value: "Organization", label: "संस्था" },
                ])}
                {input("applicantName", "अर्जदाराचे पूर्ण नाव", "text", true)}
                {input("mobile", "मोबाईल क्रमांक", "tel", true)}
                {input("email", "ई-मेल आयडी")}
                {input("identityNumber", "आधार/ओळखपत्र क्रमांक")}
                {input("panNumber", "PAN क्रमांक")}
                {input("gstNumber", "GST क्रमांक")}
              </div>
            </div>
          )}
          {step === 3 && (
            <div>
              <h2>पत्ता / सेवा / ठिकाणाची माहिती</h2>
              <div className="demand-grid">
                {input("permanentAddress", "कायमचा पत्ता", "text", true, true)}
                <label className="demand-check wide">
                  <input
                    type="checkbox"
                    checked={form.sameAddress}
                    onChange={(event) => setSameAddress(event.target.checked)}
                  />{" "}
                  कायमचा व पत्रव्यवहाराचा पत्ता समान
                </label>
                {input(
                  "correspondenceAddress",
                  "पत्रव्यवहाराचा पत्ता",
                  "text",
                  true,
                  true,
                )}
                {input("state", "राज्य", "text", true)}
                {input("district", "जिल्हा", "text", true)}
                {input("city", "शहर", "text", true)}
                {select("taluka", "तालुका", talukas, true)}
                {input("pinCode", "पिनकोड", "tel", true)}
                {select("prabhag", "प्रभाग", prabhags, true)}
                {input("lengthFt", "Length (ft)", "number", true)}
                {input("widthFt", "Width (ft)", "number", true)}
                <label className="demand-field"><span>क्षेत्रफळ (sq ft)</span><input className="form-input" value={form.areaSqFt} readOnly /></label>
                <label className="demand-field"><span>Rate (₹)</span><input className="form-input" value={form.calculatedRate} readOnly /></label>
                {input(
                  "serviceDescription",
                  "सेवा/विक्रीचा प्रकार",
                  "text",
                  true,
                )}
                {input(
                  "spaceRequirement",
                  "स्टॉल/जागेची आवश्यकता (क्षेत्रफळामध्ये)",
                  "text",
                  true,
                )}
                {input(
                  "otherInformation",
                  "इतर आवश्यक माहिती",
                  "text",
                  false,
                  true,
                )}
              </div>
            </div>
          )}
          {step === 4 && (
            <div>
              <h2>कालावधी / सुविधा / कागदपत्रे / घोषणा</h2>
              <div className="demand-grid">
                {input("startDate", "प्रारंभ तारीख", "date", true)}
                {input("endDate", "समाप्ती तारीख", "date", true)}
                {input("requiredDuration", "आवश्यक कालावधी", "text", true)}
                {input("requiredTime", "वेळ")}
                {
                  <label className="demand-check">
                    <input
                      type="checkbox"
                      checked={form.electricityRequired}
                      onChange={(event) =>
                        set("electricityRequired", event.target.checked)
                      }
                    />{" "}
                    वीज सुविधा
                  </label>
                }
                {
                  <label className="demand-check">
                    <input
                      type="checkbox"
                      checked={form.waterRequired}
                      onChange={(event) =>
                        set("waterRequired", event.target.checked)
                      }
                    />{" "}
                    पाणी सुविधा
                  </label>
                }
                {input(
                  "otherFacilities",
                  "इतर आवश्यक सुविधा",
                  "text",
                  false,
                  true,
                )}
                {input(
                  "wasteManagement",
                  "कचरा व्यवस्थापन / संबंधित आवश्यकता",
                  "text",
                  false,
                  true,
                )}
                <div className="demand-review wide">
                  <h3>अर्जाचा आढावा</h3>
                  <span>
                    सेवा:{" "}
                    {serviceTypes.find(
                      (item) => item.value === Number(form.serviceType),
                    )?.label || "-"}
                  </span>
                  <span>अर्जदार: {form.applicantName || "-"}</span>
                  <span>मोबाईल: {form.mobile || "-"}</span>
                  <span>
                    प्रभाग: {form.prabhag || "-"}
                  </span>
                  <span>
                    कालावधी: {form.startDate || "-"} ते {form.endDate || "-"}
                  </span>
                </div>
                <div className="demand-documents wide">
                  <h3>कागदपत्रे</h3>
                  <p>फक्त PDF, कमाल 20 MB</p>
                  <div className="demand-upload-row">
                    {[
                      "ओळखपत्र",
                      "पत्त्याचा पुरावा",
                      "PAN / GST",
                      "व्यवसाय कागदपत्र",
                    ].map((type) => (
                      <label key={type} className="upload-box">
                        {type}
                        <input
                          type="file"
                          data-type={type}
                          onChange={upload}
                          disabled={saving || uploading}
                          accept=".pdf,application/pdf"
                        />
                      </label>
                    ))}
                  </div>
                  {current?.documents?.map((fileDoc) => (
                    <div key={fileDoc.id} className="demand-document">
                      <span>
                        {fileDoc.documentType}: {fileDoc.fileName}
                      </span>
                      <span>
                        <button
                          className="btn btn-outline btn-sm"
                          onClick={() => downloadDocument(fileDoc)}
                        >
                          पाहा
                        </button>
                        <button
                          className="btn btn-danger btn-sm"
                          onClick={() => deleteDocument(fileDoc)}
                        >
                          हटवा
                        </button>
                      </span>
                    </div>
                  ))}
                </div>
                <label
                  className={`demand-check wide ${errors.declarationAccepted ? "invalid-check" : ""}`}
                >
                  <input
                    type="checkbox"
                    checked={form.declarationAccepted}
                    onChange={(event) =>
                      set("declarationAccepted", event.target.checked)
                    }
                  />{" "}
                  मी दिलेली माहिती खरी असून नियम व अटी मान्य आहेत. अंतिम
                  सादरीकरणासाठी ही घोषणा स्वीकारतो/स्वीकारते.
                  {errors.declarationAccepted && (
                    <small>{errors.declarationAccepted}</small>
                  )}
                </label>
                <div className="demand-fee wide">
                  <b>शुल्क सारांश</b>
                  <span>
                    शुल्क दर भविष्यातील प्रशासकीय configuration नुसार लागू केला
                    जाईल.
                  </span>
                  <strong>
                    {form.feeAmount
                      ? `₹${form.feeAmount}`
                      : "शुल्क उपलब्ध नाही"}
                  </strong>
                </div>
              </div>
            </div>
          )}
          <div className="demand-footer">
            <button
              className="btn btn-outline"
              onClick={previousStep}
              disabled={step === 1}
            >
              Back
            </button>
            {step < 4 ? (
              <button className="btn btn-primary" onClick={nextStep}>
                Next
              </button>
            ) : (
              <>
                <button
                  className="btn btn-outline"
                  onClick={() => save(false)}
                  disabled={saving || uploading}
                >
                  Save Draft
                </button>
                <button
                  className="btn btn-primary"
                  onClick={() => save(true)}
                  disabled={saving || uploading}
                >
                  {submitting ? "सादर करत आहे..." : "Final Submit"}
                </button>
              </>
            )}
          </div>
        </section>
        {!isOfficerSession && temporaryApplication && (
          <aside className="card demand-list">
            <div className="demand-list-head">
              <h2>माझे अर्ज</h2>
            </div>
            <div className="demand-list-item">
              <div>
                <b>{temporaryApplication.applicationNumber}</b>
                <small>सादर केलेला अर्ज</small>
              </div>
              <button
                className="btn btn-outline btn-sm"
                onClick={downloadTemporaryApplication}
              >
                <DownloadIcon /> Download
              </button>
            </div>
          </aside>
        )}
      </div>
      {submissionSuccess && (
        <Modal
          title="अर्ज यशस्वीरीत्या सादर झाला आहे."
          onClose={() => setSubmissionSuccess(null)}
          footer={<button className="btn btn-primary" onClick={() => setSubmissionSuccess(null)}>ठीक आहे</button>}
        >
          <p><strong>आपला अर्ज क्रमांक:</strong> {submissionSuccess.applicationNumber}</p>
          {submissionSuccess.submittedAt && (
            <p><strong>सादर केल्याची तारीख व वेळ:</strong> {new Date(submissionSuccess.submittedAt).toLocaleString("mr-IN", { dateStyle: "medium", timeStyle: "short" })}</p>
          )}
        </Modal>
      )}
    </div>
  );
}
