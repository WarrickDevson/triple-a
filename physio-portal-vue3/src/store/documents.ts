import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { DocumentItem } from '../data/documentsDemo'

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

  function downloadDocument(doc: DocumentItem) {
    let downloadUrl: string
    let filename: string

    if (doc.fileDataUrl || doc.fileUrl) {
      downloadUrl = doc.fileDataUrl || doc.fileUrl!
      filename = doc.name.includes('.') ? doc.name : `${doc.name}.${getFileExtension(doc.fileType)}`
    } else {
      // Generate sample clinical document blob for demo items
      const content = generateDemoDocumentContent(doc)
      const blob = new Blob([content], { type: 'text/plain;charset=utf-8' })
      downloadUrl = URL.createObjectURL(blob)
      filename = `${doc.name.replace(/[^a-zA-Z0-9_-]/g, '_')}.txt`
    }

    const a = document.createElement('a')
    a.href = downloadUrl
    a.download = filename
    document.body.appendChild(a)
    a.click()
    document.body.removeChild(a)

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
