using Microsoft.Extensions.Hosting;

namespace KPW.Infrastructure.Services.Ai;

internal record EducationChunk(string Title, string Content);

internal static class EducationDocumentLoader
{
    public static IReadOnlyList<EducationChunk> Load(IHostEnvironment environment)
    {
        var assemblyDir = Path.GetDirectoryName(typeof(EducationDocumentLoader).Assembly.Location)!;
        var candidates = new[]
        {
            Path.Combine(environment.ContentRootPath, "Education"),
            Path.Combine(assemblyDir, "Education"),
            Path.Combine(AppContext.BaseDirectory, "Education")
        };

        foreach (var educationPath in candidates)
        {
            if (!Directory.Exists(educationPath))
            {
                continue;
            }

            var files = Directory.GetFiles(educationPath, "*.md");
            if (files.Length == 0)
            {
                continue;
            }

            return files
                .Select(path =>
                {
                    var title = Path.GetFileNameWithoutExtension(path).Replace('_', ' ');
                    var content = File.ReadAllText(path);
                    return new EducationChunk(title, content.Trim());
                })
                .ToList();
        }

        return GetEmbeddedFallback();
    }

    private static IReadOnlyList<EducationChunk> GetEmbeddedFallback() =>
    [
        new EducationChunk(
            "Hip Dysplasia Recovery",
            "Controlled leash walks on flat surfaces help rebuild hip stability after hip dysplasia surgery. Avoid slippery floors and jumping for the first six weeks."),
        new EducationChunk(
            "Pain Monitoring",
            "A pain score above 7 out of 10 after exercise may indicate overexertion. Reduce repetitions and contact your physiotherapist if pain persists beyond 24 hours."),
        new EducationChunk(
            "Sit to Stand Exercise",
            "The sit-to-stand exercise strengthens hind limb muscles. Ensure paws stay square, movements are slow, and the pet is rewarded for correct form."),
        new EducationChunk(
            "Energy and Appetite",
            "Declining appetite combined with low energy can signal pain or medication side effects. Track daily scores and share trends with your rehabilitation team."),
        new EducationChunk(
            "Lameness Awareness",
            "Mild lameness after rest that improves with gentle movement is common in early rehab. Sudden worsening requires veterinary assessment."),
        new EducationChunk(
            "Home Safety",
            "Use non-slip mats, limit stairs, and provide a supportive bed to protect recovering joints during at-home rehabilitation programs.")
    ];
}
