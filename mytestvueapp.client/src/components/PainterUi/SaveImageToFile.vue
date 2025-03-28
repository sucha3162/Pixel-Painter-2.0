<template>
    <Button label="Download" icon="pi pi-save" @click="handleClick()"></Button>
</template>
<script setup lang="ts">
import Art from "@/entities/Art";
import Button from "primevue/button";
import { PixelGrid } from "@/entities/PixelGrid";
import GIFCreationService from "@/services/GIFCreationService";

const props = defineProps<{
  art: Art;
}>();

function handleClick() {
  if (props.art.pixelGrid.isGif) {
    saveGIF();
  } else {
    SaveToFile();
  }
}

function SaveToFile() {
  console.log(props.art.pixelGrid.grid);
  const canvas = document.createElement("canvas");
  const context = canvas.getContext("2d");
  if (!context) {
    throw new Error("Could not get context");
  }
  const image = context.createImageData(
    props.art.pixelGrid.width,
    props.art.pixelGrid.height
  );

  canvas.width = props.art.pixelGrid.width;
  canvas.height = props.art.pixelGrid.height;

  for (let x = 0; x < props.art.pixelGrid.height; x++) {
    for (let y = 0; y < props.art.pixelGrid.width; y++) {
      let pixelHex;
      if (props.art.pixelGrid.grid[x][y] === "empty") {
        pixelHex = props.art.pixelGrid.backgroundColor;
      } else {
        pixelHex = props.art.pixelGrid.grid[x][y];
      }
      pixelHex = pixelHex.replace("#", "").toUpperCase();
      const index = (x + y * props.art.pixelGrid.width) * 4;
      image?.data.set(
        [
          parseInt(pixelHex.substring(0, 2), 16),
          parseInt(pixelHex.substring(2, 4), 16),
          parseInt(pixelHex.substring(4, 6), 16),
          255,
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
  let i = 1;
  let workingGrid: PixelGrid;
  let urls = [];

  while ((workingGrid = JSON.parse(localStorage.getItem(`frame${i}`) as string) as PixelGrid) !== null) {
    console.log(workingGrid.grid);
    const canvas = document.createElement("canvas");
    const context = canvas.getContext("2d");
    if (!context) {
        throw new Error("Could not get context");
    }
    const image = context.createImageData(
        workingGrid.width,
        workingGrid.height
    );

    canvas.width = workingGrid.width;
    canvas.height = workingGrid.height;

    for (let x = 0; x < workingGrid.height; x++) {
      for (let y = 0; y < workingGrid.width; y++) {
        let pixelHex;
        if (workingGrid.grid[x][y] === "empty") {
          pixelHex = workingGrid.backgroundColor;
        } else {
          pixelHex = workingGrid.grid[x][y];
        }
        pixelHex = pixelHex.replace("#", "").toUpperCase();
        const index = (x + y * workingGrid.width) * 4;
        image?.data.set(
          [
            parseInt(pixelHex.substring(0, 2), 16),
            parseInt(pixelHex.substring(2, 4), 16),
            parseInt(pixelHex.substring(4, 6), 16),
            255,
          ],
          index
        );
      }
    }
    context?.putImageData(image, 0, 0);

    var upsizedCanvas = document.createElement("canvas");
    upsizedCanvas.width = 1080;
    upsizedCanvas.height = 1080;
    var upsizedContext = upsizedCanvas.getContext("2d");
    if (!upsizedContext) {
        throw new Error("Could not get context");
    }
    upsizedContext.imageSmoothingEnabled = false;
    upsizedContext.drawImage(canvas, 0, 0, 1080, 1080);

    var dataURL = upsizedCanvas.toDataURL("image/png");
    const strings = dataURL.split(",");
    urls.push(strings[1]);
    i++;
  }
  GIFCreationService.createGIF(urls);
}
</script>
