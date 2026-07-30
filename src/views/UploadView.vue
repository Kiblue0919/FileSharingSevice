<template>
  <div class="upload-view">
    <div class="hero-section">
      <h1 class="hero-title">Send super big files</h1>
      <p class="hero-subtitle">Simple. Fast. Beautiful.</p>
    </div>
    
    <div class="upload-container">
      <UploadForm />
    </div>
  </div>
</template>

<script setup>
import UploadForm from '../components/UploadForm.vue'
</script>

<style scoped>
.upload-view {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 40px 20px;
  min-height: calc(100vh - 70px);
}

.hero-section {
  text-align: center;
  margin-bottom: 60px;
  animation: fadeInDown 0.6s ease;
}

.hero-title {
  font-size: 48px;
  font-weight: 700;
  margin: 0 0 15px 0;
  letter-spacing: -0.5px;
  color: white;
}

.hero-subtitle {
  font-size: 20px;
  font-weight: 300;
  margin: 0;
  opacity: 0.95;
  color: white;
}

.upload-container {
  width: 100%;
  max-width: 600px;
  animation: fadeInUp 0.6s ease;
}

@keyframes fadeInDown {
  from {
    opacity: 0;
    transform: translateY(-20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@keyframes fadeInUp {
  from {
    opacity: 0;
    transform: translateY(20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@media (max-width: 768px) {
  .hero-title {
    font-size: 32px;
  }

  .hero-subtitle {
    font-size: 16px;
  }
}
</style>