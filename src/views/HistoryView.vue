<template>
	<div class="container">
		<h1>Uploaded Files</h1>

		<div v-if="loading">Loading...</div>
		<div v-else>
			<table v-if="files.length" class="files">
				<thead>
					<tr>
						<th></th>
						<th>Code</th>
						<th>Name</th>
						<th>Size</th>
						<th>Date</th>
						<th>Actions</th>
					</tr>
				</thead>
				<tbody>
					<tr v-for="f in files" :key="f.code">
						<td>
							<img v-if="f.content" :src="f.content" alt="thumb" style="width:80px; height:auto" />
						</td>
						<td>{{ f.code }}</td>
						<td>
							<span v-if="editCode !== f.code">{{ f.originalFileName }}</span>
							<input v-else v-model="editName" />
						</td>
						<td>{{ formatBytes(f.sizeBytes) }}</td>
						<td>{{ f.createdAt }}</td>
						<td>
							<router-link :to="`/f/${f.code}`">View</router-link>
							<button v-if="editCode !== f.code" @click="startEdit(f)">Edit</button>
							<button v-else @click="saveEdit(f)">Save</button>
							<button @click="remove(f.code)">Delete</button>
						</td>
					</tr>
				</tbody>
			</table>

			<div v-else>
				No files uploaded yet.
			</div>
		</div>
	</div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { listFiles, updateFile, deleteFile } from '../services/fileService'

const files = ref([])
const loading = ref(true)

const editCode = ref(null)
const editName = ref('')

const formatBytes = (bytes) => {
	if (!bytes) return '0 B'
	const kb = bytes / 1024
	if (kb < 1024) return kb.toFixed(2) + ' KB'
	return (kb / 1024).toFixed(2) + ' MB'
}

const load = async () => {
	loading.value = true
	const res = await listFiles()
	files.value = res.data
	loading.value = false
}

const startEdit = (f) => {
	editCode.value = f.code
	editName.value = f.originalFileName
}

const saveEdit = async (f) => {
	await updateFile(f.code, { originalFileName: editName.value })
	editCode.value = null
	editName.value = ''
	await load()
}

const remove = async (code) => {
	if (!confirm('Delete this file?')) return
	await deleteFile(code)
	await load()
}

onMounted(load)
</script>

<style scoped>
.container { max-width: 900px; margin: 40px auto; }
.files { width: 100%; border-collapse: collapse; }
.files th, .files td { border: 1px solid #ddd; padding: 8px; }
.files th { background: #f5f5f5; }
</style>

