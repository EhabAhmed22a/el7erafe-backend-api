
namespace DomainLayer.Exceptions
{
    public class BlockedTechnician() : Exception("تم حظر الحساب لتجاوز عدد محاولات التسجيل المسموح بها. يرجى التواصل مع الدعم.")
    {
    }
}
