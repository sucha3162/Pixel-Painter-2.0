<template>
  <DrawingCanvas
    ref="canvas"
    :pixelGrid="art.pixelGrid"
    :style="{ cursor: cursor.selectedTool.cursor }"
    v-model="cursor"
    @mousedown="
      mouseButtonHeldDown = true;
      setStartVector();
      setEndVector();
    "
    @mouseup="
      mouseButtonHeldDown = false;
      setEndVector();
      onMouseUp()"
    @contextmenu.prevent />
  <Toolbar class="fixed bottom-0 left-0 right-0 m-2">
    <template #start>
      <div class="flex gap-2">
        <Button
          icon="pi pi-ban"
          label="Quit"
          severity="secondary"
          @click="ResetArt()">
        </Button>
        <UploadButton :art="art" :connection="connection" :connected="connected" :group-name="groupName" @OpenModal="ToggleKeybinds" />
        <SaveImageToFile :art="art"></SaveImageToFile>
        <ConnectButton @OpenModal="ToggleKeybinds" @Connect="connect" @Disconnect="disconnect" :connected="connected" :isGif="art.pixelGrid.isGif" />
      </div>
    </template>

    <template #center>
      <ColorSelection 
      v-model:color="cursor.color"
      v-model:size="cursor.size"
      @enable-key-binds="keyBindActive = true"
      @disable-key-binds="keyBindActive = false" />
      <BrushSelection v-model="cursor.selectedTool" />
      <ColorSelection 
      v-model:color="art.pixelGrid.backgroundColor"
      v-model:size="cursor.size"
      isBackground="true"
      @enable-key-binds="keyBindActive = true"
      @disable-key-binds="keyBindActive = false" />
      <FrameSelection v-if="art.pixelGrid.isGif" v-model:selFrame="selectedFrame" v-model:lastFrame="lastFrame" v-model:frameIndex="index"/>
      <LayerSelection v-if="!art.pixelGrid.isGif"/>
    </template>
    <template #end>
      <Button
        icon="pi pi-expand"
        class="mr-2"
        severity="primary"
        label="Recenter"
        @click="canvas?.recenter()" />
      <Button
        :icon="intervalId != -1 ? 'pi pi-stop' : 'pi pi-play'"
        class="mr-2 Rainbow"
        label="Gravity"
        @click="runGravity()" />

      <Button
        icon="pi pi-lightbulb"
        class="Rainbow"
        label="Give Me Color!"
        @click="
        art.pixelGrid.randomizeGrid();"/>
    </template>
  </Toolbar>
</template>

<script setup lang="ts">
//vue prime
import Toolbar from "primevue/toolbar";
import Button from "primevue/button";

//custom components
import DrawingCanvas from "@/components/PainterUi/DrawingCanvas.vue";
import BrushSelection from "@/components/PainterUi/BrushSelection.vue";
import ColorSelection from "@/components/PainterUi/ColorSelection.vue";
import UploadButton from "@/components/PainterUi/UploadButton.vue";
import SaveImageToFile from "@/components/PainterUi/SaveImageToFile.vue";
import FrameSelection from "@/components/PainterUi/FrameSelection.vue";
import BackgroundSelection from "@/components/PainterUi/BackgroundSelection.vue";
import LayerSelection from "@/components/PainterUi/LayerSelection.vue";

//entities
import { PixelGrid } from "@/entities/PixelGrid";
import { Vector2 } from "@/entities/Vector2";
import PainterTool from "@/entities/PainterTool";
import Cursor from "@/entities/Cursor";
import { Pixel } from "@/entities/Pixel";
import Artist from "@/entities/Artist";

import LoginService from "@/services/LoginService";

//vue
import { ref, watch, computed, onMounted, onUnmounted } from "vue";
import router from "@/router";
import { onBeforeRouteLeave } from "vue-router";
import { useRoute } from "vue-router";
import { useToast } from "primevue/usetoast";

//scripts
import ArtAccessService from "@/services/ArtAccessService";
import Art from "@/entities/Art";
import fallingSand from "@/utils/fallingSand";
import ConnectButton from "@/components/PainterUi/ConnectButton.vue";

//Other
import * as SignalR from "@microsoft/signalr";
import { FillStyle } from "pixi.js";

