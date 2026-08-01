<template>
  <div class="file-view">
    <div v-if="loading" class="state loading-state">
      <div class="spinner"></div>
      <p>Loading file...</p>
    </div>

    <div v-else-if="error" class="state error-state">
      <div class="error-icon">⚠️</div>
      <h2>Something went wrong</h2>
      <p>We encountered an error while fetching the file.</p>
      <router-link to="/" class="state-btn">Go Home</router-link>
    </div>

    <div v-else-if="notFound" class="state not-found-state">
      <div class="error-icon">🔍</div>
      <h2>File not found</h2>
      <p>The file you're looking for doesn't exist or has been deleted.</p>
      <router-link to="/" class="state-btn">Go Home</router-link>
    </div>

    <div v-else class="file-container">
      <div class="file-header">
        <div>
          <p class="eyebrow">File Details</p>
          <h1>{{ file.originalFileName }}</h1>
        </div>
        <button @click="handleDownload" class="download-btn">
          Download File
        </button>
      </div>

      <div class="file-preview">
        <img
          v-if="file.fileUrl && file.isImage"
          :src="file.fileUrl"
          alt="preview"
          class="preview-image"
        />
        <div v-else class="no-preview">
          <div class="no-preview-icon">📄</div>
          <p>No preview available</p>
        </div>
      </div>

      <div class="file-info">
        <div class="info-item">
          <span class="info-label">File Size</span>
          <span class="info-value">{{ formatBytes(file.sizeBytes) }}</span>
        </div>
        <div class="info-item">
          <span class="info-label">Uploaded Date</span>
          <span class="info-value">{{ file.createdAt }}</span>
        </div>
        <div class="info-item">
          <span class="info-label">File Code</span>
          <span class="info-value code">{{ route.params.code }}</span>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { getFileMetadata, downloadFile } from '../services/fileService'

const route = useRoute()

const loading = ref(true)
const error = ref(false)
const notFound = ref(false)
const file = ref({})

const formatBytes = (bytes) => {
  if (!bytes) return '0 B'
  const kb = bytes / 1024
  if (kb < 1024) return `${kb.toFixed(2)} KB`
  return `${(kb / 1024).toFixed(2)} MB`
}

const handleDownload = () => {
  downloadFile(route.params.code)
}

onMounted(async () => {
  try {
    const response = await getFileMetadata(route.params.code)
    file.value = response.data
  } catch (err) {
    if (err.response && err.response.status === 404) {
      notFound.value = true
    } else {
      error.value = true
    }
  } finally {
    loading.value = false
  }
})
</script>

<style scoped>
.file-view {
  padding: 40px 20px;
  min-height: calc(100vh - 70px);
}

.state {
  min-height: calc(100vh - 150px);
  text-align: center;
  padding: 80px 20px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  color: white;
}

.spinner {
  width: 60px;
  height: 60px;
  border: 4px solid rgba(255, 255, 255, 0.22);
  border-top: 4px solid white;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin-bottom: 20px;
}

@keyframes spin {
  0% {
    transform: rotate(0deg);
  }
  100% {
    transform: rotate(360deg);
  }
}

.error-icon {
  font-size: 64px;
  margin-bottom: 20px;
}

.state h2 {
  font-size: 28px;
  margin-bottom: 10px;
}

.state p {
  font-size: 16px;
  opacity: 0.9;
  margin-bottom: 30px;
  max-width: 420px;
}

.state-btn {
  display: inline-block;
  padding: 12px 30px;
  background: white;
  color: #2b7fff;
  border-radius: 10px;
  text-decoration: none;
  font-weight: 600;
  transition: all 0.3s ease;
}

.state-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 20px rgba(0, 0, 0, 0.18);
}

.file-container {
  max-width: 900px;
  margin: 0 auto;
  background: white;
  border-radius: 24px;
  padding: 36px;
  box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1);
  animation: slideUp 0.4s ease;
}

