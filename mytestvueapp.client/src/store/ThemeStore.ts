import { defineStore } from 'pinia'

export const useThemeStore = defineStore('theme', {
  // arrow function recommended for full type inference
  state: () => {
    return {
      // all these properties will have their type inferred automatically
      Theme: "dark"
    }
  },
})