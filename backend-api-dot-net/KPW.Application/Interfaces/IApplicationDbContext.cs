using KPW.Application.Interfaces;

namespace KPW.Application;

public interface IApplicationDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
