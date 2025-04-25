<template>
  <DrawingCanvas
    ref="canvas"
    :style="{ cursor: cursor.selectedTool.cursor }"
    :grid="art.pixelGrid"
    :showLayers="showLayers"
    :greyscale="greyscale"
    v-model="cursor"
    @mousedown="
      mouseButtonHeldDown = true;
      setStartVector();
      setEndVector();
    "
    @mouseup="
      mouseButtonHeldDown = false;
      setEndVector();
      onMouseUp();
    "
    @contextmenu.prevent
  />
  <Toolbar class="fixed bottom-0 left-0 right-0 m-2">
    <template #start>
      <div class="flex gap-2">
        <Button
          icon="pi pi-ban"
          label="Quit"
          severity="secondary"
          @click="ResetArt()"
        >
        </Button>
        <UploadButton
          :art="art"
          :fps="fps"
          :connection="connection"
          :connected="connected"
          :group-name="groupName"
          @disconnect="disconnect"
          @OpenModal="ToggleKeybinds"
        />
        <SaveImageToFile :art="art" :fps="fps"></SaveImageToFile>
        <ConnectButton
          @openModal="ToggleKeybinds"
          @connect="connect"
          @disconnect="disconnect"
          :connected="connected"
          :isGif="art.pixelGrid.isGif"
        />
      </div>
    </template>

    <template #center>
      <ColorSelection
        v-model:color="cursor.color"
        v-model:size="cursor.size"
        :isBackground="false"
        @enable-key-binds="keyBindActive = true"
        @disable-key-binds="keyBindActive = false"
      />
      <BrushSelection v-model="cursor.selectedTool" />
      <ColorSelection
        v-model:color="art.pixelGrid.backgroundColor"
        v-model:size="cursor.size"
        :isBackground="true"
        @enable-key-binds="keyBindActive = true"
        @disable-key-binds="keyBindActive = false"
      />
      <FrameSelection
        v-if="art.pixelGrid.isGif"
        v-model:selFrame="selectedFrame"
        v-model:showLayers="showLayers"
      />
      <FPSSlider v-if="art.pixelGrid.isGif" v-model:fps="fps" />
      <LayerSelection
        v-if="!art.pixelGrid.isGif"
        :updateLayers="updateLayers"
        :connected="connected"
        v-model:showLayers="showLayers"
        v-model:greyscale="greyscale"
      />
    </template>
    <template #end>
      <Button
        icon="pi pi-times"
        class="mr-2"
        severity="primary"
        label="Clear"
        @click="clear()"
      />
      <Button
        icon="pi pi-expand"
        class="mr-2"
        severity="primary"
        label="Recenter"
        @click="canvas?.recenter()"
      />
      <Button
        :disabled="connected"
        :icon="intervalId != -1 ? 'pi pi-stop' : 'pi pi-play'"
        class="mr-2 Rainbow"
        label="Gravity"
        @click="runGravity()"
      />
      <Button
        icon="pi pi-lightbulb"
        class="Rainbow"
        label="Give Me Color!"
        @click="randomizeGrid()"
      />
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
import LayerSelection from "@/components/PainterUi/LayerSelection.vue";
import FPSSlider from "@/components/PainterUi/FPSSlider.vue";

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
import ConnectButton from "@/components/PainterUi/ConnectButton.vue";

//Other
import * as SignalR from "@microsoft/signalr";
import { useLayerStore } from "@/store/LayerStore";

//variables
const route = useRoute();
const canvas = ref();
const toast = useToast();
const intervalId = ref<number>(-1);
const keyBindActive = ref<boolean>(true);
const artist = ref<Artist>(new Artist());
const layerStore = useLayerStore();
const updateLayers = ref<number>(0);
const showLayers = ref<boolean>(true);
const greyscale = ref<boolean>(false);

// Connection Information
const connected = ref<boolean>(false);
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
  console.log("New Member: " + newartist.name);
  if (!art.value.artistId.includes(newartist.id)) {
    art.value.artistId.push(newartist.id);
    art.value.artistName.push(newartist.name);
  }
  console.log("NewMember-Members: " + art.value.artistName.join(" "));
});

