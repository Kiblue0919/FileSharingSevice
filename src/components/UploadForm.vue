<template>
  <div class="upload-box">

    <input
      type="file"
      @change="handleFileChange"
    />

    <br /><br />

    <button
      @click="upload"
      :disabled="!selectedFile"
    >
      Upload
    </button>

    <div
      v-if="uploadedLink"
      class="result"
    >
      <h3>Upload Success</h3>

      <p>{{ uploadedLink }}</p>

      <button @click="copyLink">
        Copy Link
      </button>
    </div>

  </div>
</template>

<script setup>
import { ref } from 'vue'
import { uploadFile } from '../services/fileService'

const selectedFile = ref(null)
const uploadedLink = ref('')

const handleFileChange = (event) => {
  selectedFile.value = event.target.files[0]
}

const upload = async () => {
  if (!selectedFile.value) return

  const formData = new FormData()

  formData.append(
    'file',
    selectedFile.value
  )

  try {
    const response =
      await uploadFile(formData)

    uploadedLink.value =
      response.data.downloadUrl

  } catch (error) {
    console.error(error)
  }
}

const copyLink = async () => {
  await navigator.clipboard.writeText(
    uploadedLink.value
  )

  alert('Copied!')
}
</script>

<style scoped>
.upload-box {
  border: 1px solid #ccc;
  padding: 20px;
  border-radius: 10px;
}

.result {
  margin-top: 20px;
}
</style>