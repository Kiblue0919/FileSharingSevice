import { createRouter, createWebHistory } from 'vue-router'
import UploadView from '../views/UploadView.vue'
import FileView from '../views/FileView.vue'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/',
      component: UploadView
    },
    {
      path: '/f/:code',
      component: FileView
    }
  ]
})

export default router