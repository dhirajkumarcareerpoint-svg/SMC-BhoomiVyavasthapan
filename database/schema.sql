/* =====================================================================
   सोलापूर महानगरपालिका - भूमी व मालमत्ता व्यवस्थापन प्रणाली
   Database Schema (SQL Server)
   टीप: हे स्क्रिप्ट संदर्भासाठी दिले आहे. वास्तविक deployment साठी
   backend/src/SMC.API मधून खालील कमांड चालवा (शिफारसीय पद्धत):
       dotnet ef migrations add InitialCreate -p ../SMC.Infrastructure -s .
       dotnet ef database update -p ../SMC.Infrastructure -s .
   EF Core migrations आपोआप हेच schema (व त्यापेक्षा अधिक अचूक) तयार करतील.
   ===================================================================== */

IF DB_ID('SMC_BhoomiVyavasthapan') IS NULL
BEGIN
    CREATE DATABASE SMC_BhoomiVyavasthapan;
END
GO

USE SMC_BhoomiVyavasthapan;
GO

-- =====================  Users  =====================
CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    FullName NVARCHAR(150) NOT NULL,
    Designation NVARCHAR(100) NULL,
    Mobile NVARCHAR(15) NULL,
    Email NVARCHAR(150) NULL,
    Role NVARCHAR(20) NOT NULL,               -- Admin / Officer / Staff
    IsActive BIT NOT NULL DEFAULT 1,
    LastLoginAt DATETIME2 NULL,
    Shera NVARCHAR(2000) NULL,
    CreatedBy NVARCHAR(100) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedBy NVARCHAR(100) NULL,
    UpdatedAt DATETIME2 NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    DeletedBy NVARCHAR(100) NULL
);
GO

-- =====================  Properties (मालमत्ता)  =====================
CREATE TABLE Properties (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Category NVARCHAR(40) NOT NULL,           -- MajorGaale, MiniGaale, LandFee, SamajMandir, Abhyasika, Gaale256, TP3_23, AdhikrutKhoke, ItarBhadetatvavarilMalmatta
    PropertyCode NVARCHAR(50) NOT NULL,
    Name NVARCHAR(250) NOT NULL,
    Ward NVARCHAR(50) NULL,
    Zone NVARCHAR(50) NULL,
    Address NVARCHAR(500) NULL,
    AreaSqFt DECIMAL(18,2) NULL,
    MonthlyRent DECIMAL(18,2) NOT NULL DEFAULT 0,
    AnnualDemand DECIMAL(18,2) NOT NULL DEFAULT 0,
    SurveyNumber NVARCHAR(50) NULL,
    TpNumber NVARCHAR(50) NULL,
    Status NVARCHAR(30) NOT NULL DEFAULT 'Rikamy',   -- Rikamy, Bhadyane, Seal, Punarlilaw, Nishkriya
    CurrentOccupant NVARCHAR(200) NULL,
    Shera NVARCHAR(2000) NULL,
    CreatedBy NVARCHAR(100) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedBy NVARCHAR(100) NULL,
    UpdatedAt DATETIME2 NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    DeletedBy NVARCHAR(100) NULL
);
CREATE INDEX IX_Properties_Code ON Properties(PropertyCode);
CREATE INDEX IX_Properties_Category ON Properties(Category);
CREATE INDEX IX_Properties_Status ON Properties(Status);
CREATE INDEX IX_Properties_Ward ON Properties(Ward);
GO