connection.on("Members", (artists: Artist[]) => {
  console.log("Recieved All Members");
  artists.forEach((artist) => {
    if (!art.value.artistId.includes(artist.id)) {
      art.value.artistId.push(artist.id);
      art.value.artistName.push(artist.name);
    }
  });
  console.log("Members-Members: " + art.value.artistName.join(" "));
});

connection.onclose((error) => {
  if (error) {
    toast.add({
      severity: "error",
      summary: "Error",
      detail: "You have disconnected!",
      life: 3000
    });
    connected.value = false;
  }
});

connection.on(
  "ReceivePixels",
  (layer: number, color: string, coords: Vector2[]) => {
    DrawPixels(layer, color, coords);
  }
);

connection.on(
  "GroupConfig",
  (canvasSize: number, backgroundColor: string, pixels: Pixel[][]) => {
    layerStore.empty();

    art.value.pixelGrid.width = canvasSize;
    art.value.pixelGrid.height = canvasSize;
    art.value.pixelGrid.backgroundColor = backgroundColor;
    art.value.pixelGrid.grid = art.value.pixelGrid.createGrid(
      canvasSize,
      canvasSize
    );
    ReplaceCanvas(pixels);
    updateLayers.value = layerStore.grids.length;

    canvas.value?.drawLayers(0);
    canvas.value?.recenter();
  }
);

connection.on("BackgroundColor", (backgroundColor: string) => {
  art.value.pixelGrid.backgroundColor = backgroundColor;
});

const connect = (groupname: string) => {
  if (artist.value.id != 0) {
    connection
      .start()
      .then(() => {
        let grids = layerStore.getGridArray();
        console.log("Connected to SignalR!");
        connection.invoke(
          "CreateOrJoinGroup",
          groupname,
          artist.value,
          grids,
          layerStore.grids[0].width,
          layerStore.grids[0].backgroundColor
        );
        groupName.value = groupname;
        connected.value = !connected.value;
        art.value.artistId = [artist.value.id];
        art.value.artistName = [artist.value.name];
      })
      .catch((err) => console.error("Error connecting to Hub:", err));
  } else {
    toast.add({
      severity: "error",
      summary: "Error",
      detail: "Please log in before collaborating!",
      life: 3000
    });
  }
};

const disconnect = () => {
  connection.invoke("LeaveGroup", groupName.value, artist.value)
    .then(() => {
      connection
        .stop()
        .then(() => {
          connected.value = !connected.value;
        })
        .catch((err) => console.error("Error Disconnecting:", err));
    })
    .catch((err) => console.error("Error Leaving Group:", err));
};
//End of Connection Information
const cursor = ref<Cursor>(
  new Cursor(new Vector2(-1, -1), PainterTool.getDefaults()[1], 1, "000000")
);

const mouseButtonHeldDown = ref<boolean>(false);

const startPix = ref<Vector2>(new Vector2(0, 0));
const endPix = ref<Vector2>(new Vector2(0, 0));
let tempGrid: string[][] = [];

const art = ref<Art>(new Art());
//if anyone has an easier way to set this lmk
art.value.isGif = layerStore.grids[0].isGif;
art.value.pixelGrid.isGif = layerStore.grids[0].isGif;

let selectedFrame = ref(1);
let lastFrame = ref(1);
let index = ref(1);
const fps = ref<number>(4);

let currentPallet: string[];
function updatePallet() {
  let temp = localStorage.getItem("currentPallet");
  if (temp) currentPallet = JSON.parse(temp);
  for (let i = 0; i < currentPallet.length; i++) {
    if (currentPallet[i] === null || currentPallet[i] === "") {
      currentPallet[i] = "000000";
    }
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
    LocalSave();
  }
  next();
});

