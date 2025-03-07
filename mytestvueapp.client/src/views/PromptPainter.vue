<template>
  <div class="absolute w-full bottom-50">
    <Card class="flex align-items-center justify-content-start w-30rem m-auto">
      <template #title>Select Background Color And Canvas Resolution</template>
      <template #content>
        <label for="backgroundColorPick">Background Color: </label>
        <ColorPicker
          class="w-auto p-2"
          v-model="backgroundColor"
          id="backgroundColorPick"
          format="hex"
        ></ColorPicker>
        <br />
        <label for="resolution">Resolution: </label>
        <InputNumber
          class="w-auto p-2"
          id="resolution"
          v-model="resolution"
          showButtons
          buttonLayout="horizontal"
          suffix=" px"
          :min="1"
          :max="64"
        >
          <template #incrementbuttonicon>
            <span class="pi pi-plus" />
          </template>
          <template #decrementbuttonicon>
            <span class="pi pi-minus" />
          </template>
        </InputNumber>
        <br />
        <label for="type" class="mb-5">Type: </label>
        <ToggleButton
          id="type"
          class="mx-1 p-2 w-2.5"
          v-model="isImage"
          onLabel="Image"
          onIcon="pi pi-image"
          offLabel="GIF"
          offIcon="pi pi-images"
          @click=""
        />
      </template>
    </Card>
  </div>
  <div
    class="absolute bottom-0 bg-primary flex align-items-center justify-content-center w-full h-10rem"
  >
    <Button
      rounded
      label="Start Painting"
      icon="pi pi-pencil"
      @click="updateLocalStorage()"
    ></Button>
  </div>
</template>
<script setup lang="ts">
import Button from "primevue/button";
import ColorPicker from "primevue/colorpicker";
import Card from "primevue/card";
import InputNumber from "primevue/inputnumber";
import { onMounted, ref } from "vue";
import router from "@/router";
import { PixelGrid } from "@/entities/PixelGrid";
import ToggleButton from "primevue/togglebutton";

const resolution = ref<number>(32);
const backgroundColor = ref<string>("#ffffff");
const isImage = ref(true);

function updateLocalStorage() {
  var pixelGrid = new PixelGrid(
    resolution.value,
    resolution.value,
    backgroundColor.value,
    !isImage.value, // Constructor wants isGif so pass in !isImage
  );

  localStorage.setItem("working-art", JSON.stringify(pixelGrid));

  router.push("/paint");
}

onMounted(() => {
  var art = localStorage.getItem("working-art");
  if (art !== null && art != "null") {
    router.push("/paint");
  }
});
</script>
<style>
.p-colorpicker-preview {
  width: 7rem !important;
  height: 2rem !important;
  border: solid 1px !important;
}
</style>