-- =====================  Leases (हस्तांतरण)  =====================
CREATE TABLE Leases (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    PropertyId INT NOT NULL REFERENCES Properties(Id),
    LesseeName NVARCHAR(200) NOT NULL,
    LesseeMobile NVARCHAR(15) NULL,
    LesseeAddress NVARCHAR(500) NULL,
    DeedNumber NVARCHAR(100) NOT NULL,
    DeedDate DATETIME2 NOT NULL,
    DurationType NVARCHAR(30) NOT NULL,       -- Min3Years, ThreeToTenYears, Max29Years11Months
    StartDate DATETIME2 NOT NULL,
    EndDate DATETIME2 NOT NULL,
    RentAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    SecurityDeposit DECIMAL(18,2) NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Saru',   -- Saru, Sampla, Radd
    Shera NVARCHAR(2000) NULL,
    CreatedBy NVARCHAR(100) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedBy NVARCHAR(100) NULL,
    UpdatedAt DATETIME2 NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    DeletedBy NVARCHAR(100) NULL
);
CREATE INDEX IX_Leases_DeedNumber ON Leases(DeedNumber);
CREATE INDEX IX_Leases_Status ON Leases(Status);
GO

-- =====================  RecoveryCases (वसुली प्रक्रिया)  =====================
CREATE TABLE RecoveryCases (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    PropertyId INT NOT NULL REFERENCES Properties(Id),
    LeaseId INT NULL REFERENCES Leases(Id),
    MonthsOverdue INT NOT NULL DEFAULT 0,
    OutstandingAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    Stage NVARCHAR(30) NOT NULL DEFAULT 'ThakbakiOlkhli', -- ThakbakiOlkhli, NoticeDili, VasuliSuru, Seal, Punarlilaw, Band
    NoticeNumber NVARCHAR(100) NULL,
    NoticeDate DATETIME2 NULL,
    RecoveredAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    RecoveryDate DATETIME2 NULL,
    SealDate DATETIME2 NULL,
    ReAuctionDate DATETIME2 NULL,
    Shera NVARCHAR(2000) NULL,
    CreatedBy NVARCHAR(100) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedBy NVARCHAR(100) NULL,
    UpdatedAt DATETIME2 NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    DeletedBy NVARCHAR(100) NULL,
    CONSTRAINT CK_RecoveryCases_MinMonths CHECK (MonthsOverdue >= 3)
);
CREATE INDEX IX_RecoveryCases_Stage ON RecoveryCases(Stage);
GO

-- =====================  SchemeApplications (विविध उपक्रम)  =====================
CREATE TABLE SchemeApplications (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    PropertyId INT NOT NULL REFERENCES Properties(Id),
    SchemeType NVARCHAR(30) NOT NULL,          -- AbhayYojana, DandMafi, Savlat, Itar
    ApplicantName NVARCHAR(200) NOT NULL,
    ApplicantMobile NVARCHAR(15) NULL,
    ApplicationDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    OriginalOutstanding DECIMAL(18,2) NOT NULL DEFAULT 0,
    WaivedAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    PayableAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Prapt',  -- Prapt, ManjurZala, Naklat
    DecisionDate DATETIME2 NULL,
    ApprovedBy NVARCHAR(150) NULL,
    Shera NVARCHAR(2000) NULL,
    CreatedBy NVARCHAR(100) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedBy NVARCHAR(100) NULL,
    UpdatedAt DATETIME2 NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    DeletedBy NVARCHAR(100) NULL
);
CREATE INDEX IX_SchemeApplications_Type ON SchemeApplications(SchemeType);
GO

-- =====================  AllocationProcesses (मालमत्ता देण्याची कार्यपद्धती)  =====================
CREATE TABLE AllocationProcesses (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    PropertyId INT NOT NULL REFERENCES Properties(Id),
    Method NVARCHAR(30) NOT NULL,              -- SarvajanikLilaw, Niviva, PrasiddhikaranArj
    NoticeNumber NVARCHAR(100) NULL,
    PublishDate DATETIME2 NOT NULL,
    LastDateToApply DATETIME2 NULL,
    AuctionDate DATETIME2 NULL,
    ReserveAmount DECIMAL(18,2) NULL,
    HighestBidAmount DECIMAL(18,2) NULL,
    HighestBidderName NVARCHAR(200) NULL,
    HighestBidderMobile NVARCHAR(15) NULL,
    Status NVARCHAR(30) NOT NULL DEFAULT 'JahirNamaPrasiddh', -- JahirNamaPrasiddh, ArjSwikarane, LilawZala, Manjur, Radd
    Shera NVARCHAR(2000) NULL,
    CreatedBy NVARCHAR(100) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedBy NVARCHAR(100) NULL,
    UpdatedAt DATETIME2 NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    DeletedBy NVARCHAR(100) NULL
);
CREATE INDEX IX_AllocationProcesses_Method ON AllocationProcesses(Method);
CREATE INDEX IX_AllocationProcesses_Status ON AllocationProcesses(Status);
GO

