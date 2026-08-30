import EntityCrudPage from '../components/EntityCrudPage'
import StatusBadge from '../components/StatusBadge'
import { SchemeType, SchemeStatus } from '../config/labels'

export default function UpkramPage() {
  return (
    <EntityCrudPage
      title="विविध उपक्रम"
      subtitle="अभय योजना, दंडमाफी, सवलत, इतर महसूलवाढीचे उपक्रम"
      apiPath="/schemeapplications"
      docEntityType="Scheme"
      auditEntityName="SchemeApplication"
      defaultForm={{
        propertyId: '', schemeType: 'AbhayYojana', applicantName: '', applicantMobile: '',
        applicationDate: '', originalOutstanding: 0, waivedAmount: 0, payableAmount: 0,
        status: 'Prapt', decisionDate: '', approvedBy: '', shera: ''
      }}
      filterFields={[
        { name: 'schemeType', label: 'उपक्रम प्रकार', options: SchemeType },
        { name: 'status', label: 'स्थिती', options: SchemeStatus }
      ]}
      columns={[
        { key: 'schemeType', label: 'उपक्रम प्रकार', render: (i) => SchemeType[i.schemeType] || i.schemeType },
        { key: 'applicantName', label: 'अर्जदार' },
        { key: 'propertyName', label: 'मालमत्ता' },
        { key: 'waivedAmount', label: 'माफ रक्कम', render: (i) => `₹${Number(i.waivedAmount).toLocaleString('en-IN')}` },
        { key: 'status', label: 'स्थिती', render: (i) => <StatusBadge status={i.status} label={SchemeStatus[i.status]} /> }
      ]}
      formFields={[
        { name: 'propertyId', label: 'मालमत्ता क्रमांक', type: 'propertyPicker', required: true, full: true },
        { name: 'schemeType', label: 'उपक्रम प्रकार', type: 'select', options: SchemeType, required: true },
        { name: 'applicantName', label: 'अर्जदाराचे नाव', required: true },
        { name: 'applicantMobile', label: 'भ्रमणध्वनी क्रमांक' },
        { name: 'applicationDate', label: 'अर्ज तारीख', type: 'date' },
        { name: 'originalOutstanding', label: 'मूळ थकबाकी (₹)', type: 'number' },
        { name: 'waivedAmount', label: 'माफ केलेली रक्कम (₹)', type: 'number' },
        { name: 'payableAmount', label: 'भरावयाची रक्कम (₹)', type: 'number' },
        { name: 'status', label: 'स्थिती', type: 'select', options: SchemeStatus, required: true },
        { name: 'decisionDate', label: 'निर्णय तारीख', type: 'date' },
        { name: 'approvedBy', label: 'मंजूर करणारे अधिकारी' },
        { name: 'shera', label: 'शेरा', type: 'textarea', full: true }
      ]}
    />
  )
}
