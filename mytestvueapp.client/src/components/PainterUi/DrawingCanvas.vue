<template>
  <div>
    <div id="canvas" style="position: fixed"></div>
  </div>
</template>

<script setup lang="ts">
import { Application, Sprite, Texture } from "pixi.js";
import { DropShadowFilter } from "@pixi/filter-drop-shadow";
import { OutlineFilter } from "@pixi/filter-outline";
import { Viewport } from "pixi-viewport"; // create viewport
import { onMounted, watch, ref } from "vue";
import { PixelGrid } from "@/entities/PixelGrid";
import PainterTool from "@/entities/PainterTool";
import { Vector2 } from "@/entities/Vector2";
import Cursor from "@/entities/Cursor";
import { useLayerStore } from "@/store/LayerStore";

//Constants
const PIXEL_SIZE = 10;
const layerStore = useLayerStore();

//props
const props = defineProps<{
  grid: PixelGrid;
  showLayers: boolean;
  greyscale: boolean;
}>();

//exposes (only put methods here if there are things painterview does that DIRECTLY update the canvas)
defineExpose({
  recenter,
  updateCursor,
  drawLayers,
  updateCell,
  init
});

//model
const cursor = defineModel<Cursor>({
  default: new Cursor(
    new Vector2(0, 0),
    PainterTool.getDefaults()[1],
    1,
    "#000000"
  )
});

//Runs on mounted, creates the canvas
onMounted(() => {
  document.getElementById("canvas")?.appendChild(app.view as any);
  init();
  drawLayers(0);
  recenter();
  checkIfPan();
});

//initialize the canvas
const app = new Application({
  resizeTo: window,
  backgroundAlpha: 0
});

// creates the viewport
const viewport = new Viewport({
  screenWidth: window.innerWidth,
  screenHeight: window.innerHeight,
  worldWidth: 100,
  worldHeight: 100
});

// add the viewport to the stage
app.stage.addChild(viewport);

// activate viewport plugins
viewport.drag().pinch().wheel().decelerate({ friction: 0.7 });

//Creates dropshadow and background layer
function init() {
  viewport.removeChildren();

  var dropShadowFilter = new DropShadowFilter();
  dropShadowFilter.distance = 0;

  const dropShadow = new Sprite(Texture.WHITE);
  dropShadow.width = layerStore.grids[0].width * PIXEL_SIZE;
  dropShadow.height = layerStore.grids[0].width * PIXEL_SIZE;
  dropShadow.tint = 0x000000;
  dropShadow.filters = [dropShadowFilter];

  const background = new Sprite(Texture.WHITE);
  background.width = layerStore.grids[0].width * PIXEL_SIZE;
  background.height = layerStore.grids[0].width * PIXEL_SIZE;
  background.tint = layerStore.grids[0].backgroundColor;

  viewport.addChild(dropShadow);
  viewport.addChild(background);
}

function drawLayers(layer: number) {
  let index = 0;
  if (!props.showLayers) {
    index = layer; //for showing only the selected layer
  } else if (props.grid.isGif && layer != 0) {
    index = layer - 1;
  }

  if (viewport.children.length > 2) {
    viewport.removeChildren(2);
  }
  let width = layerStore.grids[0].width;
  let height = layerStore.grids[0].height;

  const dropShadow = viewport.children[0] as Sprite;
  const background = viewport.children[1] as Sprite;

  if (dropShadow.width != width) {
    dropShadow.width = layerStore.grids[0].width * PIXEL_SIZE;
    dropShadow.height = layerStore.grids[0].width * PIXEL_SIZE;

    background.tint = layerStore.grids[layer].backgroundColor;
    background.width = layerStore.grids[0].width * PIXEL_SIZE;
    background.height = layerStore.grids[0].width * PIXEL_SIZE;
  }

  for (index; index <= layer; index++) {
    for (let i = 0; i < width; i++) {
      for (let j = 0; j < height; j++) {
        const sprite = viewport.addChild(new Sprite(Texture.WHITE));
        if (layerStore.grids[index].grid[i][j] === "empty") {
          sprite.tint = layerStore.grids[index].backgroundColor;
          sprite.alpha = 0;
        } else {
          let tmp = layerStore.grids[index].grid[i][j];
          if (
            index < layerStore.layer &&
            (props.greyscale || props.grid.isGif)
          ) {
            tmp = filterGreyScale(tmp);
          }
          sprite.tint = tmp;
          sprite.alpha = 1;
        }
        sprite.width = sprite.height = PIXEL_SIZE;
        sprite.position.set(i * PIXEL_SIZE, j * PIXEL_SIZE);
        sprite.interactive = index === layer ? true : false; //reduce lag
      }
    }
  }
}

