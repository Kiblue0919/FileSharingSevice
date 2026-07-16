<template>
  <div class="container">
    <h1>File Sharing Service</h1>

      <div style="margin-bottom:16px">
        <router-link to="/history">View Uploaded Files</router-link>
      </div>

      <UploadForm />
  </div>
</template>

<script setup>
import UploadForm from '../components/UploadForm.vue'
</script>

<style scoped>
.container {
  max-width: 800px;
  margin: 50px auto;
  text-align: center;
}
</style>