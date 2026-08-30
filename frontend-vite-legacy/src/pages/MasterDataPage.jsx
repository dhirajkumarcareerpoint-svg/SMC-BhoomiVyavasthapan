import {
  PropertyCategory, PropertyStatus, LeaseDurationType, LeaseStatus, RecoveryStage,
  SchemeType, SchemeStatus, AllocationMethod, AllocationStatus, CalculationStatus, UserRole
} from '../config/labels'

const groups = [
  { title: 'मालमत्ता प्रकार', icon: '🏢', data: PropertyCategory },
  { title: 'मालमत्ता स्थिती', icon: '🏷️', data: PropertyStatus },
  { title: 'हस्तांतरण कालावधी प्रकार', icon: '📜', data: LeaseDurationType },
  { title: 'हस्तांतरण स्थिती', icon: '📄', data: LeaseStatus },
  { title: 'वसुली प्रक्रिया टप्पे', icon: '💰', data: RecoveryStage },
  { title: 'विविध उपक्रम प्रकार', icon: '🎯', data: SchemeType },
  { title: 'विविध उपक्रम स्थिती', icon: '✅', data: SchemeStatus },
  { title: 'देण्याची कार्यपद्धती', icon: '🏛️', data: AllocationMethod },
  { title: 'कार्यपद्धती स्थिती', icon: '📋', data: AllocationStatus },
  { title: 'Calculation स्थिती', icon: '🧮', data: CalculationStatus },
  { title: 'वापरकर्ता भूमिका', icon: '👤', data: UserRole },
]

export default function MasterDataPage() {
  return (
    <div>
      <div className="page-header">
        <div>
          <div className="page-title">Master Data</div>
          <div className="page-subtitle">प्रणालीत सर्वत्र वापरले जाणारे संदर्भ/मास्टर यादी (केवळ पाहण्यासाठी) — मालमत्ता प्रकार, स्थिती, कालावधी व इतर वर्गीकरणे</div>
        </div>
      </div>
      <div className="stat-grid">
        {groups.map((g) => (
          <div key={g.title} className="card" style={{ padding: 18 }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: 8, marginBottom: 12 }}>
              <span style={{ fontSize: 20 }}>{g.icon}</span>
              <span style={{ fontWeight: 700, color: '#0b3d91', fontSize: 14 }}>{g.title}</span>
            </div>
            <div style={{ display: 'flex', flexDirection: 'column', gap: 6 }}>
              {Object.entries(g.data).map(([key, label]) => (
                <div key={key} style={{
                  display: 'flex', justifyContent: 'space-between', fontSize: 12.5,
                  padding: '6px 10px', background: '#f8fafc', borderRadius: 6, color: '#334155'
                }}>
                  <span>{label}</span>
                  <span style={{ color: '#94a3b8' }}>{key}</span>
                </div>
              ))}
            </div>
          </div>
        ))}
      </div>
    </div>
  )
}
