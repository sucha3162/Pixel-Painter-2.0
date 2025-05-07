import { defineStore } from "pinia"
import { PixelGrid } from "@/entities/PixelGrid"

export const useLayerStore = defineStore('layers', {
  state: () => ({
    grids: [] as PixelGrid[],
    layer: 0
  }),
  actions: {
    init(): void {
      const store = localStorage.getItem('grids') as string;
      if (store) {
        const parsedGrids = JSON.parse(store);
        this.grids = parsedGrids.map((grid: any) => Object.assign(new PixelGrid(1, 1, "FF0000", false), grid));
      }
    },
    popGrid(): void {
      this.grids.pop();
    },
    save(): void {
      localStorage.setItem('grids', JSON.stringify(this.grids));
    },
    pushGrid(grid: PixelGrid): void {
      this.grids.push(grid);
    },
    removeGrid(idx: number): void {
      this.grids.splice(idx, 1);
    },
    empty(): void {
      this.grids.splice(0, this.grids.length);
      this.layer = 0;
    },
    clearStorage(): void {
      localStorage.removeItem('grids');
    },
    getGridArray(): string[][][] {
      const arr: string[][][] = [];
      for (let i = 0; i < this.grids.length; i++)
        arr.push(this.grids[i].grid);
      return arr;
    }
  }
});