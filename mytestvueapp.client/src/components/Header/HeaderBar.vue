<template>
  <Toolbar class="border-none custom-background">
    <template #start>
      <RouterLink class="router-link-unstyled" to="/">
        <h1 class="m-0 ml-2 font-bold">
          <span style="color: var(--p-primary-color)">Pixel</span>Painter
        </h1>
      </RouterLink>
    </template>
    <template #center>
      <div>
        <RouterLink class="p-2" to="/">
          <Button rounded label="Home" icon="pi pi-home" />
        </RouterLink>
        <RouterLink class="p-2" to="/new">
          <Button rounded label="Painter" icon="pi pi-pencil" />
        </RouterLink>
        <RouterLink class="p-2" to="/gallery">
          <Button rounded label="Gallery" icon="pi pi-image" />
        </RouterLink>
      </div>
    </template>
    <template #end>
      <RouterLink class="mr-2" to="/notifications" v-if="isLoggedIn">
        <Notification />
      </RouterLink>
      <DarkModeSwitcher class="mr-2" />
      <GoogleLogin></GoogleLogin>
    </template>
  </Toolbar>
</template>

<script setup lang="ts">
import { RouterLink } from "vue-router";
import { ref, onMounted } from "vue";
import Button from "primevue/button";
import DarkModeSwitcher from "./DarkModeToggle.vue";
import GoogleLogin from "../GoogleLogin.vue";
import Toolbar from "primevue/toolbar";
import Notification from "./NotificationRedirect.vue";
import LoginService from "@/services/LoginService";
import { useLayerStore } from "@/store/LayerStore"
import { useArtistStore } from "@/store/ArtistStore"

const layerStore = useLayerStore();
const artistStore = useArtistStore();

const isLoggedIn = ref(false);

onMounted(async () => {
  layerStore.init();
  artistStore.init();

  LoginService.isLoggedIn().then((result) => {
    isLoggedIn.value = result;
  });
});
//Will want to start calculating
</script>

<style scoped>
/* Prevents the UI from getting crushed. */

.custom-background {
  background-color: transparent;
}
</style>

