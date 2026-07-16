import {
  createRouter,
  createWebHistory
} from 'vue-router'

import UploadView
from '../views/UploadView.vue'

const router = createRouter({
  history: createWebHistory(),

  routes: [
    {
      path: '/f/:code',
      component: FileView
    }
  ]
})

export default router