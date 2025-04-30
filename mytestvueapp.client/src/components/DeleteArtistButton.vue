<template>
  <Button
    label="Delete Artist"
    icon=""
    @click="visible = !visible"
    severity="danger"
    class="block m-2" />

  <Dialog
    v-model:visible="visible"
    modal
    :closable="false"
    :style="{ width: '25rem' }"
    :header="'Delete Account'">
    <Message icon="pi pi-times-circle" severity="error">
      This action cannot be undone.
    </Message>

    <div class="mt-4 mb-2">Confirm your Username to continue.</div>
    <InputText
      placeholder="Username"
      class="w-full"
      v-model="confirmText"
      autofocus />

    <template #footer>
      <Button
        label="Cancel"
        text
        severity="secondary"
        @click="visible = false" />
      <Button label="Confirm" severity="danger" @click="confirmDelete()" />
    </template>
  </Dialog>
</template>

<script setup lang="ts">
import Button from "primevue/button";
import Dialog from "primevue/dialog";
import InputText from "primevue/inputtext";
import { ref, watch, defineProps } from "vue";
import { useToast } from "primevue/usetoast";
import Artist from "@/entities/Artist";
import Message from "primevue/message";
import LoginService from "@/services/LoginService";

const props = defineProps<{
  artist: Artist;
}>();

const toast = useToast();
const visible = ref<boolean>(false);
const confirmText = ref<string>("");

watch(visible, (newVal) => {
  if (newVal) {
    confirmText.value = "";
  }
});

async function confirmDelete() {
  if (confirmText.value != props.artist.name) {
    toast.add({
      severity: "error",
      summary: "Error",
      detail: "Username's do not match",
      life: 3000
    });
    return;
  }
  LoginService.deleteArtist(props.artist.id)
    .then(() => {
      window.location.href = "/";
      toast.add({
        severity: "success",
        summary: "User Deleted",
        detail: `The user: ${props.artist.name} has been deleted successfully`,
        life: 3000
      });
    })
    .catch(() => {
      toast.add({
        severity: "error",
        summary: "Error",
        detail: "Error deleting user, please try again later",
        life: 3000
      });
    });
}
</script>

