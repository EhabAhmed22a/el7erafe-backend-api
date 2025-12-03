using DomainLayer.Models.IdentityModule;

public class RejectedTechnician : Exception
{
    public string TechnicianName { get; }
    public string UserName { get; }
    public string RejectionReason { get; }
    public string CityName { get; }
    public string GovernorateName { get; }
    public string ServiceName { get; }
    public bool IsNationalIdFrontVerified { get; }
    public bool IsNationalIdBackVerified { get; }
    public bool IsCriminalHistoryVerified { get; }

    public RejectedTechnician(Technician technician)
        : base("لم يتم الموافقة على ملفاتك الشخصية من قبل المسؤول. يرجى مراجعة المتطلبات والمحاولة مرة أخرى")
    {
        TechnicianName = technician.Name;
        UserName = technician.User?.UserName ?? string.Empty;
        RejectionReason = technician.Rejection?.Reason ?? "غير محدد";
        CityName = technician.City?.NameAr ?? string.Empty;
        GovernorateName = technician.City?.Governorate?.NameAr ?? string.Empty;
        ServiceName = technician.Service?.NameAr ?? string.Empty;
        IsNationalIdFrontVerified = technician.IsNationalIdFrontRejected;
        IsNationalIdBackVerified = technician.IsNationalIdBackRejected;
        IsCriminalHistoryVerified = technician.IsCriminalHistoryRejected;
    }
}