# Google Cloud setup (KPW MoveWell)

Live providers are wired behind existing interfaces. Switch back to local adapters by setting `Video:Provider=Local` and `Ai:Provider=Local` in `appsettings.json`.

## Project config (locked)

| Setting | Value |
|---------|--------|
| GCP project | `devson-development` |
| Bucket | `kpw-movewell` (US multi-region, private) |
| API region | `us-central1` |
| Vertex model | `gemini-3.5-flash-lite` |

## One-time GCP setup

1. **Enable APIs** on `devson-development`:
   - Cloud Storage API
   - Transcoder API
   - Vertex AI API

2. **Service account** — create one SA (e.g. `kpw-movewell-api`) with roles:
   - Storage Object Admin (on bucket `kpw-movewell`, or project-level if preferred)
   - Transcoder Admin (or Transcoder User)
   - Vertex AI User

3. **Download JSON key** for the service account. Do **not** commit it.

   Local default for this repo: place the key as  
   `KPW.Api/devson-development-*.json`  
   (already gitignored). `launchSettings.json` sets `GOOGLE_APPLICATION_CREDENTIALS` to that filename so `dotnet run` / Visual Studio pick it up automatically when the working directory is `KPW.Api`.

4. **Optional override** (PowerShell) if the file lives elsewhere:
   ```powershell
   $env:GOOGLE_APPLICATION_CREDENTIALS = "C:\secrets\devson-development-sa.json"
   ```

   The .NET Google SDK reads `GOOGLE_APPLICATION_CREDENTIALS` from the environment automatically.

## App configuration

[`appsettings.json`](KPW.Api/appsettings.json):

```json
"Video": {
  "Provider": "Google",
  "ProjectId": "devson-development",
  "Bucket": "kpw-movewell",
  "Location": "us-central1",
  "SignedUrlMinutes": 60
},
"Ai": {
  "Provider": "Vertex",
  "ProjectId": "devson-development",
  "Location": "us-central1",
  "Model": "gemini-3.5-flash-lite"
}
```

To work offline without GCP, set `Provider` to `Local` for Video and Ai.

## Smoke tests

### Video (GCS + Transcoder)

1. Start API with `GOOGLE_APPLICATION_CREDENTIALS` set.
2. Owner login (`owner@demo.kpw` / `Owner123!`) → upload exercise video in Flutter.
3. Confirm object under `videos/raw/` in bucket `kpw-movewell`.
4. Wait for background job → `ProcessingStatus` becomes `Ready`; processed file under `videos/processed/.../sd.mp4`.
5. Physio login → Video Approvals → video plays via signed HTTPS URL.

### AI chat (Vertex)

1. Owner app → Wellness Assistant.
2. Ask: *"What should I do if my dog's pain is high after exercise?"*
3. Expect Gemini-grounded answer with source titles from education docs.
4. If Vertex fails, response falls back to the standard “book a consultation” message.

### Reports (unchanged)

QuestPDF reports do not use GCP. Physio → Program Builder → Download PDF Report.

## Troubleshooting

| Symptom | Check |
|---------|--------|
| API fails at startup with bucket error | `Video:Bucket` and SA Storage permissions |
| Upload works, transcode fails | Transcoder API enabled; SA has Transcoder role; input file is valid video |
| Signed URL 403 | SA can read objects; URL not expired (`SignedUrlMinutes`) |
| AI always returns fallback | Vertex API enabled; model name/region; SA has `aiplatform.user` |
| Works on machine without GCP | Set `Video:Provider=Local` and `Ai:Provider=Local` |
