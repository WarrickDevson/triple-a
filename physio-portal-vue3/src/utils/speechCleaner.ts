import { correctVeterinaryTranscript } from './veterinaryLexicon'

/**
 * Common filler words, disfluencies, and hesitation sounds to remove from spoken transcripts.
 */
const FILLER_PATTERNS = [
  // Standalone filler sounds
  /\b(um|umm|ummm|uh|uhh|uhhh|er|err|ah|ahh|eh|hmm|hm)\b/gi,
  // Conversational filler phrases (when used as hesitations)
  /\b(you know|i mean|like sort of|kind of like|sort of like|basically like)\b/gi,
  // Leading filler transitions at the start of clauses
  /(?:^|[.!?]\s+)(?:so\s+um|and\s+um|well\s+uh|so\s+uh|like\s+um)\s+/gi,
]

/**
 * Spoken punctuation replacement map for hands-free dictation formatting.
 */
const SPOKEN_PUNCTUATION_MAP: Record<string, string> = {
  ' full stop ': '. ',
  ' period ': '. ',
  ' comma ': ', ',
  ' question mark ': '? ',
  ' exclamation mark ': '! ',
  ' new line ': '\n',
  ' next line ': '\n',
  ' bullet point ': '\n• ',
  ' next point ': '\n• ',
  ' colon ': ': ',
  ' semicolon ': '; ',
}

/**
 * Hands-free verbal stop commands that finish the dictation session automatically.
 */
export const HANDS_FREE_STOP_COMMANDS = [
  'stop dictation',
  'end note',
  'finish note',
  'stop recording',
  'end dictation'
] as const

/**
 * Voice commands cheat sheet for the info tooltip/modal.
 */
export const VOICE_COMMANDS_HELP = [
  {
    category: 'Smart Pause Detection',
    items: [
      { spoken: 'Pause ≤ 2 seconds (0.8s - 2s)', effect: 'Automatically inserts a comma ( , )' },
      { spoken: 'Pause 2 - 6 seconds', effect: 'Automatically inserts a full stop ( . ) and capitalizes next sentence' },
      { spoken: 'Pause > 6 seconds', effect: 'Automatically starts a new line paragraph' },
      { spoken: 'Pause > 12 seconds', effect: 'Automatically ends recording and saves note hands-free' },
    ]
  },
  {
    category: 'Spoken Punctuation & Formatting',
    items: [
      { spoken: '"period" or "full stop"', effect: 'Inserts a period ( . )' },
      { spoken: '"comma"', effect: 'Inserts a comma ( , )' },
      { spoken: '"new line" / "next line"', effect: 'Starts a new line' },
      { spoken: '"bullet point" / "next point"', effect: 'Inserts a bullet item ( • )' },
    ]
  },
  {
    category: 'Hands-Free Stop Phrase',
    items: [
      { spoken: '"stop dictation" or "end note"', effect: 'Instantly ends recording without touching the screen' },
    ]
  },
  {
    category: 'Speech Cleaning & Normalization',
    items: [
      { spoken: '"um", "uh", "you know", "er"', effect: 'Automatically filtered out' },
      { spoken: 'Repeated stuttered words', effect: 'Duplicates (e.g. "the the") removed automatically' },
      { spoken: 'Medical jargon (TPLO, PROM, stifle)', effect: 'Auto-corrected to correct clinical spelling' },
    ]
  }
]

/**
 * Checks if spoken text contains a hands-free stop command (e.g., "stop dictation", "end note").
 * If found, strips the trigger phrase from the text and returns shouldStop = true.
 */
export function detectAndStripStopCommand(rawText: string): { text: string; shouldStop: boolean } {
  if (!rawText) return { text: '', shouldStop: false }

  let cleaned = rawText
  let shouldStop = false

  for (const cmd of HANDS_FREE_STOP_COMMANDS) {
    const regex = new RegExp(`\\b${cmd}\\b[.]?`, 'gi')
    if (regex.test(cleaned)) {
      shouldStop = true
      cleaned = cleaned.replace(regex, ' ')
    }
  }

  return {
    text: cleaned.trim(),
    shouldStop
  }
}

/**
 * Formats punctuation based on spoken silence/pause duration:
 * - 0.75s to 2.0s pause: Adds comma ( , )
 * - 2.0s to 6.0s pause: Adds full stop ( . ) + capitalization
 * - 6.0s to 12.0s pause: Adds new line (\n\n)
 *
 * @param existingText The current accumulated text in the textarea
 * @param newChunk The newly transcribed and cleaned speech chunk
 * @param pauseSeconds The duration in seconds since the last speech utterance
 */
