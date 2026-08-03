import axios from 'axios'

const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL || 'https://b-e.up.railway.app'

const api = axios.create({
  baseURL: `${API_BASE_URL}/api/files`
})

export const uploadFile = (formData, onUploadProgress) => {
  return api.post('/', formData, {
    onUploadProgress
  })
}

export const getFileMetadata = (code) => {
  return api.get(`/${code}`)
}

export const downloadFile = (code) => {
  window.open(`${API_BASE_URL}/api/files/${code}/download`, '_blank')
}

export const deleteFile = (code) => {
  return api.delete(`/${code}`)
}

export const listFiles = () => {
  return api.get('/')
}

export const updateFile = (code, data) => {
  return api.put(`/${code}`, data)
}
