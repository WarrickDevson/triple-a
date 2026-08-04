using KPW.Application.Interfaces;
using KPW.Domain.Common;
using KPW.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace KPW.Infrastructure.Data;

public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;

    public AuditSaveChangesInterceptor(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ApplyAudit(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyAudit(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyAudit(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var utcNow = DateTime.UtcNow;
        var userId = _currentUserService.UserId;

        foreach (var entry in context.ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedDate = utcNow;
                    entry.Entity.ModifiedDate = utcNow;
                    entry.Entity.CreatedUserId = userId;
                    entry.Entity.ModifiedUserId = userId;
                    entry.Entity.IsActive = true;
                    break;
                case EntityState.Modified:
                    entry.Entity.ModifiedDate = utcNow;
                    entry.Entity.ModifiedUserId = userId;
                    break;
            }
        }
    }
}