export function formatPausePunctuation(existingText: string, newChunk: string, pauseSeconds: number): string {
  if (!newChunk || !newChunk.trim()) return existingText
  const trimmedExisting = existingText.trim()
  const trimmedChunk = newChunk.trim()

  if (!trimmedExisting) {
    // Capitalize first letter of initial text
    return trimmedChunk.charAt(0).toUpperCase() + trimmedChunk.slice(1)
  }

  // If new chunk already starts with punctuation, append directly
  if (/^[,.;:?!]/.test(trimmedChunk)) {
    return `${trimmedExisting}${trimmedChunk}`
  }

  const endsWithPunct = /[.!?]$/.test(trimmedExisting)
  const endsWithComma = /[,;:]$/.test(trimmedExisting)
  const endsWithNewline = /\n$/.test(trimmedExisting)

  // Pause > 6s: New line paragraph
  if (pauseSeconds >= 6.0) {
    const punctPrefix = (!endsWithPunct && !endsWithNewline && !endsWithComma) ? '.' : ''
    const capitalizedChunk = trimmedChunk.charAt(0).toUpperCase() + trimmedChunk.slice(1)
    return `${trimmedExisting}${punctPrefix}\n\n${capitalizedChunk}`
  }

  // Pause 2s - 6s: Full stop + Capitalize
  if (pauseSeconds >= 2.0) {
    const capitalizedChunk = trimmedChunk.charAt(0).toUpperCase() + trimmedChunk.slice(1)
    if (endsWithPunct || endsWithNewline) {
      return `${trimmedExisting} ${capitalizedChunk}`
    }
    if (endsWithComma) {
      // Replace trailing comma with period
      return `${trimmedExisting.slice(0, -1)}. ${capitalizedChunk}`
    }
    return `${trimmedExisting}. ${capitalizedChunk}`
  }

  // Pause 0.75s - 2s: Comma
  if (pauseSeconds >= 0.75) {
    if (endsWithPunct || endsWithComma || endsWithNewline) {
      return `${trimmedExisting} ${trimmedChunk}`
    }
    return `${trimmedExisting}, ${trimmedChunk}`
  }

  // Normal continuous speech (pause < 0.75s)
  if (endsWithNewline || endsWithPunct || endsWithComma) {
    return `${trimmedExisting} ${trimmedChunk}`
  }
  return `${trimmedExisting} ${trimmedChunk}`
}

/**
 * Cleans, orders, and normalizes spoken clinical transcripts:
 * 1. Converts spoken punctuation commands ("full stop", "new line", "bullet point").
 * 2. Strips speech disfluencies and hesitation filler sounds ("um", "uh", "you know", "er").
 * 3. Removes repeated stuttered words ("the the" -> "the").
 * 4. Applies specialized veterinary domain acronym corrections (TPLO, PROM, UWTM, etc.).
 * 5. Cleans up spacing, capitalization, and sentence boundaries.
 *
 * @param rawTranscript The raw speech transcript from speech recognition
 * @returns Clean, structured, professional clinical text
 */
export function cleanSpeechTranscript(rawTranscript: string): string {
  if (!rawTranscript || typeof rawTranscript !== 'string') return ''

  // Check and strip stop commands first
  const { text: withoutStopCmds } = detectAndStripStopCommand(rawTranscript)
  let text = ' ' + withoutStopCmds.trim() + ' '

  // 1. Convert spoken punctuation commands
  for (const [spoken, punct] of Object.entries(SPOKEN_PUNCTUATION_MAP)) {
    const regex = new RegExp(spoken.replace(/[.*+?^${}()|[\]\\]/g, '\\$&'), 'gi')
    text = text.replace(regex, punct)
  }

  // 2. Strip filler words and hesitation sounds
  for (const pattern of FILLER_PATTERNS) {
    text = text.replace(pattern, ' ')
  }

  // 3. Remove repeated stuttered words (e.g., "was was", "the the", "Buddy Buddy")
  text = text.replace(/\b(\w+)\s+\1\b/gi, '$1')

  // 4. Apply specialized veterinary & rehabilitation terminology corrections
  text = correctVeterinaryTranscript(text)

  // 5. Clean up whitespace and punctuation spacing
  text = text
    .replace(/\s+([,.;:?!])/g, '$1') // remove spaces before punctuation
    .replace(/([,.;:?!])([a-zA-Z])/g, '$1 $2') // ensure space after punctuation
    .replace(/\s{2,}/g, ' ') // collapse multiple spaces into single
    .trim()

  return text
}
