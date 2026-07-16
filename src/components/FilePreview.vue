<template>
  <div class="preview" v-if="src">
    <img :src="src" alt="preview" />
  </div>
</template>

<script setup>
import { ref, watch } from 'vue'
const props = defineProps({ file: { type: [Object, String], default: null } })
const src = ref(null)

const toDataUrl = (file) => new Promise((resolve, reject) => {
  if (!file) return resolve(null)
  if (typeof file === 'string') return resolve(file)
  const reader = new FileReader()
  reader.onload = () => resolve(reader.result)
  reader.onerror = reject
  reader.readAsDataURL(file)
})

watch(() => props.file, async (f) => {
  src.value = await toDataUrl(f)
}, { immediate: true })
</script>

<style scoped>
.preview img { max-width: 200px; max-height: 200px; display:block; margin-top:10px; }
</style>
