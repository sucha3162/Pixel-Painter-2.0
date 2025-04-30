<template>
  <Button label="Download" icon="pi pi-save" @click="handleClick()"></Button>
</template>
<script setup lang="ts">
import Art from "@/entities/Art";
import Button from "primevue/button";
import GIFCreationService from "@/services/GIFCreationService";
import { useLayerStore } from "@/store/LayerStore";

const layerStore = useLayerStore();
const props = defineProps<{
  art: Art;
  fps: number;
}>();

function handleClick() {
  if (props.art.pixelGrid.isGif) {
    saveGIF();
  } else {
    saveToFile();
  }
}

function flattenArt(): string[][] {
  let width = layerStore.grids[0].width;
  let height = layerStore.grids[0].height;
  let arr: string[][] = Array.from({ length: height }, () =>
    Array(width).fill(layerStore.grids[0].backgroundColor)
  );

  for (let length = 0; length < layerStore.grids.length; length++) {
    for (let i = 0; i < height; i++) {
      for (let j = 0; j < width; j++) {
        //only set empty cells to background color if its the first layer
        //layers above the first will just replace cells if they have a value
        if (layerStore.grids[length].grid[i][j] !== "empty") {
          arr[i][j] = layerStore.grids[length].grid[i][j];
        }
      }
    }
  }
  return arr;
}

function saveToFile() {
  let grid: string[][];
  if (layerStore.grids.length > 1) {
    grid = flattenArt();
  } else {
    grid = props.art.pixelGrid.grid;
  }

  const canvas = document.createElement("canvas");
  const context = canvas.getContext("2d");
  if (!context) {
    throw new Error("Could not get context");
  }
  const image = context.createImageData(grid.length, grid.length);

  canvas.width = grid.length;
  canvas.height = grid.length;

  for (let x = 0; x < grid.length; x++) {
    for (let y = 0; y < grid.length; y++) {
      let pixelHex = grid[x][y];
      pixelHex = pixelHex.replace("#", "").toUpperCase();
      const index = (x + y * grid.length) * 4;
      image?.data.set(
        [
          parseInt(pixelHex.substring(0, 2), 16),
          parseInt(pixelHex.substring(2, 4), 16),
          parseInt(pixelHex.substring(4, 6), 16),
          255
        ],
        index
      );
    }
  }
  context?.putImageData(image, 0, 0);

  //upscale the image to 1080
  var upsizedCanvas = document.createElement("canvas");
  upsizedCanvas.width = 1080;
  upsizedCanvas.height = 1080;
  var upsizedContext = upsizedCanvas.getContext("2d");
  if (!upsizedContext) {
    throw new Error("Could not get context");
  }
  upsizedContext.imageSmoothingEnabled = false; // Disable image smoothing
  upsizedContext.drawImage(canvas, 0, 0, 1080, 1080);

  const link = document.createElement("a");
  link.download = "image.png";
  link.href = upsizedCanvas.toDataURL("image/png");
  link.click();
}

function saveGIF() {
  let urls: string[] = [];
  let grids = layerStore.grids;

  for (let i = 0; i < grids.length; i++) {
    const canvas = document.createElement("canvas");
    const context = canvas.getContext("2d");
    if (!context) {
      throw new Error("Could not get context");
    }
    const image = context.createImageData(grids[i].width, grids[i].height);

    canvas.width = grids[i].width;
    canvas.height = grids[i].height;

    for (let x = 0; x < grids[i].height; x++) {
      for (let y = 0; y < grids[i].width; y++) {
        let pixelHex;
        if (grids[i].grid[x][y] === "empty") {
          pixelHex = grids[i].backgroundColor;
        } else {
          pixelHex = grids[i].grid[x][y];
        }
        pixelHex = pixelHex.replace("#", "").toUpperCase();
        const index = (x + y * grids[i].width) * 4;
        image?.data.set(
          [
            parseInt(pixelHex.substring(0, 2), 16),
            parseInt(pixelHex.substring(2, 4), 16),
            parseInt(pixelHex.substring(4, 6), 16),
            255
          ],
          index
        );
      }
    }
    context?.putImageData(image, 0, 0);

    let upsizedCanvas = document.createElement("canvas");
    upsizedCanvas.width = 1080;
    upsizedCanvas.height = 1080;
    let upsizedContext = upsizedCanvas.getContext("2d");
    if (!upsizedContext) {
      throw new Error("Could not get context");
    }
    upsizedContext.imageSmoothingEnabled = false;
    upsizedContext.drawImage(canvas, 0, 0, 1080, 1080);

    let dataURL = upsizedCanvas.toDataURL("image/png");
    const strings = dataURL.split(",");
    urls.push(strings[1]);
  }
  GIFCreationService.createGIF(urls, props.fps);
}
</script>
