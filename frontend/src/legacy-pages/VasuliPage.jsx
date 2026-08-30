import EntityCrudPage from '../components/EntityCrudPage'
import StatusBadge from '../components/StatusBadge'
import { RecoveryStage } from '../config/labels'

export default function VasuliPage() {
  return (
    <EntityCrudPage
      title="वसुली प्रक्रिया"
      subtitle="किमान 3 महिने भाडे थकीत → नोटीस → भाडे वसुली → न भरल्यास सील → पुनर्लिलाव"
      apiPath="/recoverycases"
      docEntityType="RecoveryCase"
      auditEntityName="RecoveryCase"
      defaultForm={{
        propertyId: '', leaseId: '', monthsOverdue: 3, outstandingAmount: 0, stage: 'ThakbakiOlkhli',
        noticeNumber: '', noticeDate: '', recoveredAmount: 0, recoveryDate: '', sealDate: '', reAuctionDate: '', shera: ''
      }}
      filterFields={[{ name: 'stage', label: 'टप्पा', options: RecoveryStage }]}
      columns={[
        { key: 'propertyName', label: 'मालमत्ता' },
        { key: 'monthsOverdue', label: 'थकीत महिने' },
        { key: 'outstandingAmount', label: 'थकबाकी', render: (i) => `₹${Number(i.outstandingAmount).toLocaleString('en-IN')}` },
        { key: 'recoveredAmount', label: 'वसूल रक्कम', render: (i) => `₹${Number(i.recoveredAmount).toLocaleString('en-IN')}` },
        { key: 'stage', label: 'टप्पा', render: (i) => <StatusBadge status={i.stage} label={RecoveryStage[i.stage]} /> }
      ]}
      formFields={[
        { name: 'propertyId', label: 'मालमत्ता क्रमांक', type: 'propertyPicker', required: true, full: true },
        { name: 'monthsOverdue', label: 'थकीत महिने (किमान 3)', type: 'number', required: true },
        { name: 'outstandingAmount', label: 'थकबाकी रक्कम (₹)', type: 'number', required: true },
        { name: 'stage', label: 'टप्पा', type: 'select', options: RecoveryStage, required: true },
        { name: 'noticeNumber', label: 'नोटीस क्रमांक' },
        { name: 'noticeDate', label: 'नोटीस तारीख', type: 'date' },
        { name: 'recoveredAmount', label: 'वसूल झालेली रक्कम (₹)', type: 'number' },
        { name: 'recoveryDate', label: 'वसुली तारीख', type: 'date' },
        { name: 'sealDate', label: 'सील तारीख', type: 'date' },
        { name: 'reAuctionDate', label: 'पुनर्लिलाव तारीख', type: 'date' },
        { name: 'shera', label: 'शेरा', type: 'textarea', full: true }
      ]}
    />
  )
}
