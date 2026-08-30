import EntityCrudPage from '../components/EntityCrudPage'
import StatusBadge from '../components/StatusBadge'
import { LeaseDurationType, LeaseStatus } from '../config/labels'

export default function HastantaranPage() {
  return (
    <EntityCrudPage
      title="हस्तांतरण"
      subtitle="दस्ताद्वारे भाडेपट्टा — किमान 3 वर्षे / 3 ते 10 वर्षे / कमाल 29 वर्षे 11 महिने"
      apiPath="/leases"
      docEntityType="Lease"
      auditEntityName="Lease"
      defaultForm={{
        propertyId: '', lesseeName: '', lesseeMobile: '', lesseeAddress: '', deedNumber: '',
        deedDate: '', durationType: 'Min3Years', startDate: '', endDate: '', rentAmount: 0,
        securityDeposit: '', status: 'Saru', shera: ''
      }}
      filterFields={[
        { name: 'status', label: 'स्थिती', options: LeaseStatus },
        { name: 'durationType', label: 'कालावधी', options: LeaseDurationType }
      ]}
      columns={[
        { key: 'deedNumber', label: 'दस्त क्र.' },
        { key: 'propertyName', label: 'मालमत्ता' },
        { key: 'lesseeName', label: 'भाडेकरू' },
        { key: 'durationType', label: 'कालावधी', render: (i) => LeaseDurationType[i.durationType] || i.durationType },
        { key: 'rentAmount', label: 'भाडे रक्कम', render: (i) => `₹${Number(i.rentAmount).toLocaleString('en-IN')}` },
        { key: 'status', label: 'स्थिती', render: (i) => <StatusBadge status={i.status} label={LeaseStatus[i.status]} /> }
      ]}
      formFields={[
        { name: 'propertyId', label: 'मालमत्ता क्रमांक', type: 'propertyPicker', required: true, full: true },
        { name: 'lesseeName', label: 'भाडेकरू/धारकाचे नाव', required: true },
        { name: 'lesseeMobile', label: 'भ्रमणध्वनी क्रमांक' },
        { name: 'lesseeAddress', label: 'पत्ता', full: true },
        { name: 'deedNumber', label: 'दस्त क्रमांक', required: true },
        { name: 'deedDate', label: 'दस्त नोंदणी तारीख', type: 'date', required: true },
        { name: 'durationType', label: 'कालावधी प्रकार', type: 'select', options: LeaseDurationType, required: true },
        { name: 'startDate', label: 'सुरुवात तारीख', type: 'date', required: true },
        { name: 'endDate', label: 'समाप्ती तारीख', type: 'date', required: true },
        { name: 'rentAmount', label: 'भाडे रक्कम (₹)', type: 'number' },
        { name: 'securityDeposit', label: 'अनामत रक्कम (₹)', type: 'number' },
        { name: 'status', label: 'स्थिती', type: 'select', options: LeaseStatus, required: true },
        { name: 'shera', label: 'शेरा', type: 'textarea', full: true }
      ]}
    />
  )
}
