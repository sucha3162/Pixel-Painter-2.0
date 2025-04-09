<template>
  <FloatingCard
    position="bottom"
    header="Frames"
    button-icon="pi pi-images"
    button-label=""
    width=""
    :default-open="true">
    <Button
      class="mr-1"
      icon="pi pi-minus"
      size="small"
      rounded
      @click="removeFrame()" />

    <template v-for="frame in frames" :key="frame.id">
      <Button
        :icon="frame.icon"
        :class="frame.class"
        :severity="frame.severity"
        @click="switchFrame(frame.id)" />
    </template>

    <Button
      class="ml-1"
      icon="pi pi-plus"
      size="small"
      rounded
      @click="addFrame()" />
  </FloatingCard>
</template>

<script setup lang="ts">
import Button from "primevue/button";
import FloatingCard from "./FloatingCard.vue";
import { ref, onBeforeMount } from "vue";

const selectedFrame = ref<number>(1);
const oldFrame = ref<number>(1);
const index = ref<number>(2);

const frameCount = ref<number>(1);
const frames = ref([
  { id: 1, icon: "pi pi-image", class: "m-1", severity: "secondary" }
]);

onBeforeMount(() => {
  var count = 2;
  while (localStorage.getItem(`frame${count}`) != null) {
    frames.value.push({
      id: count,
      icon: "pi pi-image",
      class: "mr-1",
      severity: "secondary"
    });

    frameCount.value++;
    count++;
    index.value++;
  }
  frames.value[0].severity = "primary";
});

function addFrame() {
  frameCount.value++;
  index.value++;
  frames.value.push({
    id: frameCount.value,
    icon: "pi pi-image",
    class: "mr-1",
    severity: "secondary"
  });
}

function removeFrame() {
  if (frames.value.length > 1) {
    frames.value.pop();
    frameCount.value--;
    index.value--;

    if (selectedFrame.value == frameCount.value + 1) {
      localStorage.getItem(`frame${frameCount.value}`);
    }
  }

  if (localStorage.getItem(`frame${frameCount.value + 1}`) != null) {
    localStorage.removeItem(`frame${frameCount.value + 1}`);
  }
}

function switchFrame(frameID: number) {
  frames.value.forEach((nFrame) => {
    nFrame.severity = "secondary";
  });
  frames.value[frameID - 1].severity = "primary";

  oldFrame.value = selectedFrame.value;
  selectedFrame.value = frameID;
}
</script>

<style>
.p-dialog.p-component {
  margin-bottom: 75px !important;
}
</style>

