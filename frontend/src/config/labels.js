// सर्व Enum मूल्यांचे मराठी लेबल्स (backend enum names -> मराठी मजकूर)

export const PropertyCategory = {
  MajorGaale: 'Major गाळे',
  MiniGaale: 'Mini गाळे',
  LandFee: 'Land Fee (भुई भाडे)',
  SamajMandir: 'समाज मंदिर',
  Abhyasika: 'अभ्यासिका',
  Gaale256: '256 गाळे',
  TP3_23: 'TP-3/23',
  AdhikrutKhoke: 'अधिकृत खोके',
  ItarBhadetatvavarilMalmatta: 'इतर भाडेतत्त्वावरील मनपा मालमत्ता'
}

export const PropertyStatus = {
  Rikamy: 'रिक्त',
  Bhadyane: 'भाडेतत्त्वावर दिलेली',
  Seal: 'सील केलेली',
  Punarlilaw: 'पुनर्लिलावासाठी',
  Nishkriya: 'निष्क्रिय'
}

export const PropertyZones = {
  Zone1: 'झोन १',
  Zone2: 'झोन २',
  Zone3: 'झोन ३',
  Zone4: 'झोन ४',
  Zone5: 'झोन ५',
  Zone6: 'झोन ६',
  Zone7: 'झोन ७',
  Zone8: 'झोन ८'
}

export const PropertyWards = Object.fromEntries(
  Array.from({ length: 26 }, (_, index) => [`Prabhag ${index + 1}`, `प्रभाग ${String(index + 1).replace(/[0-9]/g, (digit) => '०१२३४५६७८९'[digit])}`])
)

export const LeaseDurationType = {
  Min3Years: 'किमान 3 वर्षे',
  ThreeToTenYears: '3 ते 10 वर्षे',
  Max29Years11Months: 'कमाल 29 वर्षे 11 महिने'
}

export const LeaseStatus = {
  Saru: 'सुरू',
  Sampla: 'संपलेला',
  Radd: 'रद्द'
}

export const RecoveryStage = {
  ThakbakiOlkhli: 'थकबाकी ओळखली',
  NoticeDili: 'नोटीस दिली',
  VasuliSuru: 'वसुली सुरू',
  Seal: 'सील',
  Punarlilaw: 'पुनर्लिलाव',
  Band: 'प्रकरण बंद'
}

export const SchemeType = {
  AbhayYojana: 'अभय योजना',
  DandMafi: 'दंडमाफी',
  Savlat: 'सवलत',
  Itar: 'इतर महसूलवाढीचे उपक्रम'
}

export const SchemeStatus = {
  Prapt: 'अर्ज प्राप्त',
  ManjurZala: 'मंजूर',
  Naklat: 'नाकारले'
}

export const AllocationMethod = {
  SarvajanikLilaw: 'सार्वजनिक लिलाव',
  Niviva: 'निविदा मागविणे',
  PrasiddhikaranArj: 'प्रसिद्धीकरण करून अर्ज मागविणे'
}

export const AllocationStatus = {
  JahirNamaPrasiddh: 'जाहीरनामा प्रसिद्ध',
  ArjSwikarane: 'अर्ज स्वीकारणे सुरू',
  LilawZala: 'लिलाव/निविदा पूर्ण',
  Manjur: 'मंजूर',
  Radd: 'रद्द'
}

export const CalculationStatus = {
  Prarup: 'प्रारूप (Draft)',
  Nishchit: 'निश्चित केलेली',
  Radd: 'रद्द'
}

export const UserRole = {
  Admin: 'प्रशासक',
  Officer: 'अधिकारी',
  Staff: 'कर्मचारी'
}

export const statusColor = (status) => {
  const map = {
    Rikamy: '#0ea5e9', Bhadyane: '#16a34a', Seal: '#dc2626', Punarlilaw: '#f59e0b', Nishkriya: '#6b7280',
    Saru: '#16a34a', Sampla: '#6b7280', Radd: '#dc2626',
    ThakbakiOlkhli: '#f59e0b', NoticeDili: '#f97316', VasuliSuru: '#3b82f6', Band: '#16a34a',
    Prapt: '#3b82f6', ManjurZala: '#16a34a', Naklat: '#dc2626',
    JahirNamaPrasiddh: '#3b82f6', ArjSwikarane: '#f59e0b', LilawZala: '#8b5cf6', Manjur: '#16a34a',
    Prarup: '#f59e0b', Nishchit: '#16a34a'
  }
  return map[status] || '#6b7280'
}
