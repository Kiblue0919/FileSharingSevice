import axios from 'axios'

const api = axios.create({
  baseURL: 'http://localhost:5000/api/files'
})

export const getFileMetadata = () => {

  return Promise.resolve({
    data: {
      originalFileName: 'cat.jpg',
      sizeBytes: 125000,
      createdAt: '2026-06-18'
    }
  })

}

export const downloadFile = (code) => {
  window.open(
    `http://localhost:5000/api/files/${code}/download`,
    '_blank'
  )
}