<template>
  <Button
    @click="openDialog = !openDialog"
    :label="buttonLabel"
    :severity="openDialog ? 'primary' : 'secondary'"
    :icon="buttonIcon"
    class="mr-2" />
  <Dialog
    :position="position"
    v-model:visible="openDialog"
    :header="header"
    :style="{ width: width }">
    <slot />
  </Dialog>
</template>
<script setup lang="ts">
import Dialog from "primevue/dialog";
import Button from "primevue/button";
import { onMounted, ref } from "vue";

const openDialog = ref<boolean>(false);

const props = defineProps<{
  position:
    | "center"
    | "top"
    | "bottom"
    | "left"
    | "right"
    | "topleft"
    | "topright"
    | "bottomleft"
    | "bottomright";
  header: string;
  width: string;
  buttonLabel: string;
  defaultOpen: boolean;
  buttonIcon: string;
}>();

onMounted(() => {
  openDialog.value = props.defaultOpen;
});
</script>

