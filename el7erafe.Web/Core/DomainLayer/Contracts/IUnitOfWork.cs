
namespace DomainLayer.Contracts
{
    public interface IUnitOfWork
    {
        Task BeginTransactionAsync();
        Task CommitTransactionWithoutSavingChangesAsync();
        Task RollbackTransactionAsync();
    }
}
