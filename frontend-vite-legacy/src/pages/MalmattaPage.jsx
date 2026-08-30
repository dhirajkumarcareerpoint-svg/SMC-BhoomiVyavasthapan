import EntityCrudPage from '../components/EntityCrudPage'
import StatusBadge from '../components/StatusBadge'
import { PropertyCategory, PropertyStatus, PropertyZones, PropertyWards } from '../config/labels'

export default function MalmattaPage() {
  return (
    <EntityCrudPage
      title="मालमत्ता"
      subtitle="Major/Mini गाळे, Land Fee, समाज मंदिर, अभ्यासिका, 256 गाळे, TP-3/23, अधिकृत खोके, इतर मनपा मालमत्ता"
      apiPath="/properties"
      autoGenerateCode={{ field: 'propertyCode', categoryField: 'category', endpoint: '/properties/next-code' }}
      docEntityType="Property"
      auditEntityName="Property"
      defaultForm={{
        category: 'MajorGaale', propertyCode: '', name: '', ward: '', zone: '', address: '',
        areaSqFt: '', monthlyRent: 0, annualDemand: 0, surveyNumber: '', tpNumber: '',
        status: 'Rikamy', currentOccupant: '', shera: ''
      }}
      filterFields={[
        { name: 'category', label: 'मालमत्ता प्रकार', options: PropertyCategory },
        { name: 'status', label: 'स्थिती', options: PropertyStatus }
      ]}
      columns={[
        { key: 'propertyCode', label: 'मालमत्ता क्र.' },
        { key: 'name', label: 'नाव' },
        { key: 'category', label: 'मालमत्ता प्रकार', render: (i) => PropertyCategory[i.category] || i.category },
        { key: 'ward', label: 'प्रभाग' },
        { key: 'annualDemand', label: 'वार्षिक मागणी', render: (i) => `₹${Number(i.annualDemand).toLocaleString('en-IN')}` },
        { key: 'status', label: 'स्थिती', render: (i) => <StatusBadge status={i.status} label={PropertyStatus[i.status]} /> }
      ]}
      formFields={[
        { name: 'category', label: 'मालमत्ता प्रकार', type: 'select', options: PropertyCategory, required: true },
        { name: 'propertyCode', label: 'मालमत्ता क्रमांक', required: true },
        { name: 'name', label: 'मालमत्तेचे नाव', required: true },
        { name: 'ward', label: 'प्रभाग', type: 'select', options: PropertyWards },
        { name: 'zone', label: 'झोन', type: 'select', options: PropertyZones },
        { name: 'address', label: 'पत्ता', full: true },
        { name: 'areaSqFt', label: 'क्षेत्रफळ (चौ.फूट)', type: 'number' },
        { name: 'monthlyRent', label: 'मासिक भाडे (₹)', type: 'number' },
        { name: 'annualDemand', label: 'वार्षिक मागणी (₹)', type: 'number' },
        { name: 'surveyNumber', label: 'सर्वे/गट क्रमांक' },
        { name: 'tpNumber', label: 'TP क्रमांक' },
        { name: 'status', label: 'स्थिती', type: 'select', options: PropertyStatus, required: true },
        { name: 'currentOccupant', label: 'सध्याचा धारक/भाडेकरू' },
        { name: 'shera', label: 'शेरा', type: 'textarea', full: true }
      ]}
    />
  )
}
