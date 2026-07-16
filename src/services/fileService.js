// In-memory mock store
const filesStore = [
  { code: 'abc123', originalFileName: 'cat.jpg', sizeBytes: 125000, createdAt: '2026-06-18' },
  { code: 'def456', originalFileName: 'dog.png', sizeBytes: 240000, createdAt: '2026-06-10' },
]

const wait = (ms = 150) => new Promise((r) => setTimeout(r, ms))

export const listFiles = async () => {
  await wait()
  return { data: filesStore.slice().reverse() }
}

export const getFileMetadata = async (code) => {
  await wait()
  const f = filesStore.find((x) => x.code === code)
  if (!f) {
    const err = new Error('Not Found')
    err.response = { status: 404 }
    throw err
  }
  return { data: { ...f } }
}

export const uploadFile = async (formData) => {
  await wait(300)
  const file = formData.get('file')
  const code = Math.random().toString(36).slice(2, 8)
  const entry = {
    code,
    originalFileName: file && file.name ? file.name : `file-${code}`,
    sizeBytes: file && file.size ? file.size : 0,
    createdAt: new Date().toISOString().split('T')[0],
    content: null,
    type: file && file.type ? file.type : null,
  }

  if (file && file.type && file.type.startsWith('image/')) {
    // read as data URL
    const dataUrl = await new Promise((resolve, reject) => {
      const reader = new FileReader()
      reader.onload = () => resolve(reader.result)
      reader.onerror = reject
      reader.readAsDataURL(file)
    })
    entry.content = dataUrl
  }

  filesStore.push(entry)
  return { data: { downloadUrl: `${location.origin}/f/${code}`, entry } }
}

export const updateFile = async (code, updates) => {
  await wait()
  const idx = filesStore.findIndex((x) => x.code === code)
  if (idx === -1) {
    const err = new Error('Not Found')
    err.response = { status: 404 }
    throw err
  }
  filesStore[idx] = { ...filesStore[idx], ...updates }
  return { data: { ...filesStore[idx] } }
}

export const deleteFile = async (code) => {
  await wait()
  const idx = filesStore.findIndex((x) => x.code === code)
  if (idx === -1) {
    const err = new Error('Not Found')
    err.response = { status: 404 }
    throw err
  }
  filesStore.splice(idx, 1)
  return { data: { ok: true } }
}

export const downloadFile = (code) => {
  const f = filesStore.find((x) => x.code === code) || { originalFileName: `${code}.txt` }
  const blob = new Blob([`Fake content for ${f.originalFileName}`], { type: 'application/octet-stream' })
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = f.originalFileName
  document.body.appendChild(a)
  a.click()
  a.remove()
  URL.revokeObjectURL(url)
}