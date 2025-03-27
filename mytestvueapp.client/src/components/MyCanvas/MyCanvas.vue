<template>
  <div>
    <canvas :id="canvasId" class="vertical-align-middle" ref="canvas"></canvas>
  </div>
</template>

<script setup lang="ts">
import type Art from "@/entities/Art";
import { ref, onMounted, watch, computed } from "vue";

const props = defineProps<{
  modelValue?: string;
  art: Art;
  pixelSize: number;
  canvasNumber?: number;
}>(); // v-model
const emit = defineEmits(["update:modelValue"]); // Event to update parent

const canvas = ref<HTMLCanvasElement | null>(null);
const ctx = ref<CanvasRenderingContext2D | null>(null);

// Computed property for syncing v-model
const localGrid = computed({
  get: () => props.modelValue, // Get the color from the parent
  set: (newGrid) => emit("update:modelValue", newGrid), // Send update to parent
});
const canvasId = computed(() => {
  return `viewer-page-canavs-${props.canvasNumber}`;
});
    watch(props, () => {
        updateCanvas();
    });

onMounted(() => {
  if (canvas.value) {
    ctx.value = canvas.value.getContext("2d");
    updateCanvas();
  }
});

// Watch for color changes (from parent or local input)
watch(localGrid, () => {
  drawSquare();
});

function drawSquare() {
  if (!ctx.value || !canvas.value) return;
  ctx.value.clearRect(0, 0, canvas.value.width, canvas.value.height); // Clear
  renderfilter();
}

function updateCanvas() {
  let canvasInit = document.getElementById(canvasId.value) as HTMLCanvasElement;
  if (canvasInit) {
    canvas.value = canvasInit;
    let contextInit = canvasInit.getContext("2d");
    if (contextInit) {
      ctx.value = contextInit;
      ctx.value.clearRect(0, 0, canvas.value.width, canvas.value.height);
      canvas.value.width = 32 * props.pixelSize;
      canvas.value.height = 32 * props.pixelSize;
      ctx.value.scale(
        32 / props.art.pixelGrid.width,
        32 / props.art.pixelGrid.width,
      );
    }
  }
  render();
}
function render() {
  if (ctx.value && props.art.pixelGrid.encodedGrid) {
    const imageServe = props.art.pixelGrid.encodedGrid;
    var hexBegin = 0;
    var hexEnd = 6;
    for (
      let column = 0;
      column < props.art.pixelGrid.width * props.pixelSize;
      column += props.pixelSize
    ) {
      for (
        let row = 0;
        row < props.art.pixelGrid.height * props.pixelSize;
        row += props.pixelSize
      ) {
        ctx.value.fillStyle = "#" + imageServe.substring(hexBegin, hexEnd);
        ctx.value.fillRect(column, row, props.pixelSize, props.pixelSize);
        ctx.value.globalCompositeOperation = "lighter";
        hexBegin += 6;
        hexEnd += 6;
      }
    }
  }
}
function renderfilter() {
  if (ctx.value && props.art.pixelGrid.encodedGrid) {
    const imageServe = props.modelValue ? props.modelValue : "";
    var hexBegin = 0;
    var hexEnd = 6;
    for (
      let column = 0;
      column < props.art.pixelGrid.width * props.pixelSize;
      column += props.pixelSize
    ) {
      for (
        let row = 0;
        row < props.art.pixelGrid.height * props.pixelSize;
        row += props.pixelSize
      ) {
        ctx.value.fillStyle = "#" + imageServe.substring(hexBegin, hexEnd);
        ctx.value.fillRect(column, row, props.pixelSize, props.pixelSize);
        ctx.value.globalCompositeOperation = "lighter";
        hexBegin += 6;
        hexEnd += 6;
      }
    }
  }
}
</script>
