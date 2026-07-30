<template>
	<div class="history-view">
		<div class="page-header">
			<h1>Uploaded Files</h1>
			<p class="subtitle">Manage your shared files</p>
		</div>

		<div class="content">
			<div v-if="loading" class="loading-state">
				<div class="spinner"></div>
				<p>Loading files...</p>
			</div>
			
			<div v-else>
				<table v-if="files.length" class="files-table">
					<thead>
						<tr>
							<th class="thumb">Preview</th>
							<th class="code">Code</th>
							<th class="name">Name</th>
							<th class="size">Size</th>
							<th class="date">Date</th>
							<th class="actions">Actions</th>
						</tr>
					</thead>
					<tbody>
						<tr v-for="f in files" :key="f.code" class="file-row">
							<td class="thumb">
								<img v-if="f.content" :src="f.content" alt="thumb" class="thumbnail" />
								<div v-else class="no-thumb">📄</div>
							</td>
							<td class="code">
								<code>{{ f.code }}</code>
							</td>
							<td class="name">
								<span v-if="editCode !== f.code" class="filename">{{ f.originalFileName }}</span>
								<input v-else v-model="editName" class="edit-input" />
							</td>
							<td class="size">{{ formatBytes(f.sizeBytes) }}</td>
							<td class="date">{{ f.createdAt }}</td>
							<td class="actions">
								<router-link :to="`/f/${f.code}`" class="action-btn view-btn">View</router-link>
								<button v-if="editCode !== f.code" @click="startEdit(f)" class="action-btn edit-btn">Edit</button>
								<button v-else @click="saveEdit(f)" class="action-btn save-btn">Save</button>
								<button @click="remove(f.code)" class="action-btn delete-btn">Delete</button>
							</td>
						</tr>
					</tbody>
				</table>

				<div v-else class="empty-state">
					<div class="empty-icon">📁</div>
					<h3>No files uploaded yet</h3>
					<p>Start by uploading a file to get a shareable link</p>
					<router-link to="/" class="empty-btn">Upload First File</router-link>
				</div>
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
.history-view {
	padding: 40px 20px;
	min-height: calc(100vh - 70px);
}

.page-header {
	max-width: 1200px;
	margin: 0 auto 40px;
	color: white;
	text-align: center;
}

.page-header h1 {
	font-size: 36px;
	font-weight: 700;
	margin-bottom: 10px;
}

.subtitle {
	font-size: 16px;
	opacity: 0.9;
	margin: 0;
}

.content {
	max-width: 1200px;
	margin: 0 auto;
	background: white;
	border-radius: 12px;
	padding: 30px;
	box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
}

.loading-state {
	text-align: center;
	padding: 60px 20px;
	display: flex;
	flex-direction: column;
	align-items: center;
	justify-content: center;
}

.spinner {
	width: 50px;
	height: 50px;
	border: 4px solid #e0e0e0;
	border-top: 4px solid #4a90e2;
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

.loading-state p {
	font-size: 16px;
	color: #666;
	margin: 0;
}

.files-table {
	width: 100%;
	border-collapse: collapse;
	margin: 0;
}

.files-table thead {
	background: #f8f9fa;
	border-bottom: 2px solid #ddd;
}

.files-table th {
	padding: 15px;
	text-align: left;
	font-weight: 600;
	color: #333;
	font-size: 14px;
	text-transform: uppercase;
	letter-spacing: 0.5px;
}

.files-table th.thumb {
	width: 80px;
}

.files-table th.code {
	width: 100px;
}

.files-table th.size {
	width: 100px;
}

.files-table th.date {
	width: 120px;
}

.files-table th.actions {
	width: 250px;
}

.files-table tbody tr {
	border-bottom: 1px solid #eee;
	transition: background-color 0.2s ease;
}

.files-table tbody tr:hover {
	background-color: #f8fbff;
}

.files-table td {
	padding: 15px;
	font-size: 14px;
	color: #333;
}

.thumbnail {
	width: 60px;
	height: 60px;
	object-fit: cover;
	border-radius: 6px;
	display: block;
}

.no-thumb {
	width: 60px;
	height: 60px;
	background: #f0f0f0;
	border-radius: 6px;
	display: flex;
	align-items: center;
	justify-content: center;
	font-size: 24px;
}

.filename {
	word-break: break-word;
	font-weight: 500;
	color: #333;
}

code {
	background: #f5f5f5;
	padding: 4px 8px;
	border-radius: 4px;
	font-family: 'Courier New', monospace;
	font-size: 12px;
	color: #666;
}

.edit-input {
	padding: 6px 10px;
	border: 1px solid #ddd;
	border-radius: 4px;
	font-size: 14px;
	width: 100%;
	box-sizing: border-box;
}

.edit-input:focus {
	outline: none;
	border-color: #4a90e2;
	box-shadow: 0 0 0 3px rgba(74, 144, 226, 0.1);
}

.action-btn {
	display: inline-block;
	padding: 6px 12px;
	margin-right: 8px;
	border: none;
	border-radius: 4px;
	font-size: 12px;
	font-weight: 600;
	cursor: pointer;
	text-decoration: none;
	transition: all 0.2s ease;
	text-align: center;
}

.view-btn {
	background: #4a90e2;
	color: white;
}

.view-btn:hover {
	background: #357abd;
	transform: translateY(-2px);
}

.edit-btn {
	background: #f0f0f0;
	color: #333;
	border: 1px solid #ddd;
}

.edit-btn:hover {
	background: #e0e0e0;
}

.save-btn {
	background: #28a745;
	color: white;
}

.save-btn:hover {
	background: #218838;
	transform: translateY(-2px);
}

.delete-btn {
	background: #dc3545;
	color: white;
}

.delete-btn:hover {
	background: #c82333;
	transform: translateY(-2px);
}

.empty-state {
	text-align: center;
	padding: 60px 20px;
}

.empty-icon {
	font-size: 64px;
	margin-bottom: 20px;
	display: block;
}

.empty-state h3 {
	font-size: 24px;
	color: #333;
	margin: 0 0 10px;
}

.empty-state p {
	font-size: 16px;
	color: #888;
	margin: 0 0 30px;
}

.empty-btn {
	display: inline-block;
	padding: 12px 30px;
	background: #4a90e2;
	color: white;
	border-radius: 6px;
	text-decoration: none;
	font-weight: 600;
	transition: all 0.3s ease;
}

.empty-btn:hover {
	background: #357abd;
	transform: translateY(-2px);
	box-shadow: 0 4px 12px rgba(74, 144, 226, 0.3);
}

@media (max-width: 768px) {
	.content {
		padding: 20px;
	}

	.files-table th,
	.files-table td {
		padding: 10px;
		font-size: 12px;
	}

	.files-table th.thumb,
	.files-table th.code,
	.files-table th.size,
	.files-table th.date {
		width: auto;
	}

	.action-btn {
		display: block;
		margin-bottom: 5px;
		margin-right: 0;
		padding: 5px 10px;
		font-size: 11px;
	}

	.action-btn:last-child {
		margin-bottom: 0;
	}
}
</style>