function updateCell(layer: number, x: number, y: number, color: string) {
  if (layer <= layerStore.layer) {
    let idx = 2;

    if (!props.grid.isGif) {
      //square the width to get last index of grid before current,
      //mult by layer to get selected layer,
      //add by 2 to account for dropshadow and background sprites in viewport
      idx += layerStore.grids[0].width ** 2 * layer;
      if (!props.showLayers) {
        idx = 2;
      }
    }

    //no way around this, viewport stores sprites in a 1d array
    idx += x * layerStore.grids[0].width + y;
    const cell = viewport.children[idx] as Sprite;
    if (color === "empty") {
      cell.alpha = 0;
    } else {
      let tmp = color;
      if (layer < layerStore.layer && props.greyscale) {
        tmp = filterGreyScale(tmp);
      }
      cell.tint = tmp;
      cell.alpha = 1;
    }
  }
}

function filterGreyScale(hex: string): string {
  let r = parseInt(hex.slice(0, 2), 16),
    g = parseInt(hex.slice(2, 4), 16),
    b = parseInt(hex.slice(4, 6), 16);

  r = Math.round(r * 0.3);
  g = Math.round(g * 0.59);
  b = Math.round(b * 0.11);

  let gray = r + g + b;

  let newrgb = [gray, gray, gray];

  let val = newrgb.map((x) => x.toString(16).padStart(2, "0")).join("");
  return val;
}

const pos = ref<any>();

viewport.on("pointermove", (e) => {
  pos.value = viewport.toWorld(e.globalX, e.globalY);

  updateCursor();
});

//update cursor, not full canvas
function updateCursor() {
  if (
    cursor.value.selectedTool.label == "Brush" ||
    cursor.value.selectedTool.label == "Eraser"
  ) {
    cursor.value.position.x = Math.floor(
      (pos.value.x - ((cursor.value.size - 1) / 2) * PIXEL_SIZE) / PIXEL_SIZE
    );
    cursor.value.position.y = Math.floor(
      (pos.value.y - ((cursor.value.size - 1) / 2) * PIXEL_SIZE) / PIXEL_SIZE
    );
  } else {
    cursor.value.position.x = Math.floor(pos.value.x / PIXEL_SIZE);
    cursor.value.position.y = Math.floor(pos.value.y / PIXEL_SIZE);
  }

  // Remove the old cursor
  viewport.children.forEach((child) => {
    if (child.alpha == 0.99) {
      viewport.removeChild(child);
    }
  });

  // Add the new cursor
  const cursorBox = new Sprite(Texture.WHITE);
  var outlineFilter = new OutlineFilter(1, 0x000000);
  outlineFilter.knockout = true;
  cursorBox.alpha = 0.99;

  if (
    cursor.value.selectedTool.label == "Brush" ||
    cursor.value.selectedTool.label == "Eraser"
  ) {
    cursorBox.width = cursor.value.size * PIXEL_SIZE;
    cursorBox.height = cursor.value.size * PIXEL_SIZE;
    cursorBox.position.set(
      cursor.value.position.x * PIXEL_SIZE,
      cursor.value.position.y * PIXEL_SIZE
    );
  } else {
    cursorBox.width = PIXEL_SIZE;
    cursorBox.height = PIXEL_SIZE;
    cursorBox.position.set(
      cursor.value.position.x * PIXEL_SIZE,
      cursor.value.position.y * PIXEL_SIZE
    );
  }

  cursorBox.filters = [outlineFilter];
  viewport.addChild(cursorBox);
}

//centers the canvas
function recenter() {
  viewport.fit();
  viewport.setZoom(40 / layerStore.grids[0].width);
  viewport.moveCenter(
    (layerStore.grids[0].width * PIXEL_SIZE) / 2,
    (layerStore.grids[0].height * PIXEL_SIZE) / 2 +
      layerStore.grids[0].height * 2.5
  );
}

watch(
  () => props.grid.backgroundColor,
  () => {
    const bg = viewport.children[1] as Sprite;
    if (bg.tint !== props.grid.backgroundColor) {
      bg.tint = props.grid.backgroundColor;
    }
  }
);

watch([() => props.showLayers, () => props.greyscale], () => {
  drawLayers(layerStore.layer);
});

//disable panning when not in pan mode
watch(
  () => cursor.value.selectedTool.label,
  () => {
    //disable viewport drag if the tool is not the pan tool
    //but keep the .on click event
    checkIfPan();
  }
);

function checkIfPan() {
  if (cursor.value.selectedTool.label != "Pan") {
    viewport.threshold = 10000000;
  } else {
    viewport.threshold = 5;
  }
}
</script>
