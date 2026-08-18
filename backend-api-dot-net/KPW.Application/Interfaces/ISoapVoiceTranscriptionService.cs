using KPW.Application.DTOs.SoapNotes;

namespace KPW.Application.Interfaces;

public interface ISoapVoiceTranscriptionService
{
    Task<StructuredSoapNoteDto> ParseNarrativeAsync(ParseSoapNarrativeRequestDto request, CancellationToken cancellationToken = default);
    Task<SoapTranscriptionResultDto> TranscribeAudioAsync(Stream audioStream, string contentType, string? petName, string? species, CancellationToken cancellationToken = default);
    Task<ProcessSessionAudioResponseDto> ProcessSessionAudioAsync(Stream audioStream, string fileName, string contentType, string? petName, string? species, int? petId = null, CancellationToken cancellationToken = default);
    Task<PolishSoapSectionResponseDto> PolishSectionAsync(PolishSoapSectionRequestDto request, CancellationToken cancellationToken = default);
    AiConfigStatusDto GetAiConfigStatus();
    SoapVocabularyDto GetDomainVocabulary();
}
