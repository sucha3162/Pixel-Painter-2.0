import { defineStore } from 'pinia'
import Notification from '@/entities/Notification'

export const useNotificationStore = defineStore('notification', {
  // arrow function recommended for full type inference
  state: () => {
    return {
      notifications: [] as Notification[]
    }
  },
})