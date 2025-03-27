<template>
  <FloatingCard position="bottom"
                header="Layers"
                button-icon="pi pi-clone"
                button-label=""
                width=""
                :default-open="true">

    <Button class="mr-1" :disabled="layers.length == 1" icon="pi pi-minus" size="small" rounded @click="popLayer()" />

    <template v-for="layer in layers">
      <Button icon="pi pi-stop"
              :class="['m-1', { 'selected-layer': layer === selectedLayer } ]"
              severity="secondary"
              @click="selectedLayer = layer"
              @contextmenu.prevent="deleteLayer(layer)"/>
    </template>

    <Button class="ml-1" :disabled="layers.length==8" icon="pi pi-plus" size="small" rounded @click="pushLayer()" />
  </FloatingCard>
</template>

<script setup lang="ts">
import Button from "primevue/button";
import FloatingCard from "./FloatingCard.vue";
import { ref } from 'vue';

const selectedLayer = ref<number>(1);

const layers = ref<number[]>([1]);

function pushLayer() {
  if (layers.value.length < 8) {
    layers.value.push(layers.value.length+1);
  }
}

function deleteLayer(id) {
  if (layers.value.length > 1) {
    const isConfirmed = confirm("This will delete layer " + id + ". Are you sure you want to delete this layer?");
    if (!isConfirmed) return;

    const index = id - 1;

    if (index === -1) {
      console.warn(`Layer with id ${id} not found`);
      return;
    }
    layers.value.splice(index, 1);

    layers.value.forEach((layer, idx) => {
      layer = idx + 1;
    });
  }
}

function popLayer() {
  if (layers.value.length > 1) {

    layers.value.pop();
    if (!layers.value.some(layer => layer === selectedLayer.value)) {
      selectedLayer.value = layers.value[layers.value.length - 1] ??  1;
    }
  }
}
</script>

<style scoped>
  .selected-layer {
    background-color: #555 !important;
    color: white !important;
  }
</style>