@keyframes slideUp {
  from {
    opacity: 0;
    transform: translateY(20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.file-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 28px;
  padding-bottom: 18px;
  border-bottom: 1px solid #e5e7eb;
  gap: 16px;
  flex-wrap: wrap;
}

.eyebrow {
  color: #2b7fff;
  text-transform: uppercase;
  letter-spacing: 0.12em;
  font-size: 12px;
  font-weight: 700;
  margin-bottom: 8px;
}

.file-header h1 {
  font-size: 28px;
  color: #111827;
  margin: 0;
  flex: 1;
  min-width: 250px;
  word-break: break-word;
}

.download-btn {
  padding: 12px 28px;
  background: linear-gradient(135deg, #2b7fff 0%, #1e40af 100%);
  color: white;
  border: none;
  border-radius: 12px;
  font-size: 15px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
}

.download-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 18px rgba(43, 127, 255, 0.28);
}

.file-preview {
  margin-bottom: 24px;
  background: #f8fafc;
  border: 1px solid #e5e7eb;
  border-radius: 18px;
  overflow: hidden;
  min-height: 220px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.preview-image {
  width: 100%;
  height: auto;
  max-height: 520px;
  object-fit: contain;
  display: block;
}

.no-preview {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  width: 100%;
  padding: 44px 20px;
  color: #94a3b8;
}

.no-preview-icon {
  font-size: 48px;
  margin-bottom: 10px;
}

.no-preview p {
  margin: 0;
  font-size: 14px;
}

.file-info {
  background: #f8fafc;
  border: 1px solid #e5e7eb;
  border-radius: 18px;
  padding: 20px;
}

.info-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 14px 0;
  border-bottom: 1px solid #e5e7eb;
  gap: 12px;
}

.info-item:last-child {
  border-bottom: none;
}

.info-label {
  font-weight: 600;
  color: #64748b;
  font-size: 13px;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.info-value {
  color: #111827;
  font-size: 14px;
  word-break: break-word;
  text-align: right;
}

.info-value.code {
  font-family: 'Courier New', monospace;
  background: white;
  padding: 4px 8px;
  border-radius: 8px;
  font-size: 12px;
  color: #4b5563;
  border: 1px solid #e5e7eb;
}

@media (max-width: 768px) {
  .file-container {
    padding: 20px;
    border-radius: 18px;
  }

  .file-header {
    flex-direction: column;
    align-items: flex-start;
  }

  .file-header h1 {
    font-size: 22px;
  }

  .download-btn {
    width: 100%;
    text-align: center;
  }

  .info-item {
    flex-direction: column;
    align-items: flex-start;
  }

  .info-value {
    text-align: left;
  }
}
</style>
*** End Patch

    .no-preview p {
      margin: 0;
      font-size: 14px;
    }

    .file-info {
      background: #f8fafc;
      border: 1px solid #e5e7eb;
      border-radius: 18px;
      padding: 20px;
    }

    .info-item {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 14px 0;
      border-bottom: 1px solid #e5e7eb;
      gap: 12px;
    }

    .info-item:last-child {
      border-bottom: none;
    }

    .info-label {
      font-weight: 600;
      color: #64748b;
      font-size: 13px;
      text-transform: uppercase;
      letter-spacing: 0.5px;
    }

    .info-value {
      color: #111827;
      font-size: 14px;
      word-break: break-word;
      text-align: right;
    }

    .info-value.code {
      font-family: 'Courier New', monospace;
      background: white;
      padding: 4px 8px;
      border-radius: 8px;
      font-size: 12px;
      color: #4b5563;
      border: 1px solid #e5e7eb;
    }

    @media (max-width: 768px) {
      .file-container {
        padding: 20px;
        border-radius: 18px;
      }

      .file-header {
        flex-direction: column;
        align-items: flex-start;
      }

      .file-header h1 {
        font-size: 22px;
      }

      .download-btn {
        width: 100%;
        text-align: center;
      }

      .info-item {
        flex-direction: column;
        align-items: flex-start;
      }

      .info-value {
        text-align: left;
      }
    }
  border-radius: 8px;
  overflow: hidden;
  min-height: 200px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.preview-image {
  width: 100%;
  height: auto;
  max-height: 500px;
  object-fit: contain;
  display: block;
}

.no-preview {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  width: 100%;
  padding: 40px 20px;
  color: #999;
}

.no-preview-icon {
  font-size: 48px;
  margin-bottom: 10px;
}

.no-preview p {
  margin: 0;
  font-size: 14px;
}

.file-info {
  background: #f8f9fa;
  border-radius: 8px;
  padding: 20px;
}

.info-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px 0;
  border-bottom: 1px solid #e0e0e0;
}

.info-item:last-child {
  border-bottom: none;
}

.info-label {
  font-weight: 600;
  color: #666;
  font-size: 14px;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.info-value {
  color: #333;
  font-size: 14px;
  word-break: break-word;
}

.info-value.code {
  font-family: 'Courier New', monospace;
  background: white;
  padding: 4px 8px;
  border-radius: 4px;
  font-size: 12px;
  color: #666;
}

@media (max-width: 768px) {
  .file-container {
    padding: 20px;
  }

  .file-header {
    flex-direction: column;
    align-items: flex-start;
  }

  .file-header h1 {
    font-size: 20px;
  }

  .download-btn {
    width: 100%;
    text-align: center;
  }

  .info-item {
    flex-direction: column;
    align-items: flex-start;
    gap: 8px;
  }

  .state {
    padding: 40px 20px;
  }
