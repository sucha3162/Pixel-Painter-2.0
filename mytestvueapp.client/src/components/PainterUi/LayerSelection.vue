<template>
  <FloatingCard 
  position="bottomleft"
  header="Layers [Right click to delete]"
  button-icon="pi pi-clone"
  button-label=""
  width=""
  :default-open="false">
    <div class="flex align-items-center justify-content-center">
      <Button class="mr-1" :disabled="layers.length == 1 || props.connected" icon="pi pi-minus" size="small" rounded @click="popLayer()" />
      <template v-for="layer in layers">
        <Button icon="pi pi-stop"
                :class="['m-1', { 'selected-layer': layer === selectedLayer } ]"
                severity="secondary"
                @click="switchLayer(layer)"
                @contextmenu.prevent="deleteLayer(layer)"
        />
      </template>
      <Button class="ml-1" :disabled="(layers.length==8 || props.connected)" icon="pi pi-plus" size="small" rounded @click="pushLayer()" />
    </div>
    <div class="mt-4 space-y-2">
      <Button
        :label="showLayers ? 'Hide Layers' : 'Show Layers'"
        :icon="showLayers ? 'pi pi-eye-slash' : 'pi pi-eye'"
        :severity="showLayersSeverity"
        :disabled="layers.length == 1"
        size="small"
        class="w-full"
        @click="showLayers = !showLayers; changeGreyscale()"
      />
      <Button
        :label="greyscale ? 'Full color layers' : 'Greyscale layers'"
        :icon="'pi pi-palette'"
        :severity="greyscaleSeverity "
        :disabled="(layers.length == 1 || !showLayers)"
        size="small"
        class="w-full"
        @click="greyscale = !greyscale"
      />
    </div>
  </FloatingCard>
</template>

<script setup lang="ts">
import Button from "primevue/button";
import FloatingCard from "./FloatingCard.vue";
import { useLayerStore } from "@/store/LayerStore"
import { PixelGrid } from "@/entities/PixelGrid"
import { Tooltip } from "primevue";

import { computed, ref, onBeforeMount, watch } from 'vue';

const props = defineProps<{
  updateLayers: number;
  connected: boolean;
}>();

const layerStore = useLayerStore();
const selectedLayer = ref<number>(0);
const layers = ref<number[]>([0]);

const showLayers = defineModel<boolean>("showLayers", { default: true });
const greyscale = defineModel<boolean>("greyscale", { default: false });

const isSingleLayer = computed(() => layers.value.length === 1)

const showLayersSeverity = computed(() => {
  if (showLayers.value && layers.value.length !== 1) {
    return 'primary';
  } else {
    return 'secondary';
  }
})

const greyscaleSeverity = computed(() => {
  if (layers.value.length === 1 || !showLayers.value) {
    return 'secondary';
  } else if (greyscale.value) {
    return 'primary';
  } else {
    return 'secondary';
  }
})

onBeforeMount(() => {
  for (let i = 1; i < layerStore.grids.length; i++)
    layers.value.push(i);
});

function pushLayer() {
  if (layers.value.length < 8) {
    layers.value.push(layers.value.length);
    layerStore.pushGrid(new PixelGrid(
      layerStore.grids[0].height,
      layerStore.grids[0].height,
      layerStore.grids[0].backgroundColor,
      layerStore.grids[0].isGif)
    );
  }
}

function deleteLayer(idx: number) {
  if (layers.value.length > 1 && !props.connected) {
    const isConfirmed = confirm("This will delete layer " + (idx+1) + ". Are you sure you want to delete this layer?");
    if (!isConfirmed) return;

    if (idx === -1) {
      console.warn(`Layer with id ${(idx+1)} not found`);
      return;
    }
    //remove specific layer
    layers.value.splice(idx, 1);
    layerStore.removeGrid(idx);


    layers.value.forEach((layer, index) => {
      layers.value[index] = index;
    });

    if (!layers.value.some(layer => layer === selectedLayer.value)) {
      selectedLayer.value = layers.value[layers.value.length - 1] ?? 1;
      layerStore.layer = selectedLayer.value;
    } else {
      selectedLayer.value--;
      layerStore.layer--;
    }
  }
}

function popLayer() {
  if (layers.value.length > 1) {

    layers.value.pop();
    layerStore.popGrid();
    if (!layers.value.some(layer => layer === selectedLayer.value)) {
      selectedLayer.value = layers.value[layers.value.length - 1] ?? 1;
      layerStore.layer = selectedLayer.value;
    }
  }
}

function switchLayer(layer: number) {
  selectedLayer.value = layer;
  layerStore.layer = layer;
}

function changeGreyscale() {
  if (!showLayers.value) {
    greyscale.value = false;
  }
};

watch(() => layers.value.length, () => {
  if (layers.value.length === 1) {
    greyscale.value = false;
  }
});

watch(() => props.updateLayers, () => {
  layers.value.splice(0, layers.value.length);
  for (let i = 0; i < layerStore.grids.length; i++) {
    layers.value.push(i);
  }
  selectedLayer.value = layerStore.layer;
});
</script>

<style scoped>
  .selected-layer {
    background-color: #555 !important;
    color: white !important;
  }
</style>