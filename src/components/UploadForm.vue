<template>
  <div class="upload-card">
    <!-- Upload State -->
    <div v-if="!uploadedLink && !uploading">
      <div
        class="dropzone"
        :class="{ 'drag-over': isDragging }"
        @dragover.prevent="isDragging = true"
        @dragleave.prevent="isDragging = false"
        @drop.prevent="handleDrop"
        @click="triggerFileInput"
      >
        <input
          ref="fileInput"
          type="file"
          @change="handleFileChange"
          class="file-input"
        />
        
        <div class="upload-icon">☁️</div>
        <h3 class="dropzone-text">Drop it like it's hot!</h3>
        <p class="sub-text">Max file size: 10 MB</p>
      </div>

      <div v-if="selectedFile" class="preview-card">
        <div class="preview-media">
          <img v-if="previewUrl" :src="previewUrl" alt="File preview" class="preview-image" />
          <div v-else class="preview-fallback">{{ fileInitial }}</div>
        </div>

        <div class="preview-details">
          <p class="preview-name">{{ selectedFile.name }}</p>
          <p class="preview-meta">{{ formatFileSize(selectedFile.size) }}</p>
        </div>
      </div>

      <button
        v-if="selectedFile"
        @click="upload"
        class="upload-btn"
      >
        🚀 Upload File
      </button>
    </div>

    <!-- Uploading State -->
    <div v-if="uploading" class="uploading-state">
      <div class="spinner"></div>
      <p class="upload-progress">Uploading... {{ uploadProgress }}%</p>
    </div>

    <!-- Success State -->
    <div v-if="uploadedLink" class="success-state">
      <div class="success-icon">✨</div>
      <h3 class="success-title">Upload Success!</h3>
      <p class="success-subtitle">Your file is ready to share</p>
      
      <div class="link-box">
        <p class="link-label">📋 Your Download Link:</p>
        <div class="link-wrapper">
          <input
            type="text"
            :value="uploadedLink"
            readonly
            class="link-input"
          />
          <button @click="copyLink" class="copy-btn">
            📋 Copy
          </button>
        </div>
      </div>

      <button @click="resetForm" class="upload-another-btn">
        ➕ Upload Another File
      </button>
    </div>
  </div>
</template>

<script setup>
import { computed, ref } from 'vue'
import { uploadFile } from '../services/fileService'

const selectedFile = ref(null)
const previewUrl = ref('')
const uploadedLink = ref('')
const isDragging = ref(false)
const uploading = ref(false)
const uploadProgress = ref(0)
const fileInput = ref(null)

const formatFileSize = (size) => {
  if (!size) return '0 B'
  const mb = size / 1024 / 1024
  if (mb >= 1) return `${mb.toFixed(2)} MB`
  return `${(size / 1024).toFixed(2)} KB`
}

const fileInitial = computed(() => {
  if (!selectedFile.value?.name) return ''
  return selectedFile.value.name.charAt(0).toUpperCase()
})

const clearPreviewUrl = () => {
  if (previewUrl.value) {
    URL.revokeObjectURL(previewUrl.value)
    previewUrl.value = ''
  }
}

const triggerFileInput = () => {
  if (fileInput.value) {
    fileInput.value.click()
  }
}

const handleFileChange = (event) => {
  const files = event.target.files
  if (files && files.length > 0) {
    validateAndSetFile(files[0])
  }
}

const handleDrop = (event) => {
  isDragging.value = false
  const files = event.dataTransfer.files
  if (files && files.length > 0) {
    validateAndSetFile(files[0])
  }
}

const validateAndSetFile = (file) => {
  if (file.size > 10 * 1024 * 1024) {
    alert('File exceeds the maximum allowed size of 10 MB.')
    return
  }
  clearPreviewUrl()
  if (file.type && file.type.startsWith('image/')) {
    previewUrl.value = URL.createObjectURL(file)
  }
  selectedFile.value = file
}

const upload = async () => {
  if (!selectedFile.value) return

  uploading.value = true
  uploadProgress.value = 0

  const formData = new FormData()
  formData.append('file', selectedFile.value)

  try {
    const response = await uploadFile(formData)
    uploadedLink.value = response.data.downloadUrl
    uploadProgress.value = 100
  } catch (error) {
    console.error('Upload error:', error)
    const errorMsg = error.response?.data?.message || 'Upload failed. Please try again.'
    alert(errorMsg)
  } finally {
    uploading.value = false
  }
}

const copyLink = async () => {
  if (!uploadedLink.value) return
  try {
    await navigator.clipboard.writeText(uploadedLink.value)
    alert('Copied to clipboard!')
  } catch (err) {
    console.error('Failed to copy: ', err)
  }
}

