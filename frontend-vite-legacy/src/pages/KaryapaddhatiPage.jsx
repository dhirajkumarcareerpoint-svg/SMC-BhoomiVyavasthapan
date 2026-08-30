import EntityCrudPage from '../components/EntityCrudPage'
import StatusBadge from '../components/StatusBadge'
import { AllocationMethod, AllocationStatus } from '../config/labels'

export default function KaryapaddhatiPage() {
  return (
    <EntityCrudPage
      title="मालमत्ता देण्याची कार्यपद्धती"
      subtitle="सार्वजनिक लिलाव, निविदा मागविणे, प्रसिद्धीकरण करून अर्ज मागविणे — सर्वाधिक बोली लावणाऱ्या पात्र व्यक्तीला मालमत्ता वाटप"
      apiPath="/allocationprocesses"
      docEntityType="Allocation"
      auditEntityName="AllocationProcess"
      modalClassName="karyapaddhati-modal"
      defaultForm={{
        propertyId: '', method: 'SarvajanikLilaw', noticeNumber: '', publishDate: '', lastDateToApply: '',
        auctionDate: '', reserveAmount: '', highestBidAmount: '', highestBidderName: '', highestBidderMobile: '',
        status: 'JahirNamaPrasiddh', shera: ''
      }}
      filterFields={[
        { name: 'method', label: 'पद्धत', options: AllocationMethod },
        { name: 'status', label: 'स्थिती', options: AllocationStatus }
      ]}
      columns={[
        { key: 'method', label: 'पद्धत', render: (i) => AllocationMethod[i.method] || i.method },
        { key: 'propertyName', label: 'मालमत्ता' },
        { key: 'noticeNumber', label: 'जाहिरात/निविदा क्र.' },
        { key: 'highestBidderName', label: 'सर्वाधिक बोली लावणारा' },
        { key: 'status', label: 'स्थिती', render: (i) => <StatusBadge status={i.status} label={AllocationStatus[i.status]} /> }
      ]}
      formFields={[
        { name: 'propertyId', label: 'मालमत्ता क्रमांक', type: 'propertyPicker', required: true, full: true },
        { name: 'method', label: 'पद्धत', type: 'select', options: AllocationMethod, required: true },
        { name: 'noticeNumber', label: 'जाहिरात/निविदा क्रमांक' },
        { name: 'publishDate', label: 'प्रसिद्धी तारीख', type: 'date', required: true },
        { name: 'lastDateToApply', label: 'अर्जाची अंतिम तारीख', type: 'date' },
        { name: 'auctionDate', label: 'लिलाव/निविदा तारीख', type: 'date' },
        { name: 'reserveAmount', label: 'राखीव किंमत (₹)', type: 'number' },
        { name: 'highestBidAmount', label: 'सर्वाधिक बोली रक्कम (₹)', type: 'number' },
        { name: 'highestBidderName', label: 'सर्वाधिक बोली लावणाऱ्याचे नाव' },
        { name: 'highestBidderMobile', label: 'भ्रमणध्वनी क्रमांक' },
        { name: 'status', label: 'स्थिती', type: 'select', options: AllocationStatus, required: true },
        { name: 'shera', label: 'शेरा', type: 'textarea', full: true }
      ]}
    />
  )
}
