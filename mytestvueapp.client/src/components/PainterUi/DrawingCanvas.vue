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

//Constants
var PIXEL_SIZE = 10;

//props
const props = defineProps<{
  pixelGrid: PixelGrid;
}>();

//exposes the recenter function to be called in parent component
  defineExpose({ recenter, updateCursor, updateCanvas });

//other variables
const firstLoad = ref<boolean>(true);

//model
const cursor = defineModel<Cursor>({
  default: new Cursor(
    new Vector2(0, 0),
    PainterTool.getDefaults()[1],
    1,
    "#000000",
  ),
});

//Runs on mounted, creates the canvas
onMounted(() => {
  document.getElementById("canvas")?.appendChild(app.view as any);

  drawCanvas();
  recenter();
  checkIfPan();
});

//initialize the canvas
const app = new Application({
  resizeTo: window,
  backgroundAlpha: 0,
});

// creates the viewport
var viewport = new Viewport({
  screenWidth: window.innerWidth,
  screenHeight: window.innerHeight,
  worldWidth: 100,
  worldHeight: 100,
});

// add the viewport to the stage
app.stage.addChild(viewport);

// activate viewport plugins
viewport.drag().pinch().wheel().decelerate({ friction: 0.7 });

function updateCanvas() {
  //dropshadow and background are idx 0 and 1 respectively
  let idx = 2;

  if (viewport.children[1].tint !== props.pixelGrid.backgroundColor) {
    viewport.children[1].tint = props.pixelGrid.backgroundColor;
  }
  else { 
    for (var i = 0; i < props.pixelGrid.height; i++) {
      for (var j = 0; j < props.pixelGrid.width; j++) {
        //tint of erased values doesn't matter since we look at the grid
        if (props.pixelGrid.grid[i][j] === "empty") {
          viewport.children[idx].alpha = 0;
        } else {
          viewport.children[idx].tint = props.pixelGrid.grid[i][j];
          viewport.children[idx].alpha = 1;
        }
        idx++;
      }
    }
  }
}

//Draws the canvas
function drawCanvas() {
  viewport.removeChildren();

  var dropShadowFilter = new DropShadowFilter();
  dropShadowFilter.distance = 0;

  const dropShadow = new Sprite(Texture.WHITE);
  dropShadow.width = props.pixelGrid.width * PIXEL_SIZE;
  dropShadow.height = props.pixelGrid.height * PIXEL_SIZE;
  dropShadow.tint = 0x000000;
  dropShadow.filters = [dropShadowFilter];

  const background = new Sprite(Texture.WHITE);
  background.width = props.pixelGrid.width * PIXEL_SIZE;
  background.height = props.pixelGrid.height * PIXEL_SIZE;

  background.tint = props.pixelGrid.backgroundColor;

  viewport.addChild(dropShadow);
  viewport.addChild(background);

  for (var i = 0; i < props.pixelGrid.height; i++) {
    for (var j = 0; j < props.pixelGrid.width; j++) {
      const sprite = viewport.addChild(new Sprite(Texture.WHITE));  
      if (props.pixelGrid.grid[i][j] === "empty") {
        sprite.tint = props.pixelGrid.backgroundColor;
        sprite.alpha = 0;
      } else {
        sprite.tint = props.pixelGrid.grid[i][j];
        sprite.alpha = 1;
      }
      sprite.width = sprite.height = PIXEL_SIZE;
      sprite.position.set(i * PIXEL_SIZE, j * PIXEL_SIZE);
      sprite.interactive = true;
    }
  }
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
      (pos.value.x - ((cursor.value.size - 1) / 2) * PIXEL_SIZE) / PIXEL_SIZE,
    );
    cursor.value.position.y = Math.floor(
      (pos.value.y - ((cursor.value.size - 1) / 2) * PIXEL_SIZE) / PIXEL_SIZE,
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
      cursor.value.position.y * PIXEL_SIZE,
    );
  } else {
    cursorBox.width = PIXEL_SIZE;
    cursorBox.height = PIXEL_SIZE;
    cursorBox.position.set(
      cursor.value.position.x * PIXEL_SIZE,
      cursor.value.position.y * PIXEL_SIZE,
    );
  }

  cursorBox.filters = [outlineFilter];
  viewport.addChild(cursorBox);
}

//centers the canvas
function recenter() {
  viewport.fit();
  viewport.setZoom(40 / props.pixelGrid.width);
  viewport.moveCenter(
    (props.pixelGrid.width * PIXEL_SIZE) / 2,
    (props.pixelGrid.height * PIXEL_SIZE) / 2 + props.pixelGrid.height * 2.5,
  );
}

watch(props.pixelGrid, (prev, next) => {
  if (firstLoad.value) {
    drawCanvas();
    updateCanvas();
    firstLoad.value = false;
  } else {
    updateCanvas();
  }
});

//disable panning when not in pan mode
watch(
  () => cursor.value.selectedTool.label,
  () => {
    //disable viewport drag if the tool is not the pan tool
    //but keep the .on click event
    checkIfPan();
  },
);

function checkIfPan() {
  if (cursor.value.selectedTool.label != "Pan") {
    viewport.threshold = 10000000;
  } else {
    viewport.threshold = 5;
  }
}
</script>
