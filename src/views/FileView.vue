<template>

  <div class="container">

    <!-- Loading -->
    <div v-if="loading">
      Loading...
    </div>

    <!-- Error -->
    <div v-else-if="error">
      Something went wrong.
    </div>

    <!-- Not Found -->
    <div v-else-if="notFound">
      File not found.
    </div>

    <!-- Success -->
    <div v-else>

      <div v-if="file.content && file.type && file.type.startsWith('image/')">
        <img :src="file.content" alt="preview" style="max-width:100%; margin-bottom:12px" />
      </div>

      <h2>{{ file.originalFileName }}</h2>

      <p>
        Size:
        {{ formatBytes(file.sizeBytes) }}
      </p>

      <p>
        Upload Date:
        {{ file.createdAt }}
      </p>

      <button
        @click="handleDownload"
      >
        Download
      </button>

    </div>

  </div>

</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'

import {
  getFileMetadata,
  downloadFile
} from '../services/fileService'

const route = useRoute()

const loading = ref(true)
const error = ref(false)
const notFound = ref(false)

const file = ref({})

const formatBytes = (bytes) => {
  if (!bytes) return '0 B'

  const kb = bytes / 1024

  if (kb < 1024)
    return kb.toFixed(2) + ' KB'

  return (kb / 1024).toFixed(2) + ' MB'
}

const handleDownload = () => {
  downloadFile(route.params.code)
}

onMounted(async () => {

  try {

    const response =
      await getFileMetadata(
        route.params.code
      )

    file.value = response.data

  } catch (err) {

    if (
      err.response &&
      err.response.status === 404
    ) {
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
.container {
  max-width: 700px;
  margin: 50px auto;
}
</style>