onMounted(async () => {
  document.addEventListener("keydown", handleKeyDown);
  window.addEventListener("beforeunload", handleBeforeUnload);

  //Get the current user
  LoginService.GetCurrentUser().then((user: Artist) => {
    if (user.id == 0) {
      artist.value.id = 0;
      artist.value.name = "Guest";
    }
    artist.value = user;
  });

  if (route.params.id) {
    const id: number = parseInt(route.params.id as string);
    ArtAccessService.getArtById(id)
      .then((data) => {
        art.value.id = data.id;
        art.value.title = data.title;
        art.value.isPublic = data.isPublic;

        canvas.value?.recenter();
        art.value.pixelGrid.backgroundColor =
          layerStore.grids[0].backgroundColor;
      })
      .catch(() => {
        toast.add({
          severity: "error",
          summary: "Error",
          detail: "You cannot edit this art",
          life: 3000
        });
        router.push("/new");
      });
  } else if (layerStore.grids.length === 0) {
    router.push("/new");
  } else {
    canvas.value?.recenter();
    art.value.pixelGrid.isGif = layerStore.grids[0].isGif;
    art.value.pixelGrid.backgroundColor = layerStore.grids[0].backgroundColor;
    art.value.pixelGrid.width = layerStore.grids[0].width;
    art.value.pixelGrid.height = layerStore.grids[0].height;
    tempGrid = JSON.parse(JSON.stringify(layerStore.grids[0].grid));
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
  LocalSave();
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

watch(
  () => art.value.pixelGrid.backgroundColor,
  (next, prev) => {
    ChangeBackgroundColor(next);
    for (let i = 0; i < layerStore.grids.length; i++) {
      layerStore.grids[i].backgroundColor = next;
    }
  }
);

watch(selectedFrame, () => {
  const workingGrid = layerStore.grids[selectedFrame.value];

  if (workingGrid == null) {
    const newGrid = new PixelGrid(
      art.value.pixelGrid.width,
      art.value.pixelGrid.height,
      art.value.pixelGrid.backgroundColor,
      art.value.pixelGrid.isGif
    );
    layerStore.grids[0].DeepCopy(newGrid);
    canvas.value?.drawLayers(0);

    canvas.value?.recenter();
  } else {
    canvas.value?.recenter();
  }
});

//functions
watch(
  () => layerStore.layer,
  (next, prev) => {
    layerStore.layer = Math.max(next, 0);
    tempGrid = JSON.parse(JSON.stringify(layerStore.grids[layerStore.layer].grid));
    canvas.value?.drawLayers(layerStore.layer);
  }
);

//functions

function runGravity() {
  if (intervalId.value != -1) {
    clearInterval(intervalId.value);
    intervalId.value = -1;
  } else {
    intervalId.value = setInterval(
      fallingSand,
      30,
      layerStore.grids[layerStore.layer]
    );
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

function ReplaceCanvas(pixels: Pixel[][]) {
  for (let l = 0; l < pixels.length; l++) {
    layerStore.pushGrid(
      new PixelGrid(
        art.value.pixelGrid.width,
        art.value.pixelGrid.height,
        art.value.pixelGrid.backgroundColor,
        false
      )
    );
    for (let p = 0; p < pixels[l].length; p++) {
      layerStore.grids[l].grid[pixels[l][p].x][pixels[l][p].y] =
        pixels[l][p].color;
    }
  }
}

function DrawPixels(layer: number, color: string, coords: Vector2[]) {
  for (const coord of coords) {
    layerStore.grids[layer].grid[coord.x][coord.y] = color;
    canvas.value?.updateCell(layer, coord.x, coord.y, color);
  }
}

function SendPixels(layer: number, color: string, coords: Vector2[]) {
  if (connected.value) {
    connection.invoke("SendPixels", groupName.value, layer, color, coords);
  }
}

function ChangeBackgroundColor(color: string) {
  if (connected.value) {
    connection.invoke("ChangeBackgroundColor", groupName.value, color);
  }
}

function DrawAtCoords(coords: Vector2[]) {
  let coordinates: Vector2[] = [];

  if (
    cursor.value.selectedTool.label === "Rectangle" ||
    cursor.value.selectedTool.label === "Ellipse"
  ) {
    if (tempGrid) {
      for (let i = 0; i < layerStore.grids[layerStore.layer].height; i++) {
        for (let j = 0; j < layerStore.grids[layerStore.layer].width; j++) {
          layerStore.grids[layerStore.layer].grid[i][j] = tempGrid[i][j];

          canvas.value?.updateCell(layerStore.layer, i, j, tempGrid[i][j]);
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
              coord.x + i < layerStore.grids[layerStore.layer].width &&
              coord.y + j >= 0 &&
              coord.y + j < layerStore.grids[layerStore.layer].height
            ) {
              coordinates.push(new Vector2(coord.x + i, coord.y + j));
              layerStore.grids[layerStore.layer].grid[coord.x + i][
                coord.y + j
              ] = cursor.value.color;

              if (!layerStore.grids[0].isGif) {
                canvas.value?.updateCell(
                  layerStore.layer,
                  coord.x + i,
                  coord.y + j,
                  cursor.value.color
                );
              }
            }
          }
        }
        SendPixels(layerStore.layer, cursor.value.color, coordinates);
      } else if (cursor.value.selectedTool.label === "Eraser") {
        for (let i = 0; i < cursor.value.size; i++) {
          for (let j = 0; j < cursor.value.size; j++) {
            if (
              coord.x + i >= 0 &&
              coord.x + i < layerStore.grids[layerStore.layer].width &&
              coord.y + j >= 0 &&
              coord.y + j < layerStore.grids[layerStore.layer].height
            ) {
              if (art.value.pixelGrid.backgroundColor != null) {
                coordinates.push(new Vector2(coord.x + i, coord.y + j));
                layerStore.grids[layerStore.layer].grid[coord.x + i][
                  coord.y + j
                ] = "empty";
                canvas.value?.updateCell(
                  layerStore.layer,
                  coord.x + i,
                  coord.y + j,
                  "empty"
                );
              }
            }
          }
        }
        SendPixels(layerStore.layer, "empty", coordinates);
      } else if (
        coord.x >= 0 &&
        coord.x < layerStore.grids[layerStore.layer].width &&
        coord.y >= 0 &&
        coord.y < layerStore.grids[layerStore.layer].height
      ) {
        if (cursor.value.selectedTool.label === "Pipette") {
          let tmp = layerStore.grids[layerStore.layer].grid[coord.x][coord.y];
          if (tmp === "empty")
            cursor.value.color = art.value.pixelGrid.backgroundColor;
        } else if (cursor.value.selectedTool.label === "Bucket") {
          if (
            layerStore.grids[layerStore.layer].grid[coord.x][coord.y] !=
            cursor.value.color
          ) {
            coordinates = fill(
              cursor.value.position.x,
              cursor.value.position.y
            );
            SendPixels(layerStore.layer, cursor.value.color, coordinates);
          }
        } else if (
          cursor.value.selectedTool.label === "Rectangle" ||
          cursor.value.selectedTool.label === "Ellipse"
        ) {
          layerStore.grids[layerStore.layer].grid[coord.x][coord.y] =
            cursor.value.color;

          canvas.value?.updateCell(
            layerStore.layer,
            coord.x,
            coord.y,
            cursor.value.color
          );
        }
      }
    }
  });
  if (layerStore.grids[0].isGif) {
    canvas.value?.drawLayers(layerStore.layer);
  }
}

function fill(
  x: number,
  y: number,
  color: string = cursor.value.color
): Vector2[] {
  let vectors: Vector2[] = [];
  if (y >= 0 && y < layerStore.grids[layerStore.layer].height) {
    const oldColor = layerStore.grids[layerStore.layer].grid[x][y];
    layerStore.grids[layerStore.layer].grid[x][y] = color;

    canvas.value?.updateCell(layerStore.layer, x, y, color);

    vectors.push(new Vector2(x, y));
    if ("empty" !== color) {
      if (x + 1 < layerStore.grids[layerStore.layer].width) {
        if (layerStore.grids[layerStore.layer].grid[x + 1][y] === oldColor) {
          vectors = vectors.concat(fill(x + 1, y, color));
        }
      }
      if (y + 1 < layerStore.grids[layerStore.layer].height) {
        if (layerStore.grids[layerStore.layer].grid[x][y + 1] === oldColor) {
          vectors = vectors.concat(fill(x, y + 1, color));
        }
      }
      if (x - 1 >= 0) {
        if (layerStore.grids[layerStore.layer].grid[x - 1][y] === oldColor) {
          vectors = vectors.concat(fill(x - 1, y, color));
        }
      }
      if (y - 1 >= 0) {
        if (layerStore.grids[layerStore.layer].grid[x][y - 1] === oldColor) {
          vectors = vectors.concat(fill(x, y - 1, color));
        }
      }
    }
  }

  return vectors;
}

function randomizeGrid() {
  for (let i = 0; i < layerStore.grids[layerStore.layer].height; i++) {
    for (let j = 0; j < layerStore.grids[layerStore.layer].width; j++) {
      let color = ((Math.random() * 0xffffff) << 0)
        .toString(16)
        .padStart(6, "0");
      layerStore.grids[layerStore.layer].grid[i][j] = color;

      canvas.value?.updateCell(layerStore.layer, i, j, color);

      if (connected.value) {
        let coords: Vector2[] = [];
        coords.push(new Vector2(i, j));
        SendPixels(layerStore.layer, color, coords);
      }
    }
  }
  layerStore.grids[layerStore.layer].encodedGrid =
    layerStore.grids[layerStore.layer].getEncodedGrid();
}

function fallingSand() {
  let pixelGrid: PixelGrid = layerStore.grids[layerStore.layer];

  for (let x = 0; x < pixelGrid.width; x++) {
    for (let y = pixelGrid.height - 1; y >= 0; y--) {
      if (pixelGrid.grid[x][y] !== "empty") {
        if (y + 1 < pixelGrid.height && pixelGrid.grid[x][y + 1] === "empty") {
          const below = pixelGrid.grid[x][y + 1];
          pixelGrid.grid[x][y + 1] = pixelGrid.grid[x][y];

          canvas.value?.updateCell(
            layerStore.layer,
            x,
            y + 1,
            pixelGrid.grid[x][y]
          );

          pixelGrid.grid[x][y] = below;
          canvas.value?.updateCell(layerStore.layer, x, y, below);
        } else {
          //generate a random number either -1 or 1
          const random = Math.random() > 0.5 ? 1 : -1;

          if (
            y + 1 < pixelGrid.height &&
            x + random < pixelGrid.width &&
            x + random >= 0 &&
            pixelGrid.grid[x + random][y + 1] === "empty"
          ) {
            const belowRight = pixelGrid.grid[x + random][y + 1];
            pixelGrid.grid[x + random][y + 1] = pixelGrid.grid[x][y];

            canvas.value?.updateCell(
              layerStore.layer,
              x + random,
              y + 1,
              pixelGrid.grid[x][y]
            );
            pixelGrid.grid[x][y] = belowRight;

            canvas.value?.updateCell(layerStore.layer, x, y, belowRight);
          }
        }
      }
    }
  }
}

function clear(): void {
  let coords: Vector2[] = [];
  for (let i = 0; i < layerStore.grids[layerStore.layer].width; i++) {
    for (let j = 0; j < layerStore.grids[layerStore.layer].height; j++) {
      layerStore.grids[layerStore.layer].grid[i][j] = "empty";
      canvas.value?.updateCell(layerStore.layer, i, j, "empty");
      coords.push(new Vector2(i, j));
    }
  }

  if (connected.value) {
    SendPixels(layerStore.layer, "empty", coords);
  }
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
  tempGrid = JSON.parse(
    JSON.stringify(layerStore.grids[layerStore.layer].grid)
  );
}
function setEndVector() {
  if (mouseButtonHeldDown.value) {
    endPix.value = new Vector2(
      cursor.value.position.x,
      cursor.value.position.y
    );
  } else {
    tempGrid = JSON.parse(
      JSON.stringify(layerStore.grids[layerStore.layer].grid)
    );
  }
}

function ResetArt() {
  layerStore.clearStorage();
  layerStore.empty();

  if (art.value.pixelGrid.isGif) {
    let tempCount = 0;
    while (localStorage.getItem(`frame${tempCount}`) != null) {
      localStorage.removeItem(`frame${tempCount}`);
      tempCount++;
    }
  }

  router.push("/new");
}

function onMouseUp() {
  if (cursor.value.selectedTool.label == "Rectangle") {
    SendPixels(
      layerStore.layer,
      cursor.value.color,
      GetRectanglePixels(startPix.value, endPix.value)
    );
  }
  if (cursor.value.selectedTool.label == "Ellipse") {
    CalculateEllipse(startPix.value, endPix.value).forEach((vector) => {});
    SendPixels(
      layerStore.layer,
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
  layerStore.save();
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