//variables
const route = useRoute();
const canvas = ref();
const toast = useToast();
const intervalId = ref<number>(-1);
const keyBindActive = ref<boolean>(true);
const artist = ref<Artist>(new Artist);

// Connection Information
const connected = ref(false);
const groupName = ref("");
let connection = new SignalR.HubConnectionBuilder()
            .withUrl("https://localhost:7154/signalhub", {
                skipNegotiation: true,
                transport: SignalR.HttpTransportType.WebSockets
            })
            .build();

connection.on("Send", (user: string, msg: string) => {
        console.log("Received Message", user + " " + msg);
});

connection.on("NewMember", (newartist: Artist) => {
  console.log("New Member: "+ newartist.name);
  if (!art.value.artistId.includes(newartist.id)){
    art.value.artistId.push(newartist.id);
    art.value.artistName.push(newartist.name);
  }
  console.log("NewMember-Members: " +  art.value.artistName.join(" "))
});

connection.on("Members", (artists: Artist[]) => {
  console.log("Recieved All Members");
  artists.forEach(artist => {
    if (!art.value.artistId.includes(artist.id)){
      art.value.artistId.push(artist.id);
      art.value.artistName.push(artist.name);
    }
  })
  console.log("Members-Members: " +  art.value.artistName.join(" "))
});

connection.onclose(error => {
  if (error) {
    toast.add({
            severity: "error",
            summary: "Error",
            detail: "You have disconnected!",
            life: 3000,
          });
    connected.value = false;
    }
});

connection.on("ReceivePixel", (color: string, coord: Vector2) => {
        DrawPixel(color, coord);
});

connection.on("ReceivePixels", (color: string, coords: Vector2[]) => {
        DrawPixels(color, coords);
});

connection.on("ReceiveBucket", (color: string, coord: Vector2) => {
        fill(coord.x, coord.y, color);
});

connection.on("GroupConfig", (canvasSize: number, backgroundColor: string, pixels: Pixel[]) => {
  art.value.pixelGrid.width = canvasSize;
  art.value.pixelGrid.height = canvasSize;
  art.value.pixelGrid.backgroundColor = backgroundColor;
  art.value.pixelGrid.grid = art.value.pixelGrid.createGrid(canvasSize,canvasSize,backgroundColor);
  ReplaceCanvas(pixels);
});

connection.on("BackgroundColor", (backgroundColor: string) => {
  art.value.pixelGrid.backgroundColor = backgroundColor;
});

const connect = (groupname: string) => {

  if (artist.value.id != 0){
    connection.start()
        .then(
            () => {
                console.log("Connected to SignalR!");
                connection.invoke("CreateOrJoinGroup", groupname, artist.value, art.value.pixelGrid.grid, art.value.pixelGrid.width, art.value.pixelGrid.backgroundColor);
                groupName.value = groupname;
                connected.value = !connected.value;
            }
        ).catch(err => console.error("Error connecting to Hub:",err));
  } else {
    toast.add({
          severity: "error",
          summary: "Error",
          detail: "Please log in before collaborating!",
          life: 3000,
        });
  }
}


const disconnect = (groupname: string) => {
  connection.invoke("LeaveGroup", groupname, artist.value)
    .then(() => {
      connection.stop()
        .then(() => {
          connected.value = !connected.value;
        }).catch(err => console.error("Error Disconnecting:", err));
    }
    ).catch(err => console.error("Error Leaving Group:",err));
}
//End of Connection Information
const cursor = ref<Cursor>(
  new Cursor(new Vector2(-1, -1), PainterTool.getDefaults()[1], 1, "000000")
);

const mouseButtonHeldDown = ref<boolean>(false);

const startPix = ref<Vector2>(new Vector2(0, 0));
const endPix = ref<Vector2>(new Vector2(0, 0));
let tempGrid: string[][] = [];

const art = ref<Art>(new Art());

let selectedFrame = ref(1);
let lastFrame = ref(1);
let index = ref(1);


