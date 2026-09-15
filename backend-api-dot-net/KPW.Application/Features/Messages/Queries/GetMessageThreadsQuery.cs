using KPW.Application.DTOs.Messages;
using KPW.Application.Features.Pets;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using KPW.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.Messages.Queries;

public record GetMessageThreadsQuery : IRequest<IReadOnlyList<MessageThreadDto>>;

public class GetMessageThreadsQueryHandler : IRequestHandler<GetMessageThreadsQuery, IReadOnlyList<MessageThreadDto>>
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorageService;

    public GetMessageThreadsQueryHandler(
        DbContext dbContext,
        ICurrentUserService currentUserService,
        IFileStorageService fileStorageService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _fileStorageService = fileStorageService;
    }

    public async Task<IReadOnlyList<MessageThreadDto>> Handle(
        GetMessageThreadsQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is null)
        {
            throw new UnauthorizedAccessException();
        }

        var currentUserId = _currentUserService.UserId.Value;
        var currentUser = await _dbContext.Set<User>()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == currentUserId, cancellationToken);

        if (currentUser is null)
        {
            throw new UnauthorizedAccessException();
        }

        var query = _dbContext.Set<MessageThread>()
            .Include(t => t.Pet)
            .Include(t => t.Owner)
            .Include(t => t.Physio)
            .Include(t => t.Messages)
                .ThenInclude(m => m.Sender)
            .AsQueryable();

        if (_currentUserService.Role == UserRole.Owner)
        {
            query = query.Where(t => t.OwnerId == currentUserId);
        }
        else if (_currentUserService.Role == UserRole.Physio)
        {
            if (currentUser.ClinicId is not null)
            {
                query = query.Where(t => t.PhysioId == currentUserId || t.Pet.Owner.ClinicId == currentUser.ClinicId);
            }
            else
            {
                query = query.Where(t => t.PhysioId == currentUserId);
            }
        }
        else if (currentUser.ClinicId is not null)
        {
            query = query.Where(t => t.Pet.Owner.ClinicId == currentUser.ClinicId);
        }
        else
        {
            return [];
        }

        var threads = await query.ToListAsync(cancellationToken);

        return threads
            .Select(t =>
            {
                var lastMessage = t.Messages
                    .OrderByDescending(m => m.CreatedDate)
                    .FirstOrDefault();
                var unreadCount = t.Messages.Count(m =>
                    m.SenderUserId != currentUserId && m.ReadAt is null);

                return new MessageThreadDto(
                    t.MessageThreadId,
                    t.PetId,
                    t.Pet.PetName,
                    t.OwnerId,
                    $"{t.Owner.FirstName} {t.Owner.LastName}",
                    t.PhysioId,
                    $"{t.Physio.FirstName} {t.Physio.LastName}",
                    lastMessage?.Body,
                    lastMessage?.CreatedDate,
                    unreadCount,
                    !string.IsNullOrWhiteSpace(t.Pet.ProfilePictureUrl)
                        ? _fileStorageService.GetPublicUrl(t.Pet.ProfilePictureUrl)
                        : t.Pet.ProfilePictureUrl,
                    !string.IsNullOrWhiteSpace(t.Owner.ProfilePictureUrl)
                        ? _fileStorageService.GetPublicUrl(t.Owner.ProfilePictureUrl)
                        : t.Owner.ProfilePictureUrl,
                    !string.IsNullOrWhiteSpace(t.Physio.ProfilePictureUrl)
                        ? _fileStorageService.GetPublicUrl(t.Physio.ProfilePictureUrl)
                        : t.Physio.ProfilePictureUrl);
            })
            .OrderByDescending(t => t.LastMessageAt ?? DateTime.MinValue)
            .ToList();
    }
}
