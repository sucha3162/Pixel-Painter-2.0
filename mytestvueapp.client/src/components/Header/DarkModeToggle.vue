<template>
  <Button
    rounded
    :icon="checked ? 'pi pi-sun' : 'pi pi-moon'"
    @click="checked = !checked"></Button>
</template>
<script setup lang="ts">
import Button from "primevue/button";
import { onMounted, ref, watch } from "vue";
import { useThemeStore } from "@/store/ThemeStore";

const checked = ref<boolean>(false);
const store = useThemeStore();

watch(checked, () => {
  if (checked.value) {
    const element = document.querySelector("html");
    element?.classList.toggle("dark-mode-toggle");
    store.Theme = "light";
  } else {
    const element = document.querySelector("html");
    element?.classList.toggle("dark-mode-toggle");
    store.Theme = "dark";
  }
});

onMounted(() => {
  const element = document.querySelector("html");
  element?.classList.toggle("dark-mode-toggle");
});
</script>