-- =====================  Documents  =====================
-- =====================  Calculations (गणना)  =====================
-- टीप: CalculatedAmount/TotalAmount साठी निश्चित व्यवसाय सूत्र प्रणालीत अद्याप उपलब्ध नाही;
-- ही रक्कम सध्या अधिकाऱ्याने पडताळून भरावी लागते (संरचना भविष्यातील स्वयंचलित आकारणीसाठी तयार आहे).
CREATE TABLE Calculations (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    PropertyId INT NOT NULL REFERENCES Properties(Id),
    Rate DECIMAL(18,2) NULL,
    PeriodMonths INT NOT NULL DEFAULT 0,
    PreviousOutstanding DECIMAL(18,2) NULL,
    CurrentDemand DECIMAL(18,2) NULL,
    CalculatedAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    TotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    CalculationDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    Status NVARCHAR(30) NOT NULL DEFAULT 'Prarup',   -- Prarup, Nishchit, Radd
    Shera NVARCHAR(2000) NULL,
    CreatedBy NVARCHAR(100) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedBy NVARCHAR(100) NULL,
    UpdatedAt DATETIME2 NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    DeletedBy NVARCHAR(100) NULL
);
CREATE INDEX IX_Calculations_PropertyId ON Calculations(PropertyId);
CREATE INDEX IX_Calculations_Status ON Calculations(Status);
GO

CREATE TABLE Documents (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    EntityType NVARCHAR(30) NOT NULL,          -- Property, Lease, RecoveryCase, Scheme, Allocation, Calculation
    EntityId INT NOT NULL,
    FileName NVARCHAR(300) NOT NULL,
    StoredFileName NVARCHAR(300) NOT NULL,
    FilePath NVARCHAR(500) NOT NULL,
    ContentType NVARCHAR(100) NULL,
    FileSizeBytes BIGINT NOT NULL DEFAULT 0,
    PropertyId INT NULL REFERENCES Properties(Id),
    LeaseId INT NULL REFERENCES Leases(Id),
    RecoveryCaseId INT NULL REFERENCES RecoveryCases(Id),
    SchemeApplicationId INT NULL REFERENCES SchemeApplications(Id),
    AllocationProcessId INT NULL REFERENCES AllocationProcesses(Id),
    CalculationId INT NULL REFERENCES Calculations(Id),
    Shera NVARCHAR(2000) NULL,
    CreatedBy NVARCHAR(100) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedBy NVARCHAR(100) NULL,
    UpdatedAt DATETIME2 NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    DeletedBy NVARCHAR(100) NULL
);
CREATE INDEX IX_Documents_Entity ON Documents(EntityType, EntityId);
GO

-- =====================  AuditLogs  =====================
CREATE TABLE AuditLogs (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL REFERENCES Users(Id),
    UserName NVARCHAR(150) NOT NULL,
    Action NVARCHAR(30) NOT NULL,              -- Create, Update, Delete, Login, Upload
    EntityName NVARCHAR(100) NOT NULL,
    EntityId INT NOT NULL,
    FieldName NVARCHAR(100) NULL,
    OldValue NVARCHAR(2000) NULL,
    NewValue NVARCHAR(2000) NULL,
    Timestamp DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    IpAddress NVARCHAR(50) NULL
);
CREATE INDEX IX_AuditLogs_Entity ON AuditLogs(EntityName, EntityId);
CREATE INDEX IX_AuditLogs_Timestamp ON AuditLogs(Timestamp);
GO

PRINT 'SMC_BhoomiVyavasthapan schema तयार झाले.';
