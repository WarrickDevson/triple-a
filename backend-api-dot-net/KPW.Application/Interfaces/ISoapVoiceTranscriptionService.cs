using KPW.Application.DTOs.SoapNotes;

namespace KPW.Application.Interfaces;

public interface ISoapVoiceTranscriptionService
{
    Task<StructuredSoapNoteDto> ParseNarrativeAsync(ParseSoapNarrativeRequestDto request, CancellationToken cancellationToken = default);
    Task<SoapTranscriptionResultDto> TranscribeAudioAsync(Stream audioStream, string contentType, string? petName, string? species, CancellationToken cancellationToken = default);
    SoapVocabularyDto GetDomainVocabulary();
}
