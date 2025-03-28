import { defineStore } from "pinia"
import { DoublyLinkedList }  from "@/utils/DoublyLinkedList";

export const useTempStore = defineStore('storeId', {
  state: () => {
    return {
      temp: DoublyLinkedList,
    }
  }
})