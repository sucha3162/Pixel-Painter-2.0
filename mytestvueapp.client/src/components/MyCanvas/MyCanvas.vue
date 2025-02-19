<template>
  <div>
    <canvas :id="canvasId" class="vertical-align-middle"></canvas>
  </div>
</template>

<script setup lang="ts">
import type Art from "@/entities/Art";
import { onMounted, ref, computed, watch } from "vue";

const props = defineProps<{
  art: Art;
  pixelSize: number;
  canvasNumber?: number;
}>();

const canvasId = computed(() => {
  return `viewer-page-canavs-${props.canvasNumber}`;
});

const canvas = ref<HTMLCanvasElement>();
const context = ref<CanvasRenderingContext2D>();
onMounted(() => {
  updateCanvas();
});
watch(props, () => {
  updateCanvas();
});
function updateCanvas() {
  let canvasInit = document.getElementById(canvasId.value) as HTMLCanvasElement;
  if (canvasInit) {
    canvas.value = canvasInit;
    let contextInit = canvasInit.getContext("2d");
    if (contextInit) {
      context.value = contextInit;
      context.value.clearRect(0, 0, canvas.value.width, canvas.value.height);
      canvas.value.width = 32 * props.pixelSize;
      canvas.value.height = 32 * props.pixelSize;
      context.value.scale(
        32 / props.art.pixelGrid.width,
        32 / props.art.pixelGrid.width
      );
    }
  }
  render();
}
function render() {
  if (context.value && props.art.pixelGrid.encodedGrid) {
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
        context.value.fillStyle = "#" + imageServe.substring(hexBegin, hexEnd);
        context.value.fillRect(column, row, props.pixelSize, props.pixelSize);
        context.value.globalCompositeOperation = "lighter";
        hexBegin += 6;
        hexEnd += 6;
      }
    }
  }
}
</script>
