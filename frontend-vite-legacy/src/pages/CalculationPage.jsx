import EntityCrudPage from '../components/EntityCrudPage'
import StatusBadge from '../components/StatusBadge'
import { CalculationStatus } from '../config/labels'

export default function CalculationPage() {
  return (
    <EntityCrudPage
      title="Calculation"
      subtitle="निवडलेल्या मालमत्तेसाठी भाडे/शुल्क आकारणीची नोंद — मालमत्ता क्रमांक निवडताच संबंधित माहिती स्वयंचलित आणली जाते"
      apiPath="/calculations"
      docEntityType="Calculation"
      auditEntityName="Calculation"
      defaultForm={{
        propertyId: '', rate: '', periodMonths: 1, previousOutstanding: 0, currentDemand: 0,
        calculatedAmount: 0, totalAmount: 0, calculationDate: '', status: 'Prarup', shera: ''
      }}
      filterFields={[{ name: 'status', label: 'स्थिती', options: CalculationStatus }]}
      columns={[
        { key: 'propertyCode', label: 'मालमत्ता क्र.' },
        { key: 'propertyName', label: 'मालमत्ता' },
        { key: 'periodMonths', label: 'कालावधी (महिने)' },
        { key: 'calculatedAmount', label: 'गणना रक्कम', render: (i) => `₹${Number(i.calculatedAmount).toLocaleString('en-IN')}` },
        { key: 'totalAmount', label: 'एकूण रक्कम', render: (i) => `₹${Number(i.totalAmount).toLocaleString('en-IN')}` },
        { key: 'status', label: 'स्थिती', render: (i) => <StatusBadge status={i.status} label={CalculationStatus[i.status]} /> }
      ]}
      formFields={[
        {
          name: 'propertyId', label: 'मालमत्ता क्रमांक', type: 'propertyPicker', required: true, full: true,
          // मालमत्ता निवडताच लागू दर (Rate) मालमत्तेच्या मासिक भाड्यावरून सुचवला जातो;
          // अधिकारी आवश्यकतेनुसार तो संपादित करू शकतात (केवळ सूचना, अंतिम रक्कम नाही).
          onPropertyLoaded: (property, setField, form) => {
            if (!form.rate) setField('rate', property.monthlyRent ?? '')
          }
        },
        { name: 'rate', label: 'लागू दर (₹) (सुचवलेला — आवश्यकतेनुसार बदला)', type: 'number' },
        { name: 'periodMonths', label: 'गणना कालावधी (महिने)', type: 'number', required: true },
        { name: 'previousOutstanding', label: 'मागील थकबाकी (₹)', type: 'number' },
        { name: 'currentDemand', label: 'चालू मागणी (₹)', type: 'number' },
        { name: 'calculatedAmount', label: 'गणना केलेली रक्कम (₹) (स्वहस्ते पडताळून भरा)', type: 'number', required: true },
        { name: 'totalAmount', label: 'एकूण रक्कम (₹) (स्वहस्ते पडताळून भरा)', type: 'number', required: true },
        { name: 'calculationDate', label: 'गणना तारीख', type: 'date', required: true },
        { name: 'status', label: 'स्थिती', type: 'select', options: CalculationStatus, required: true },
        { name: 'shera', label: 'शेरा', type: 'textarea', full: true }
      ]}
    />
  )
}
