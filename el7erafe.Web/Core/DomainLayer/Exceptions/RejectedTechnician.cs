using DomainLayer.Models.IdentityModule;

public class RejectedTechnician : Exception
{
    public string TechnicianName { get; }
    public string UserName { get; }
    public string RejectionReason { get; }
    public string CityName { get; }
    public string GovernorateName { get; } // Added
    public string ServiceName { get; }
    public bool IsNationalIdFrontVerified { get; } // Added
    public bool IsNationalIdBackVerified { get; } // Added
    public bool IsCriminalHistoryVerified { get; } // Added

    public RejectedTechnician(Technician technician)
        : base("لم يتم الموافقة على ملفاتك الشخصية من قبل المسؤول. يرجى مراجعة المتطلبات والمحاولة مرة أخرى")
    {
        TechnicianName = technician.Name;
        UserName = technician.User?.UserName ?? string.Empty;
        RejectionReason = technician.Rejection?.Reason ?? "غير محدد";
        CityName = technician.City?.NameAr ?? string.Empty;
        GovernorateName = technician.City?.Governorate?.NameAr ?? string.Empty; // Added
        ServiceName = technician.Service?.NameAr ?? string.Empty;
        IsNationalIdFrontVerified = technician.IsNationalIdFrontVerified; // Added
        IsNationalIdBackVerified = technician.IsNationalIdBackVerified; // Added
        IsCriminalHistoryVerified = technician.IsCriminalHistoryVerified; // Added
    }
}