let currentPallet: string[];
function updatePallet() {
  let temp = localStorage.getItem("currentPallet");
  if (temp) currentPallet = JSON.parse(temp);
  for (let i = 0; i < currentPallet.length; i++)
    if (currentPallet[i] === null || currentPallet[i] === "") {
      currentPallet[i] = "000000";
    }
}
const cursorPositionComputed = computed(
  //default vue watchers can't watch deep properties
  //it can only watch individual references to the object specified
  //since when cursor position changes, the object holding the cursor position doesn't change
  //thus the watcher won't trigger
  //in order to get around this, we can use a computed property
  //a computed propert is updated every time a dependency changes
  //this computed property will return a new object every time the cursor position changes
  //thus the watcher watching this value will trigger with the old and new values
  //vue likes to be funky like that :3
  () => new Vector2(cursor.value.position.x, cursor.value.position.y)
);

//lifecycle hooks
onBeforeRouteLeave((to, from, next) => {
  if (to.path != "/new" && !to.path.includes("/art")) {
      if (art.value.pixelGrid.isGif) {
          LocalSaveGif();
      } else {
          LocalSave();
      }
    }
  next();
});

onMounted(() => {
  document.addEventListener("keydown", handleKeyDown);
  window.addEventListener("beforeunload", handleBeforeUnload);

  //Get the current user
  LoginService.GetCurrentUser().then((user: Artist) => {
    if (user.id == 0) {
      artist.value.id = 0;
      artist.value.name = "Guest"
    }
    artist.value = user;
  });

  const workingGrid = JSON.parse(
    localStorage.getItem("working-art") as string
  ) as PixelGrid;

  if (route.params.id) {
    localStorage.removeItem("working-art");
    const id: number = parseInt(route.params.id as string);
    ArtAccessService.getArtById(id)
      .then((data) => {
        art.value.pixelGrid.DeepCopy(data.pixelGrid);
        art.value.id = data.id;
        art.value.title = data.title;
        art.value.isPublic = data.isPublic;

        canvas.value?.recenter();
      })
      .catch(() => {
        toast.add({
          severity: "error",
          summary: "Error",
          detail: "You cannot edit this art",
          life: 3000,
        });
        router.push("/new");
      });
  } else if (workingGrid == null) {
    router.push("/new");
  } else {
    art.value.pixelGrid.DeepCopy(workingGrid);
    canvas.value?.recenter();

    tempGrid = JSON.parse(JSON.stringify(art.value.pixelGrid.grid));
  }
});

onUnmounted(() => {
  document.removeEventListener("keydown", handleKeyDown);
  window.removeEventListener("beforeunload", handleBeforeUnload);
});

const ToggleKeybinds = (disable: boolean) => {
  if (disable) {
    document.removeEventListener("keydown", handleKeyDown);
  } else {
    document.addEventListener("keydown", handleKeyDown);
  }
};

function handleBeforeUnload(event: BeforeUnloadEvent) {
    if (art.value.pixelGrid.isGif) {
        LocalSaveGif();
    } else { 
        LocalSave();
    }
}

watch(
  cursorPositionComputed,
  (start: Vector2, end: Vector2) => {
    if (cursor.value.selectedTool.label === "Rectangle") {
      if (mouseButtonHeldDown.value) {
        setEndVector();
        DrawAtCoords(GetRectanglePixels(startPix.value, endPix.value));
      }
    } else if (cursor.value.selectedTool.label === "Ellipse") {
      if (mouseButtonHeldDown.value) {
        setEndVector();
        DrawAtCoords(GetEllipsePixels(startPix.value, endPix.value));
      }
    } else {
      DrawAtCoords(GetLinePixels(start, end));
    }
  },
  { deep: true }
);

watch(mouseButtonHeldDown, async () => {
  DrawAtCoords([cursor.value.position]);
});

watch(() => art.value.pixelGrid.backgroundColor, (newColor, oldColor) => {
  ChangeBackgroundColor(newColor);
});
    
watch(selectedFrame, () => {
  if (lastFrame.value <= index.value) {
      localStorage.setItem(`frame${lastFrame.value}`, JSON.stringify(art.value.pixelGrid));
  }

  const workingGrid = JSON.parse(
      localStorage.getItem(`frame${selectedFrame.value}`) as string
  ) as PixelGrid;

  if (workingGrid == null) {
      const newGrid = new PixelGrid(
          art.value.pixelGrid.width,
          art.value.pixelGrid.height,
          art.value.pixelGrid.backgroundColor,
          art.value.pixelGrid.isGif
      );
      art.value.pixelGrid.DeepCopy(newGrid);
      canvas.value?.updateCanvas();

      canvas.value?.recenter();
      localStorage.setItem(`frame${selectedFrame.value}`, JSON.stringify(art.value.pixelGrid));

  } else {
      art.value.pixelGrid.DeepCopy(workingGrid);
      canvas.value?.updateCanvas();

      canvas.value?.recenter();
      localStorage.setItem(`frame${selectedFrame.value}`, JSON.stringify(art.value.pixelGrid));
  }
});

