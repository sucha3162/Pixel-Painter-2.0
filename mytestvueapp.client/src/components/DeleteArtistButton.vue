<template>
  <Button
    label="Delete Artist"
    icon=""
    @click="visible = !visible"
    severity="danger"
    class="block m-2"></Button>

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
      autofocus></InputText>

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
import { ref, watch } from "vue";
import { useToast } from "primevue/usetoast";

import Message from "primevue/message";
import LoginService from "@/services/LoginService";

const toast = useToast();
const visible = ref<boolean>(false);
const confirmText = ref<string>("");

watch(visible, (newVal) => {
  if (newVal) {
    confirmText.value = "";
  }
});

async function confirmDelete() {
  LoginService.deleteArtist(confirmText.value)
    .then(() => {
      window.location.href = "/";
      toast.add({
        severity: "success",
        summary: "User Deleted",
        detail: "The User has been deleted successfully",
        life: 3000
      });
    })
    .catch(() => {
      toast.add({
        severity: "error",
        summary: "Error",
        detail:
          "error deleting user, please make sure you have spelt it correctly",
        life: 3000
      });
    });
}
</script>

