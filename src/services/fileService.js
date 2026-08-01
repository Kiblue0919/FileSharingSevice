import axios from 'axios'

const api = axios.create({
  baseURL: 'http://localhost:5117/api/files'
})

export const uploadFile = (formData, onUploadProgress) => {
  return api.post('/', formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
    onUploadProgress
  })
}

export const getFileMetadata = (code) => {
  return api.get(`/${code}`)
}

export const downloadFile = (code) => {
  window.open(`http://localhost:5117/api/files/${code}/download`, '_blank')
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