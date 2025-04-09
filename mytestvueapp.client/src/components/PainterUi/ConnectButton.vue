<template>
  <Button
    :label="connected ? 'Disconnect' : 'Connect'"
    :severity="connected ? 'danger' : 'primary'"
    :disabled="isGif"
    icon="pi pi-wifi"
    @click="toggleModal()" />

  <Dialog v-model:visible="visible" modal :style="{ width: '25rem' }">
    <template #header>
      <h1 class="mr-2">Connect?</h1>
    </template>

    <div class="flex align-items-center gap-3">
      <span>Group: </span>
      <InputText v-model="groupname" placeholder="group-name" class="w-full" />
    </div>

    <template #footer>
      <Button
        label="Cancel"
        text
        severity="secondary"
        @click="visible = false"
        autofocus />
      <Button
        label="Connect"
        severity="secondary"
        @click="connect()"
        autofocus />
    </template>
  </Dialog>
</template>

<script setup lang="ts">
import { ref, watch } from "vue";
import Button from "primevue/button";
import Dialog from "primevue/dialog";
import InputText from "primevue/inputtext";

const emit = defineEmits(["OpenModal", "Connect", "Disconnect"]);

const props = defineProps<{
  connected: boolean;
  isGif: boolean;
}>();

const visible = ref<boolean>(false);
const groupname = ref<string>("");

function toggleModal() {
  if (!props.connected) {
    visible.value = !visible.value;
  } else {
    disconnect();
  }
}

function connect() {
  emit("Connect", groupname.value);
  visible.value = !visible.value;
}

function disconnect() {
  emit("Disconnect", groupname.value);
  if (!props.connected) {
    toggleModal();
  }
}

watch(visible, () => {
  emit("OpenModal", visible.value);
});
</script>