const resetForm = () => {
  selectedFile.value = null
  clearPreviewUrl()
  uploadedLink.value = ''
  uploading.value = false
  uploadProgress.value = 0
  if (fileInput.value) {
    fileInput.value.value = ''
  }
}
</script>

<style scoped>
.upload-card {
  width: 100%;
  max-width: 500px;
  margin: 0 auto;
}

.dropzone {
  background: white;
  border: 3px dashed #cbd5e1;
  border-radius: 16px;
  padding: 50px 20px;
  text-align: center;
  cursor: pointer;
  transition: all 0.3s ease;
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.05);
}

.dropzone:hover {
  border-color: #1d8dd8;
  background: #f8fafc;
  transform: translateY(-2px);
}

.dropzone.drag-over {
  border-color: #1d8dd8;
  background: #eff6ff;
  transform: scale(1.02);
}

.file-input {
  display: none;
}

.upload-icon {
  font-size: 56px;
  margin-bottom: 12px;
}

.preview-card {
  margin-top: 14px;
  background: rgba(255, 255, 255, 0.96);
  border: 1px solid #dbeafe;
  border-radius: 14px;
  padding: 14px;
  display: flex;
  align-items: center;
  gap: 12px;
  box-shadow: 0 8px 20px rgba(15, 23, 42, 0.08);
}

.preview-media {
  width: 72px;
  height: 72px;
  flex: 0 0 72px;
  border-radius: 12px;
  overflow: hidden;
  background: #eff6ff;
  display: flex;
  align-items: center;
  justify-content: center;
}

.preview-image {
  width: 100%;
  height: 100%;
  object-fit: cover;
  display: block;
}

.preview-fallback {
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #1d8dd8;
  font-size: 28px;
  font-weight: 700;
}

.preview-details {
  min-width: 0;
  text-align: left;
}

.preview-name {
  margin: 0;
  color: #0f172a;
  font-weight: 700;
  font-size: 14px;
  word-break: break-word;
}

.preview-meta {
  margin: 6px 0 0;
  color: #64748b;
  font-size: 12px;
}

.dropzone-text {
  font-size: 20px;
  font-weight: 600;
  color: #1e293b;
  margin: 0 0 8px 0;
  word-break: break-all;
}

.dropzone-text.selected {
  color: #1d8dd8;
}

.sub-text {
  font-size: 14px;
  color: #64748b;
  margin: 0;
}

.upload-btn {
  width: 100%;
  padding: 14px 24px;
  margin-top: 16px;
  background: #1d8dd8;
  color: white;
  border: none;
  border-radius: 10px;
  font-size: 16px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s ease;
}

.upload-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(37, 99, 235, 0.2);
}

.uploading-state {
  background: white;
  border-radius: 16px;
  padding: 50px 20px;
  text-align: center;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.05);
}

.spinner {
  width: 48px;
  height: 48px;
  border: 4px solid #e2e8f0;
  border-top: 4px solid #1d8dd8;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin-bottom: 16px;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.upload-progress {
  font-size: 16px;
  color: #475569;
  font-weight: 500;
  margin: 0;
}

.success-state {
  background: white;
  border-radius: 16px;
  padding: 40px 30px;
  text-align: center;
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.05);
  animation: fadeIn 0.3s ease;
}

@keyframes fadeIn {
  from { opacity: 0; transform: translateY(10px); }
  to { opacity: 1; transform: translateY(0); }
}

.success-icon {
  font-size: 48px;
  margin-bottom: 12px;
}

.success-title {
  color: #0f172a;
  margin: 0 0 6px 0;
  font-size: 22px;
}

.success-subtitle {
  color: #64748b;
  font-size: 14px;
  margin: 0 0 20px 0;
}

.link-box {
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 10px;
  padding: 16px;
  margin-bottom: 20px;
  text-align: left;
}

.link-label {
  font-size: 12px;
  color: #64748b;
  font-weight: 600;
  margin: 0 0 8px 0;
}

.link-wrapper {
  display: flex;
  gap: 8px;
}

.link-input {
  flex: 1;
  padding: 10px 12px;
  border: 1px solid #cbd5e1;
  border-radius: 6px;
  font-size: 13px;
  background: white;
  color: #1e293b;
  outline: none;
}

.copy-btn {
  padding: 10px 16px;
  background: #1d8dd8;
  color: white;
  border: none;
  border-radius: 6px;
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
  white-space: nowrap;
  transition: background 0.2s;
}

.copy-btn:hover {
  background: #1d8dd8;
}

.upload-another-btn {
  width: 100%;
  padding: 12px 20px;
  background: #f1f5f9;
  color: #334155;
  border: 1px solid #cbd5e1;
  border-radius: 8px;
  font-size: 14px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
}

.upload-another-btn:hover {
  background: #e2e8f0;
}
</style>