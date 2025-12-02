using DomainLayer.Contracts;
using DomainLayer.Models.IdentityModule;
using Microsoft.EntityFrameworkCore;
using Persistance.Databases;

namespace Persistance.Repositories
{
    public class RejectionCommentsRepository(ApplicationDbContext dbContext) : IRejectionCommentsRepository
    {

        public async Task<IEnumerable<RejectionComment>?> GetAllRejectionCommentsAsync()
        {
            return await dbContext.Set<RejectionComment>()
                .ToListAsync();
        }
    }
}
