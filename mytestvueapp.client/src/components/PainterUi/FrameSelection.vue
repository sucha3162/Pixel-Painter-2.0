<template>
  <FloatingCard position="bottom"
                header="Frames"
                button-icon="pi pi-images"
                button-label=""
                width=""
                :default-open="true">

    <div>
      <Button class="mr-1"
              icon="pi pi-minus"
              size="small"
              rounded
              @click="removeFrame()" />

      <template v-for="frame in frames" :key="frame.id">
        <Button :icon="frame.icon"
                :class="frame.class"
                :severity="frame.severity"
                @click="switchFrame(frame.id)" />
      </template>

      <Button class="ml-1"
              icon="pi pi-plus"
              size="small"
              rounded
              @click="addFrame()" />
    </div>
    <div class="flex justify-center w-full">
      <Button class="w-full"
              :label="showPrevFrame ? 'Hide Previous Frame' : 'Show Previous Frame'"
              :severity="buttonSeverity"
              icon=""
              size="small"
              rounded
              @click="showPrevFrame = !showPrevFrame; changeSeverity()" />
    </div>
  </FloatingCard>
</template>

<script setup lang="ts">
import Button from "primevue/button";
import FloatingCard from "./FloatingCard.vue";
import { ref, onBeforeMount } from "vue";
import { useLayerStore } from "@/store/LayerStore";
import { PixelGrid } from "@/entities/PixelGrid";

const selectedFrame = defineModel<number>("selFrame", { default: 0 });
const frameStore = useLayerStore();
const frames = ref([
  { id: 0, icon: "pi pi-image", class: "m-1", severity: "secondary" }
]);
const showPrevFrame = defineModel<boolean>("showLayers", { default: true });
let buttonSeverity = "secondary";

onBeforeMount(() => {
  for (let i = 1; i < frameStore.grids.length; i++) {
    frames.value.push({
      id: i,
      icon: "pi pi-image",
      class: "mr-1",
      severity: "secondary"
    });
  }

  frames.value[0].severity = "primary";
});

  function changeSeverity() {
    if (showPrevFrame.value) {
      buttonSeverity = "primary";
    } else {
      buttonSeverity = "secondary";
    }
  }

function addFrame() {
  frames.value.push({
    id: frameStore.grids.length,
    icon: "pi pi-image",
    class: "mr-1",
    severity: "secondary"
  });
  frameStore.pushGrid(
    new PixelGrid(
      frameStore.grids[0].height,
      frameStore.grids[0].height,
      frameStore.grids[0].backgroundColor,
      frameStore.grids[0].isGif
    )
  );
}

function removeFrame() {
  if (frames.value.length > 1) {
    frames.value.pop();
    frameStore.popGrid();

    if (selectedFrame.value >= frames.value.length) {
      switchFrame(selectedFrame.value - 1);
    }
  }
}

function switchFrame(frameID: number) {
  frames.value.forEach((nFrame) => {
    nFrame.severity = "secondary";
  });
  frames.value[frameID].severity = "primary";

  selectedFrame.value = frameID;
  frameStore.layer = frameID;
}
</script>

<style>
.p-dialog.p-component {
  margin-bottom: 75px !important;
}
</style>