//functions
function runGravity() {
  if (intervalId.value != -1) {
    clearInterval(intervalId.value);
    intervalId.value = -1;
  } else {
    intervalId.value = setInterval(fallingSand, 30, art.value.pixelGrid);
  }
}

function GetLinePixels(start: Vector2, end: Vector2): Vector2[] {
  const pixels: Vector2[] = [];

  const dx = Math.abs(end.x - start.x);
  const dy = Math.abs(end.y - start.y);

  const sx = start.x < end.x ? 1 : -1;
  const sy = start.y < end.y ? 1 : -1;

  let err = dx - dy;

  let currentX = start.x;
  let currentY = start.y;

  // eslint-disable-next-line no-constant-condition
  while (true) {
    pixels.push(new Vector2(currentX, currentY));

    // Check if we have reached the end point
    if (currentX === end.x && currentY === end.y) break;

    const e2 = 2 * err;

    if (e2 > -dy) {
      err -= dy;
      currentX += sx;
    }

    if (e2 < dx) {
      err += dx;
      currentY += sy;
    }
  }

  return pixels;
}

function DrawPixel(color: string, coord: Vector2) {
  art.value.pixelGrid.grid[coord.x][coord.y] = color;
}

function ReplaceCanvas(pixels: Pixel[]) {
  pixels.forEach(pixel => {
    art.value.pixelGrid.grid[pixel.x][pixel.y] = pixel.color;
  })
}

function DrawPixels(color: string, coords: Vector2[]) {
  for (const coord of coords) {
    art.value.pixelGrid.grid[coord.x][coord.y] = color;
  }
}

function SendPixels(color: string, coords: Vector2[]) {
  if (connected.value) {
    connection.invoke(
      "SendPixels",
      groupName.value,
      color,
      coords
    )
  }
}

function ChangeBackgroundColor(color: string) {
  if (connected.value) {
    connection.invoke(
      "ChangeBackgroundColor",
      groupName.value,
      color
    )
  }
}

function DrawAtCoords(coords: Vector2[]) {
    let coordinates: Vector2[] = [];

    if (
    cursor.value.selectedTool.label === "Rectangle" ||
    cursor.value.selectedTool.label === "Ellipse"
  ) {
    if (tempGrid) {
      for (let i = 0; i < art.value.pixelGrid.height; i++) {
        for (let j = 0; j < art.value.pixelGrid.width; j++) {
          art.value.pixelGrid.grid[i][j] = tempGrid[i][j];
        }
      }
    }
  }
  coords.forEach((coord: Vector2) => {
    if (mouseButtonHeldDown.value) {
      if (cursor.value.selectedTool.label === "Brush") {
        for (let i = 0; i < cursor.value.size; i++) {
          for (let j = 0; j < cursor.value.size; j++) {
            if (
              coord.x + i >= 0 &&
              coord.x + i < art.value.pixelGrid.width &&
              coord.y + j >= 0 &&
              coord.y + j < art.value.pixelGrid.height
            ) {
              coordinates.push(new Vector2(coord.x + i, coord.y + j));
              art.value.pixelGrid.grid[coord.x + i][coord.y + j] =
                cursor.value.color;
            }
          }
        }
        SendPixels(cursor.value.color, coordinates);
      } else if (cursor.value.selectedTool.label === "Eraser") {
        for (let i = 0; i < cursor.value.size; i++) {
          for (let j = 0; j < cursor.value.size; j++) {
            if (
              coord.x + i >= 0 &&
              coord.x + i < art.value.pixelGrid.width &&
              coord.y + j >= 0 &&
              coord.y + j < art.value.pixelGrid.height
            ) {
              if (art.value.pixelGrid.backgroundColor != null) {
                coordinates.push(new Vector2(coord.x + i, coord.y + j));
                art.value.pixelGrid.grid[coord.x + i][coord.y + j] =
                  "empty";
              }
            }
          }
        }
        SendPixels("empty", coordinates);
      } else if (
        coord.x >= 0 &&
        coord.x < art.value.pixelGrid.width &&
        coord.y >= 0 &&
        coord.y < art.value.pixelGrid.height
      ) {
        if (cursor.value.selectedTool.label === "Pipette") {
          let tmp = art.value.pixelGrid.grid[coord.x][coord.y];
          if (tmp === "empty") tmp = art.value.pixelGrid.backgroundColor;

          cursor.value.color = tmp;
        } else if (cursor.value.selectedTool.label === "Bucket") {
          if (
            art.value.pixelGrid.grid[coord.x][coord.y] != cursor.value.color
          ) {
            coordinates = fill(cursor.value.position.x, cursor.value.position.y);
            SendPixels(cursor.value.color, coordinates);
          }
        } else if (
          cursor.value.selectedTool.label === "Rectangle" ||
          cursor.value.selectedTool.label === "Ellipse"
        ) {
          art.value.pixelGrid.grid[coord.x][coord.y] = cursor.value.color;
        }
      }
    }
  });
}

