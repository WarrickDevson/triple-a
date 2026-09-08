import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { DocumentItem } from '../data/documentsDemo'
import { apiClient, API_BASE_URL } from '../api/client'

const STORAGE_KEY = 'triple-a-documents'

export const useDocumentsStore = defineStore('documents', () => {
  const documents = ref<DocumentItem[]>(loadInitialDocuments())
  const selectedDocument = ref<DocumentItem | null>(null)
  const isPreviewOpen = ref(false)
  const isUploadOpen = ref(false)
  const notificationMessage = ref<string | null>(null)

  function loadInitialDocuments(): DocumentItem[] {
    try {
      const stored = localStorage.getItem(STORAGE_KEY)
      if (stored) {
        return JSON.parse(stored) as DocumentItem[]
      }
    } catch {
      // Ignore parse failure
    }
    return []
  }

  function persistDocuments() {
    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(documents.value))
    } catch {
      // Ignore quota exceeded or storage error
    }
  }

  function showToast(message: string) {
    notificationMessage.value = message
    setTimeout(() => {
      if (notificationMessage.value === message) {
        notificationMessage.value = null
      }
    }, 4000)
  }

  function addDocument(newDoc: Omit<DocumentItem, 'id'>) {
    const item: DocumentItem = {
      ...newDoc,
      id: Date.now(),
    }
    documents.value = [item, ...documents.value]
    persistDocuments()
    showToast(`Document "${item.name}" uploaded successfully.`)
    return item
  }

  function deleteDocument(id: number) {
    const target = documents.value.find((d) => d.id === id)
    documents.value = documents.value.filter((d) => d.id !== id)
    persistDocuments()
    if (target) {
      showToast(`Document "${target.name}" removed.`)
    }
  }

  function toggleDocumentShare(id: number, share?: boolean) {
    const target = documents.value.find((d) => d.id === id)
    if (target) {
      target.isSharedWithOwner = share !== undefined ? share : !target.isSharedWithOwner
      target.sharedAt = target.isSharedWithOwner ? new Date().toISOString().slice(0, 10) : undefined
      persistDocuments()
      showToast(
        target.isSharedWithOwner
          ? `Document "${target.name}" is now shared with ${target.ownerName || 'the owner'}.`
          : `Document "${target.name}" is no longer shared.`,
      )
    }
  }

  function openPreview(doc: DocumentItem) {
    selectedDocument.value = doc
    isPreviewOpen.value = true
  }

  function closePreview() {
    isPreviewOpen.value = false
    selectedDocument.value = null
  }

  function openUpload() {
    isUploadOpen.value = true
  }

  function closeUpload() {
    isUploadOpen.value = false
  }

  function resolveFileUrl(url: string): string {
    if (!url) return ''
    if (url.startsWith('data:') || url.startsWith('blob:') || url.startsWith('http://') || url.startsWith('https://')) {
      return url
    }
    const base = API_BASE_URL.replace(/\/+$/, '')
    return `${base}${url.startsWith('/') ? url : `/${url}`}`
  }

  function triggerBrowserDownload(url: string, filename: string) {
    const a = document.createElement('a')
    a.href = url
    a.download = filename
    a.setAttribute('download', filename)
    document.body.appendChild(a)
    a.click()
    document.body.removeChild(a)
  }

  async function downloadDocument(doc: DocumentItem) {
    let filename = doc.name.includes('.')
      ? doc.name
      : `${doc.name}.${getFileExtension(doc.fileType)}`

    // Case 1: Data URL (local in-memory base64)
    if (doc.fileDataUrl && doc.fileDataUrl.startsWith('data:')) {
      triggerBrowserDownload(doc.fileDataUrl, filename)
      showToast(`Downloading "${filename}"...`)
      return
    }

    // Case 2: Server or Remote File URL
    if (doc.fileUrl || doc.fileDataUrl) {
      const rawUrl = doc.fileUrl || doc.fileDataUrl!
      const resolvedUrl = resolveFileUrl(rawUrl)

      showToast(`Preparing download for "${filename}"...`)
      try {
        const res = await apiClient.get<Blob>(resolvedUrl, { responseType: 'blob' })
        const blobUrl = URL.createObjectURL(res.data)
        triggerBrowserDownload(blobUrl, filename)
        setTimeout(() => URL.revokeObjectURL(blobUrl), 2000)
        showToast(`Downloaded "${filename}".`)
        return
      } catch (clientErr) {
        console.warn('apiClient blob download failed, attempting native fetch...', clientErr)
        try {
          const res = await fetch(resolvedUrl)
          if (!res.ok) throw new Error(`HTTP ${res.status}`)
          const blob = await res.blob()
          const blobUrl = URL.createObjectURL(blob)
          triggerBrowserDownload(blobUrl, filename)
          setTimeout(() => URL.revokeObjectURL(blobUrl), 2000)
          showToast(`Downloaded "${filename}".`)
          return
        } catch (fetchErr) {
          console.warn('Native fetch failed, opening in new tab as fallback...', fetchErr)
          window.open(resolvedUrl, '_blank')
          return
        }
      }
    }

    // Case 3: Demo item without attached binary file -> generate clinical document text
    const content = generateDemoDocumentContent(doc)
    const blob = new Blob([content], { type: 'text/plain;charset=utf-8' })
    const downloadUrl = URL.createObjectURL(blob)
    filename = `${doc.name.replace(/[^a-zA-Z0-9_-]/g, '_')}.txt`
    triggerBrowserDownload(downloadUrl, filename)
    setTimeout(() => URL.revokeObjectURL(downloadUrl), 2000)
    showToast(`Downloading "${filename}"...`)
  }

  function getFileExtension(fileType?: string): string {
    if (!fileType) return 'pdf'
    if (fileType.includes('png')) return 'png'
    if (fileType.includes('jpg') || fileType.includes('jpeg')) return 'jpg'
    if (fileType.includes('pdf')) return 'pdf'
    if (fileType.includes('text') || fileType.includes('plain')) return 'txt'
    if (fileType.includes('word') || fileType.includes('document')) return 'docx'
    return 'pdf'
  }

  function generateDemoDocumentContent(doc: DocumentItem): string {
    return `=====================================================
TRIPLE A VETERINARY PHYSIOTHERAPY CLINIC
CLINICAL DOCUMENT: ${doc.name.toUpperCase()}
=====================================================

Document ID:      #DOC-${doc.id.toString().padStart(4, '0')}
Category:         ${doc.category}
Patient:          ${doc.petName}
Owner:            ${doc.ownerName}
Uploaded Date:    ${doc.uploadedAt}
File Size:        ${doc.sizeKb} KB

-----------------------------------------------------
DOCUMENT DETAILS & CLINICAL SUMMARY
-----------------------------------------------------

Patient Name:     ${doc.petName}
Owner Name:       ${doc.ownerName}
Category:         ${doc.category}

Summary:
This document represents an official veterinary rehabilitation record for ${doc.petName}.
It contains verified clinical assessment notes, owner consent documentation, or treatment protocol files recorded by the attending physiotherapist.

For further information or questions regarding this document, please contact Triple A Physiotherapy Clinic.

-----------------------------------------------------
End of Document — Triple A Veterinary Rehabilitation Portal
=====================================================
`
  }

  return {
    documents,
    selectedDocument,
    isPreviewOpen,
    isUploadOpen,
    notificationMessage,
    addDocument,
    deleteDocument,
    toggleDocumentShare,
    openPreview,
    closePreview,
    openUpload,
    closeUpload,
    downloadDocument,
    showToast,
  }
})
