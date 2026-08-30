# सोलापूर महानगरपालिका — भूमी व मालमत्ता व्यवस्थापन प्रणाली
## Solapur Municipal Corporation — Land & Property Management System

Clean Architecture वर आधारित पूर्ण-स्टॅक अनुप्रयोग: **ASP.NET Core 8 Web API + SQL Server/EF Core + React (Vite) + JWT Authentication**.

---

## ⚠️ महत्त्वाची सूचना (या ZIP बद्दल प्रामाणिक स्थिती)

हा कोड ज्या sandbox मध्ये तयार केला गेला त्यात **.NET SDK उपलब्ध नाही आणि इंटरनेट/नेटवर्क access देखील बंद आहे** (npm registry, NuGet दोन्ही अवरोधित). त्यामुळे:

- **Backend (C#):** कोड काळजीपूर्वक हाताने लिहिला आहे, पण `dotnet build` चालवून compile-verify करता आलेला **नाही**.
- **Frontend (React):** `npm install` करता आले नाही, त्यामुळे `npm run build`/`vite` ने देखील verify करता आलेले **नाही**.
- **EF Core Migration:** एक हाताने लिहिलेली `InitialCreate` migration दिली आहे, पण ती `dotnet ef migrations add` ने generate केलेली नसल्यामुळे प्रथम खालील "Database Setup" विभागातील शिफारसीय पद्धत वापरा.

**म्हणजे:** आर्किटेक्चर, business logic, API contracts, आणि UI हे पूर्ण व सुसंगत आहेत — पण प्रथमच run करताना छोट्या compile errors (टायपो, using मिसिंग, इ.) येण्याची शक्यता आहे. खाली दिलेल्या स्टेप्सने चालवताना जर एरर आली, तर ती सहसा एक-दोन ओळींची छोटी दुरुस्ती असेल. कृपया `dotnet build` व `npm run dev` चालवून समोर येणाऱ्या errors पुढील संदेशात मला सांगा — मी लगेच दुरुस्त करून देईन.

---

## 📁 प्रोजेक्ट रचना (Clean Architecture)

```
SMC-BhoomiVyavasthapan/
├── backend/
│   ├── SMC.sln
│   └── src/
│       ├── SMC.Domain/          → Entities, Enums (कोणतेही dependency नाही)
│       ├── SMC.Application/     → DTOs, Services, Interfaces, Reports (Excel/PDF)
│       ├── SMC.Infrastructure/  → EF Core DbContext, Migrations, JWT, File Storage
│       └── SMC.API/             → Controllers, Program.cs, Swagger, JWT config
├── frontend/                    → React (Vite) SPA — मराठी UI
├── database/
│   ├── schema.sql               → संपूर्ण SQL Server schema (पर्यायी manual मार्ग)
│   └── seed.sql                 → seed data बद्दल टीप
└── README.md
```

---

## 🧱 Technology Stack

| स्तर | Technology |
|---|---|
| Backend | ASP.NET Core 8 Web API, Clean Architecture |
| Database | SQL Server + EF Core 8 (Code-First Migrations) |
| Auth | JWT Bearer Authentication, BCrypt password hashing |
| Frontend | React 18 + Vite, React Router, Axios, Recharts |
| Reports | ClosedXML (Excel), QuestPDF (PDF) |
| API Docs | Swagger / OpenAPI |

---

## 🗂️ 5 मुख्य Tabs (सर्व backend + frontend मध्ये अंमलात आणलेले)

1. **मालमत्ता** — Major गाळे, Mini गाळे, Land Fee, समाज मंदिर, अभ्यासिका, 256 गाळे, TP-3/23, अधिकृत खोके, इतर मनपा मालमत्ता (एकाच `Property` table मध्ये `Category` द्वारे विभागलेले)
2. **हस्तांतरण** — दस्ताद्वारे भाडेपट्टा (किमान 3 वर्षे / 3-10 वर्षे / कमाल 29 वर्षे 11 महिने)
3. **वसुली प्रक्रिया** — थकबाकी → नोटीस → वसुली → सील → पुनर्लिलाव (workflow स्टेजेस)
4. **विविध उपक्रम** — अभय योजना, दंडमाफी, सवलत, इतर महसूलवाढीचे उपक्रम
5. **मालमत्ता देण्याची कार्यपद्धती** — सार्वजनिक लिलाव, निविदा, प्रसिद्धीकरण करून अर्ज मागविणे

प्रत्येक विभागात: **Add / View / Edit / Update / Search / Filter / Pagination / Documents / शेरा / Save** — हे सर्व एका सामायिक, config-driven `EntityCrudPage` React component द्वारे दिले आहे.

---

## 👤 वापरकर्ते (10 Staff Logins — DbSeeder मध्ये आपोआप तयार होतात)

| वापरकर्तानाव | पासवर्ड | Role |
|---|---|---|
| admin | Admin@123 | Admin |
| officer1 | Officer@123 | Officer |
| officer2 | Officer@123 | Officer |
| staff1 ... staff7 | Staff@123 | Staff |

**पहिल्या login नंतर सर्व पासवर्ड बदलण्याची शिफारस आहे.**

### Role-आधारित अधिकार
- **Admin:** सर्व CRUD + Delete + User Management
- **Officer:** Create/Update (Delete नाही)
- **Staff:** View + शेरा सह records पाहणे (Create/Update साठी backend मध्ये policy `AdminOrOfficer` वापरली आहे — गरज असल्यास Staff ला Create अधिकार देण्यासाठी `Program.cs` मधील policy व संबंधित Controllers मधील `[Authorize(Policy=...)]` सहज बदलता येईल)

---

## 🚀 चालवण्याच्या पायऱ्या

### आवश्यक Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/)
- SQL Server (LocalDB / Express / Developer / Docker) — किंवा SQL Server connection उपलब्ध असलेला कोणताही server

### 1. Database Setup (दोन पर्यायांपैकी एक निवडा)

**पर्याय A (शिफारसीय — EF Core Migrations):**
```bash
cd backend/src/SMC.API
dotnet tool install --global dotnet-ef   # एकदाच
dotnet ef migrations add InitialCreate -p ../SMC.Infrastructure -s .
dotnet ef database update -p ../SMC.Infrastructure -s .
```
वरील `migrations add` कमांड आधीपासून दिलेल्या hand-written migration ला **overwrite/regenerate** करून तुमच्या स्थानिक EF Core version शी 100% सुसंगत migration तयार करेल — हीच सर्वात सुरक्षित पद्धत आहे.

**पर्याय B (Manual SQL):**
```bash
sqlcmd -S localhost -i database/schema.sql
```
यानंतर backend चालवा — `DbSeeder.cs` आपोआप 10 users व नमुना मालमत्ता seed करेल (जर tables आधीच रिकामे असतील तरच).

### 2. Backend चालवा
```bash
cd backend/src/SMC.API
# appsettings.json मधील ConnectionStrings:DefaultConnection आपल्या SQL Server शी जुळवा
dotnet restore
dotnet build
dotnet run
```
- API: `http://localhost:5000`
- Swagger UI: `http://localhost:5000/swagger`

### एकाच कमांडने चालवा
Repository root मधून PowerShell मध्ये:
```powershell
.\start.ps1
```
ही script API `http://localhost:5000` आणि frontend `http://localhost:5173` वर सुरू करते.

### 3. Frontend चालवा
```bash
cd frontend
npm install
cp .env.example .env    # गरज असल्यास VITE_API_BASE_URL बदला
npm run dev
```
- App: `http://localhost:5173`

### 4. Login करा
वर दिलेल्या 10 users पैकी कोणत्याही एकाने login करा.

---

## 🔐 Security वैशिष्ट्ये
- JWT Bearer Authentication (8 तास expiry)
- BCrypt password hashing
- Role-based Authorization (Admin/Officer/Staff policies)
- Soft Delete (सर्व मुख्य entities — `IsDeleted`, `DeletedBy`, `DeletedAt`)
- Secure file upload (extension whitelist: pdf/jpg/png/docx/xlsx, 10MB मर्यादा, random file नावे)
- Global exception middleware (मराठी error messages)
- EF Core global query filters (soft-deleted records आपोआप वगळले जातात)

## 📝 Audit Trail
प्रत्येक Create/Update/Delete वर `AuditLog` table मध्ये नोंद होते: कोणी (User), काय (EntityName+Field), जुनी value, नवीन value, तारीख-वेळ, IP. प्रत्येक record च्या "बदल इतिहास" tab मध्ये तसेच स्वतंत्र "Audit इतिहास" पानावर हे दिसते.

## 📊 Reports
`/api/reports/...` endpoints द्वारे मालमत्ता/हस्तांतरण/वसुली/Audit अहवाल Excel (ClosedXML) व PDF (QuestPDF) स्वरूपात डाउनलोड करता येतात — frontend च्या "अहवाल" पानावरून एका क्लिकवर.

## 🖥️ Dashboard
एकूण मालमत्ता, एकूण गाळे, रिक्त मालमत्ता, भाडेतत्त्वावरील मालमत्ता, वार्षिक मागणी, एकूण वसुली, एकूण थकबाकी, प्रलंबित प्रकरणे — Bar/Pie charts (Recharts) सह.

---

## 🧩 पुढील सुधारणा सुचवल्यास (Roadmap कल्पना)
- Unit/Integration tests (xUnit + FluentAssertions)
- Refresh tokens
- Email/SMS notifications (नोटीस पाठवण्यासाठी)
- Advanced role-permission matrix (field-level)
- Docker Compose (API + SQL Server + Frontend एकत्र)

---

## 📞 त्रुटी आढळल्यास
`dotnet build` किंवा `npm run dev`/`npm run build` चालवताना कोणतीही error आढळल्यास, ती error message पुढील संदेशात पाठवा — मी लगेच नेमकी दुरुस्ती करून देईन.