function fill(x: number, y: number, color: string = cursor.value.color) : Vector2[] {
  let vectors: Vector2[] = [];
  if (y >= 0 && y < art.value.pixelGrid.height) {
    const oldColor = art.value.pixelGrid.grid[x][y];
    art.value.pixelGrid.grid[x][y] = color;
    vectors.push(new Vector2(x,y));
    if (oldColor != color) {
      if (x + 1 < art.value.pixelGrid.width) {
        if (art.value.pixelGrid.grid[x + 1][y] == oldColor) {
          vectors = vectors.concat(fill(x + 1, y, color));
        }
      }
      if (y + 1 < art.value.pixelGrid.height) {
        if (art.value.pixelGrid.grid[x][y + 1] == oldColor) {
          vectors = vectors.concat(fill(x, y + 1, color));
        }
      }
      if (x - 1 >= 0) {
        if (art.value.pixelGrid.grid[x - 1][y] == oldColor) {
          vectors = vectors.concat(fill(x - 1, y, color));
        }
      }
      if (y - 1 >= 0) {
        if (art.value.pixelGrid.grid[x][y - 1] == oldColor) {
          vectors = vectors.concat(fill(x, y - 1, color));
        }
      }
    }
  }

  return vectors;
}

function GetRectanglePixels(start: Vector2, end: Vector2): Vector2[] {
  let coords: Vector2[] = [];
  let leftBound = Math.min(start.x, end.x);
  let rightBound = Math.max(start.x, end.x);
  let lowerBound = Math.min(start.y, end.y);
  let upperBound = Math.max(start.y, end.y);

  for (let i = 0; i < cursor.value.size; i++) {
    if (
      leftBound + i <= rightBound &&
      rightBound - i >= leftBound &&
      upperBound - i >= lowerBound &&
      lowerBound + i <= upperBound
    ) {
      coords = coords.concat(
        CalculateRectangle(
          new Vector2(leftBound + i, lowerBound + i),
          new Vector2(rightBound - i, upperBound - i)
        )
      );
    }
  }

  return coords;
}

function CalculateRectangle(start: Vector2, end: Vector2): Vector2[] {
  let coords: Vector2[] = [];

  // generate x coordinates
  let stepX = start.x;
  while (stepX != end.x) {
    coords.push(new Vector2(stepX, start.y));
    coords.push(new Vector2(stepX, end.y));

    if (stepX < end.x) stepX++;
    if (stepX > end.x) stepX--;
  }

  // generate y coordinates
  let stepY = start.y;
  while (stepY != end.y) {
    coords.push(new Vector2(start.x, stepY));
    coords.push(new Vector2(end.x, stepY));

    if (stepY < end.y) stepY++;
    if (stepY > end.y) stepY--;
  }

  coords.push(end);
  return coords;
}
function GetEllipsePixels(start: Vector2, end: Vector2): Vector2[] {
  let coords: Vector2[] = [];

  let leftBound = Math.min(start.x, end.x);
  let rightBound = Math.max(start.x, end.x);
  let lowerBound = Math.min(start.y, end.y);
  let upperBound = Math.max(start.y, end.y);

  for (let i = 0; i < cursor.value.size; i++) {
    if (
      leftBound + i <= rightBound &&
      rightBound - i >= leftBound &&
      upperBound - i >= lowerBound &&
      lowerBound + i <= upperBound
    ) {
      coords = coords.concat(
        CalculateEllipse(
          new Vector2(leftBound + i, lowerBound + i),
          new Vector2(rightBound - i, upperBound - i)
        )
      );
    }
  }

  return coords;
}

