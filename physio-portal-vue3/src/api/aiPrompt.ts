import { apiClient } from './client'

export interface AiPromptConfig {
  systemPrompt: string
  defaultSystemPrompt: string
  isCustomized: boolean
  lastModifiedAt: string | null
  lastModifiedBy: string | null
}

export async function fetchAiPromptConfig(): Promise<AiPromptConfig> {
  const res = await apiClient.get<AiPromptConfig>('/api/ai/prompt-config')
  return res.data
}

export async function updateAiPromptConfig(systemPrompt: string): Promise<AiPromptConfig> {
  const res = await apiClient.put<AiPromptConfig>('/api/ai/prompt-config', { systemPrompt })
  return res.data
}

export async function resetAiPromptConfig(): Promise<AiPromptConfig> {
  const res = await apiClient.post<AiPromptConfig>('/api/ai/prompt-config/reset')
  return res.data
}
