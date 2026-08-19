using KPW.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.SoapNotes.Commands;

public record DeleteSoapNoteCommand(int SoapNoteId) : IRequest<bool>;

public class DeleteSoapNoteCommandHandler : IRequestHandler<DeleteSoapNoteCommand, bool>
{
    private readonly DbContext _dbContext;

    public DeleteSoapNoteCommandHandler(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(DeleteSoapNoteCommand command, CancellationToken cancellationToken)
    {
        var note = await _dbContext.Set<SoapNote>()
            .FirstOrDefaultAsync(s => s.SoapNoteId == command.SoapNoteId, cancellationToken);

        if (note is null)
        {
            return false;
        }

        note.IsActive = false;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