function CalculateEllipse(start: Vector2, end: Vector2): Vector2[] {
  let coords: Vector2[] = [];
  if (start.x == end.x && start.y == end.y) {
    coords.push(start);
    return coords;
  }
  let leftBound = Math.min(start.x, end.x);
  let rightBound = Math.max(start.x, end.x);
  let lowerBound = Math.min(start.y, end.y);
  let upperBound = Math.max(start.y, end.y);

  let xOffset = rightBound - leftBound;
  let yOffset = upperBound - lowerBound;

  let center = new Vector2(leftBound + xOffset / 2, lowerBound + yOffset / 2);


  let a = Math.max(xOffset, yOffset) / 2; //Major Axis length
  let b = Math.min(xOffset, yOffset) / 2; //Minor Axis length

  if (xOffset > yOffset) {
    // Major Axis is Horrizontal
    for (let i = leftBound; i <= rightBound; i++) {
      let yP = Math.round(ellipseXtoY(center, a, b, i));
      let yN = center.y - (yP - center.y);
      coords.push(new Vector2(i, yP));
      coords.push(new Vector2(i, yN));
    }
    for (let i = lowerBound; i < upperBound; i++) {
      let xP = Math.round(ellipseYtoX(center, b, a, i));
      let xN = center.x - (xP - center.x);
      coords.push(new Vector2(xP, i));
      coords.push(new Vector2(xN, i));
    }
  } else {
    // Major Axis is vertical
    for (let i = lowerBound; i <= upperBound; i++) {
      let xP = Math.round(ellipseYtoX(center, a, b, i));
      let xN = center.x - (xP - center.x);
      coords.push(new Vector2(xP, i));
      coords.push(new Vector2(xN, i));
    }
    for (let i = leftBound; i < rightBound; i++) {
      let yP = Math.round(ellipseXtoY(center, b, a, i));
      let yN = center.y - (yP - center.y);
      coords.push(new Vector2(i, yP));
      coords.push(new Vector2(i, yN));
    }
  }
  return coords;
}

function ellipseXtoY(
  center: Vector2,
  majorAxis: number,
  minorAxis: number,
  x: number
): number {
  let yPow = Math.pow((x - center.x) / majorAxis, 2);
  let ySqrt = Math.sqrt(1 - yPow);
  let y = minorAxis * ySqrt + center.y;
  return y;
}

function ellipseYtoX(
  center: Vector2,
  majorAxis: number,
  minorAxis: number,
  y: number
): number {
  let xPow = Math.pow((y - center.y) / majorAxis, 2);
  let xSqrt = Math.sqrt(1 - xPow);
  let x = minorAxis * xSqrt + center.x;
  return x;
}

function setStartVector() {
  startPix.value = new Vector2(
    cursor.value.position.x,
    cursor.value.position.y
  );
  tempGrid = JSON.parse(JSON.stringify(art.value.pixelGrid.grid));
}
function setEndVector() {
  if (mouseButtonHeldDown.value) {
    endPix.value = new Vector2(
      cursor.value.position.x,
      cursor.value.position.y
    );
  } else {
    tempGrid = JSON.parse(JSON.stringify(art.value.pixelGrid.grid));
  }
}

function ResetArt() {
  localStorage.removeItem("working-art");
  localStorage.removeItem("working-list");

    if (art.value.pixelGrid.isGif) {
        let tempCount = 1;
        while (localStorage.getItem(`frame${tempCount}`) != null) {
            localStorage.removeItem(`frame${tempCount}`);
            tempCount++;    
        }
    }

  router.push("/new");
}

function onMouseUp() {
  if (
    cursor.value.selectedTool.label == "Rectangle"
  ) {
    SendPixels(
      cursor.value.color,
      GetRectanglePixels(startPix.value, endPix.value)
    );
  }
  if (
    cursor.value.selectedTool.label == "Ellipse"
  ) {
    CalculateEllipse(startPix.value, endPix.value).forEach((vector) => {
    });
    SendPixels(
      cursor.value.color,
      GetEllipsePixels(startPix.value, endPix.value)
    );
  }
}

