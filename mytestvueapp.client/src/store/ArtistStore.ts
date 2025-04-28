import { defineStore } from "pinia"
import Artist from "@/entities/Artist"

export const useArtistStore = defineStore('artists', {
    state: () => { 
        return {
            artists: [] as Artist[]
        }
    },
    actions: {
         init() {
              const store = localStorage.getItem('artists') as string;
              if (store) {
                const parsedArtists = JSON.parse(store);
                this.artists = parsedArtists.map((artist: any) => Object.assign(new Artist(), artist));
              }
            },
        addArtist(artist: Artist) {
            if (!this.artists.includes(artist)) {
                this.artists.push(artist);
            }
        },
        save() {
            localStorage.setItem('artists', JSON.stringify(this.artists));
        },
        empty() {
            this.artists = [] as Artist[];
          },
        clearStorage() {
            localStorage.removeItem('artists');
        }

    }
})