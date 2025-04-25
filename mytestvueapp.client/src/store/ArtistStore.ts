import { defineStore } from "pinia"
import Artist from "@/entities/Artist"

export const useArtistStore = defineStore('artists', {
    state: () => { 
        return {
            artists: [] as Artist[]
        }
    },
})