function handleKeyDown(event: KeyboardEvent) {
  if (keyBindActive.value) {
    if (event.key === "p") {
      event.preventDefault();
      cursor.value.selectedTool.label = "Pan";
      canvas?.value.updateCursor();
    } else if (event.key === "b") {
      event.preventDefault();
      cursor.value.selectedTool.label = "Brush";
      canvas?.value.updateCursor();
    } else if (event.key === "e") {
      event.preventDefault();
      cursor.value.selectedTool.label = "Eraser";
      canvas?.value.updateCursor();
    } else if (event.key === "d") {
      event.preventDefault();
      cursor.value.selectedTool.label = "Pipette";
      canvas?.value.updateCursor();
    } else if (event.key === "f") {
      event.preventDefault();
      cursor.value.selectedTool.label = "Bucket";
      canvas?.value.updateCursor();
    } else if (event.key === "r") {
      event.preventDefault();
      cursor.value.selectedTool.label = "Rectangle";
      canvas?.value.updateCursor();
    } else if (event.key === "q" && cursor.value.size > 1) {
      event.preventDefault();
      cursor.value.size -= 1;
      canvas?.value.updateCursor();
    } else if (event.key === "w" && cursor.value.size < 32) {
      event.preventDefault();
      cursor.value.size += 1;
      canvas?.value.updateCursor();
    } else if (event.key === "1") {
      event.preventDefault();
      updatePallet();
      cursor.value.color = currentPallet[0];
    } else if (event.key === "2") {
      event.preventDefault();
      updatePallet();
      cursor.value.color = currentPallet[1];
    } else if (event.key === "3") {
      event.preventDefault();
      updatePallet();
      cursor.value.color = currentPallet[2];
    } else if (event.key === "4") {
      event.preventDefault();
      updatePallet();
      cursor.value.color = currentPallet[3];
    } else if (event.key === "5") {
      event.preventDefault();
      updatePallet();
      cursor.value.color = currentPallet[4];
    } else if (event.key === "6") {
      event.preventDefault();
      updatePallet();
      cursor.value.color = currentPallet[5];
    } else if (event.key === "7") {
      event.preventDefault();
      updatePallet();
      cursor.value.color = currentPallet[6];
    } else if (event.key === "8") {
      event.preventDefault();
      updatePallet();
      cursor.value.color = currentPallet[7];
    } else if (event.key === "9") {
      event.preventDefault();
      updatePallet();
      cursor.value.color = currentPallet[8];
    } else if (event.key === "0") {
      event.preventDefault();
      updatePallet();
      cursor.value.color = currentPallet[9];
    } else if (event.key === "-") {
      event.preventDefault();
      updatePallet();
      cursor.value.color = currentPallet[10];
    } else if (event.key === "=") {
      event.preventDefault();
      updatePallet();
      cursor.value.color = currentPallet[11];
    }
  }
}

function LocalSave() {
  localStorage.setItem("working-art", JSON.stringify(art.value.pixelGrid));
}

function LocalSaveGif() {
  const workingGrid = JSON.parse(
    localStorage.getItem("frame1") as string
  ) as PixelGrid;

  localStorage.setItem("working-art", JSON.stringify(workingGrid));
  localStorage.setItem(`frame${selectedFrame.value}`, JSON.stringify(art.value.pixelGrid));
}

</script>
<style scoped>
.Rainbow,
.Rainbow:hover {
  color: white;
  border-color: white;
  background: -webkit-linear-gradient(
      225deg,
      rgb(251, 175, 21),
      rgb(251, 21, 242),
      rgb(21, 198, 251)
    )
    0% 0% / 300% 300%;
  -webkit-animation: gradient_move 3s ease infinite;
  animation: gradient_move 3s ease infinite;
}
@-webkit-keyframes gradient_move {
  0% {
    background-position: 0% 92%;
  }
  50% {
    background-position: 100% 9%;
  }
  100% {
    background-position: 0% 92%;
  }
}

@keyframes gradient_move {
  0% {
    background-position: 0% 92%;
  }
  50% {
    background-position: 100% 9%;
  }
  100% {
    background-position: 0% 92%;
  }
}
</style>
