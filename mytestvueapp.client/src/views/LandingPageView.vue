<template>
  <div class="p-8">
    <div class="text-8xl font-bold">Pixel Painter</div>
    <div class="text-5xl">A somewhat minimalist painting application</div>
    <Button
      class="mt-4 text-2xl"
      label="Get Started"
      @click="router.push('/new')"
    />

    <img
      src="../assets/images/pop_cat.gif"
      alt="cat"
      style="position: fixed; bottom: 0; right: 0; height: 60%" />
  </div>
</template>

<script setup lang="ts">
import Button from "primevue/button";
import router from "@/router";
</script>

