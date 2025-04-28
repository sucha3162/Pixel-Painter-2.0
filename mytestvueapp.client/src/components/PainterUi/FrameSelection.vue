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
                @click="switchFrame(frame.id)" 
                @contextmenu.prevent="deleteFrame(frame.id)"/>
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
import { ref, onBeforeMount, nextTick } from "vue";
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
      class: "m-1",
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
    class: "m-1",
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
  switchFrame(frames.value.length - 1);
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

  if (frameID == selectedFrame.value && selectedFrame.value === 0) {
		frameStore.layer = -1;
    nextTick(() => {
      frameStore.layer = 0;
    });
  } else {
		frameStore.layer = frameID;
  }
	selectedFrame.value = frameID;
}

  function deleteFrame(frameID: number) {
    if (frames.value.length > 1) {
      if (confirm("Are you sure you want to delete frame " + (frameID + 1) + "?")) {
        frames.value.splice(frameID, 1);
				frameStore.removeGrid(frameID);

        for (var i = 0; i < frames.value.length; i++) {
					frames.value[i].id = i;
        }

        if (selectedFrame.value == 0) {
					switchFrame(selectedFrame.value);
				} else if (selectedFrame.value >= frameID) {
          switchFrame(selectedFrame.value - 1);
        }
      }
    }
  }

</script>

<style>
.p-dialog.p-component {
  margin-bottom: 75px !important;
}
</style>
