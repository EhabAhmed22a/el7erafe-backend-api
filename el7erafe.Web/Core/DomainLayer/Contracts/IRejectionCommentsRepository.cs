using DomainLayer.Models.IdentityModule;

namespace DomainLayer.Contracts
{
    public interface IRejectionCommentsRepository
    {
        Task<IEnumerable<RejectionComment>?> GetAllRejectionCommentsAsync();
    }
}
