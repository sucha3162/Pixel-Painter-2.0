<template>
  <Button
    label="Delete Art"
    icon=""
    @click="visible = !visible"
    severity="danger"
    class="block" />

  <Dialog
    v-model:visible="visible"
    modal
    :closable="false"
    :style="{ width: '25rem' }"
    :header="'Delete ' + art.title + '?'">
    <Message icon="pi pi-times-circle" severity="error">
      This action cannot be undone.
    </Message>

    <div class="mt-4 mb-2">Confirm the title to continue.</div>
    <InputText
      placeholder="Title"
      class="w-full"
      v-model="confirmText"
      autofocus />

    <template #footer>
      <Button
        label="Cancel"
        text
        severity="secondary"
        @click="visible = false" />
      <Button
        label="Confirm"
        severity="danger"
        @click="confirmDelete()"
        :disabled="confirmText != art.title" />
    </template>
  </Dialog>
</template>

<script setup lang="ts">
import Button from "primevue/button";
import Dialog from "primevue/dialog";
import InputText from "primevue/inputtext";
import { ref, watch } from "vue";
import ArtAccessService from "@/services/ArtAccessService";
import { useToast } from "primevue/usetoast";
import router from "@/router";
import type Art from "@/entities/Art";
import Message from "primevue/message";

const toast = useToast();
const props = defineProps<{
  art: Art;
}>();
const visible = ref<boolean>(false);
const confirmText = ref<string>("");

watch(visible, (newVal) => {
  if (newVal) {
    confirmText.value = "";
  }
});

async function confirmDelete() {
  ArtAccessService.deleteContributingArtist(props.art.id)
    .then(() => {
      router.push("/account#art");
      toast.add({
        severity: "success",
        summary: "Art Deleted",
        detail: "The art has been deleted successfully",
        life: 3000
      });
    })
    .catch(() => {
      toast.add({
        severity: "error",
        summary: "Error",
        detail: "An error occurred while deleting the art",
        life: 3000
      });
    });
}
</script>

