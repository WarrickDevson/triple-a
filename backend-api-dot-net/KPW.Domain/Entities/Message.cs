using KPW.Domain.Common;

namespace KPW.Domain.Entities;

public class Message : AuditableEntity
{
    public int MessageId { get; set; }
    public int MessageThreadId { get; set; }
    public int SenderUserId { get; set; }
    public string Body { get; set; } = string.Empty;
    public int? VideoSubmissionId { get; set; }
    public DateTime? ReadAt { get; set; }

    public MessageThread Thread { get; set; } = null!;
    public User Sender { get; set; } = null!;
    public VideoSubmission? VideoSubmission { get; set; }
}
