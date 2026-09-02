using Microsoft.EntityFrameworkCore;
using SMC.Domain.Entities;
using SMC.Domain.Enums;

namespace SMC.Infrastructure.Persistence;

/// <summary>Database तयार झाल्यावर 10 staff users आणि नमुना मालमत्ता seed करते.</summary>
public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        if (!await db.Users.AnyAsync())
        {
            var users = new List<User>
            {
                new() { Username = "admin", FullName = "प्रशासक (Admin)", Designation = "प्रणाली प्रशासक", Role = UserRole.Admin,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"), CreatedBy = "System" },
                new() { Username = "officer1", FullName = "श्री. संजय पाटील", Designation = "मालमत्ता अधिकारी", Role = UserRole.JE,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Officer@123"), CreatedBy = "System" },
                new() { Username = "officer2", FullName = "श्रीमती. सुनीता जाधव", Designation = "महसूल अधिकारी", Role = UserRole.OS,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Officer@123"), CreatedBy = "System" },
                new() { Username = "staff1", FullName = "श्री. राहुल कुलकर्णी", Designation = "लिपिक", Role = UserRole.Staff,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Staff@123"), CreatedBy = "System" },
                new() { Username = "staff2", FullName = "श्री. महेश देशमुख", Designation = "लिपिक", Role = UserRole.Staff,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Staff@123"), CreatedBy = "System" },
                new() { Username = "staff3", FullName = "श्रीमती. प्रिया शिंदे", Designation = "लिपिक", Role = UserRole.Staff,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Staff@123"), CreatedBy = "System" },
                new() { Username = "staff4", FullName = "श्री. विजय भोसले", Designation = "लिपिक", Role = UserRole.Staff,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Staff@123"), CreatedBy = "System" },
                new() { Username = "staff5", FullName = "श्रीमती. कल्पना मोरे", Designation = "लिपिक", Role = UserRole.Staff,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Staff@123"), CreatedBy = "System" },
                new() { Username = "staff6", FullName = "श्री. अनिल गायकवाड", Designation = "लिपिक", Role = UserRole.Staff,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Staff@123"), CreatedBy = "System" },
                new() { Username = "staff7", FullName = "श्रीमती. वैशाली निंबाळकर", Designation = "लिपिक", Role = UserRole.Staff,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Staff@123"), CreatedBy = "System" },
            };
            db.Users.AddRange(users);
            await db.SaveChangesAsync();
        }

        await EnsureOfficerAsync(db, "officer1", "श्री. संजय पाटील", "मालमत्ता अधिकारी", UserRole.JE, "Officer@123");
        await EnsureOfficerAsync(db, "officer2", "श्रीमती. सुनीता जाधव", "महसूल अधिकारी", UserRole.OS, "Officer@123");
        await EnsureOfficerAsync(db, "assistantcommissioner", "सहाय्यक आयुक्त", "सहाय्यक आयुक्त", UserRole.AssistantCommissioner, "Assistant@123");

        if (!await db.Properties.AnyAsync())
        {
            var props = new List<Property>
            {
                new() { Category = PropertyCategory.MajorGaale, PropertyCode = "MG-001", Name = "मुख्य बाजारपेठ गाळा क्र.1",
                    Ward = "प्रभाग 5", Address = "मुख्य बाजारपेठ, सोलापूर", AreaSqFt = 450, MonthlyRent = 15000,
                    AnnualDemand = 180000, Status = PropertyStatus.Bhadyane, CurrentOccupant = "मे. गणेश ट्रेडर्स", CreatedBy = "System" },
                new() { Category = PropertyCategory.MiniGaale, PropertyCode = "MN-014", Name = "उपबाजार मिनी गाळा क्र.14",
                    Ward = "प्रभाग 3", Address = "उपबाजार परिसर", AreaSqFt = 120, MonthlyRent = 4000,
                    AnnualDemand = 48000, Status = PropertyStatus.Rikamy, CreatedBy = "System" },
                new() { Category = PropertyCategory.LandFee, PropertyCode = "LF-007", Name = "भुई भाडे जागा - स्टेशन रोड",
                    Ward = "प्रभाग 8", Address = "स्टेशन रोड", AreaSqFt = 800, MonthlyRent = 6000,
                    AnnualDemand = 72000, Status = PropertyStatus.Bhadyane, CurrentOccupant = "श्री. रमेश यादव", CreatedBy = "System" },
                new() { Category = PropertyCategory.SamajMandir, PropertyCode = "SM-002", Name = "समाज मंदिर - विजापूर रोड",
                    Ward = "प्रभाग 12", Address = "विजापूर रोड", AreaSqFt = 2000, MonthlyRent = 0,
                    AnnualDemand = 0, Status = PropertyStatus.Rikamy, CreatedBy = "System" },
                new() { Category = PropertyCategory.Abhyasika, PropertyCode = "AB-003", Name = "नागरी अभ्यासिका - होटगी रोड",
                    Ward = "प्रभाग 9", Address = "होटगी रोड", AreaSqFt = 600, MonthlyRent = 2000,
                    AnnualDemand = 24000, Status = PropertyStatus.Bhadyane, CurrentOccupant = "युवा अभ्यासिका मंडळ", CreatedBy = "System" },
                new() { Category = PropertyCategory.Gaale256, PropertyCode = "256-045", Name = "256 गाळे योजना - गाळा क्र.45",
                    Ward = "प्रभाग 6", Address = "256 गाळे संकुल", AreaSqFt = 200, MonthlyRent = 5000,
                    AnnualDemand = 60000, Status = PropertyStatus.Seal, CreatedBy = "System" },
                new() { Category = PropertyCategory.TP3_23, PropertyCode = "TP-323-011", Name = "TP-3/23 भूखंड क्र.11",
                    Ward = "प्रभाग 15", Address = "TP स्कीम 3/23", AreaSqFt = 1500, TpNumber = "TP-3/23",
                    SurveyNumber = "S.No. 245", MonthlyRent = 0, AnnualDemand = 0, Status = PropertyStatus.Rikamy, CreatedBy = "System" },
                new() { Category = PropertyCategory.AdhikrutKhoke, PropertyCode = "AK-021", Name = "अधिकृत खोके - जुनी मंडई",
                    Ward = "प्रभाग 2", Address = "जुनी मंडई परिसर", AreaSqFt = 40, MonthlyRent = 1500,
                    AnnualDemand = 18000, Status = PropertyStatus.Bhadyane, CurrentOccupant = "श्रीमती. सुनंदा पवार", CreatedBy = "System" },
                new() { Category = PropertyCategory.ItarBhadetatvavarilMalmatta, PropertyCode = "IT-005", Name = "इतर मनपा मालमत्ता - गोदाम जागा",
                    Ward = "प्रभाग 1", Address = "औद्योगिक वसाहत", AreaSqFt = 3000, MonthlyRent = 25000,
                    AnnualDemand = 300000, Status = PropertyStatus.Punarlilaw, CreatedBy = "System" },
            };
            db.Properties.AddRange(props);
            await db.SaveChangesAsync();
        }
    }

    private static async Task EnsureOfficerAsync(ApplicationDbContext db, string username, string fullName, string designation, UserRole role, string password)
    {
        var user = await db.Users.FirstOrDefaultAsync(x => x.Username == username && !x.IsDeleted);
        if (user is null)
        {
            db.Users.Add(new User
            {
                Username = username,
                FullName = fullName,
                Designation = designation,
                Role = role,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                CreatedBy = "System"
            });
            await db.SaveChangesAsync();
            return;
        }

        var passwordNeedsRepair = string.IsNullOrWhiteSpace(user.PasswordHash) || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        if (user.Role != role || !user.IsActive || passwordNeedsRepair)
        {
            user.Role = role;
            user.IsActive = true;
            if (passwordNeedsRepair)
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            user.UpdatedBy = "System";
            user.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }
    }
}
