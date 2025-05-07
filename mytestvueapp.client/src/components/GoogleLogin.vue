<template>
  <Button
    :label="isLoggedIn ? 'Account' : 'Login'"
    rounded
    @click="buttonClick()"
    icon="pi pi-google"
  />
</template>
<script setup lang="ts">
import Button from "primevue/button";
import { onMounted, ref } from "vue";
import router from "@/router";
import LoginService from "@/services/LoginService";
import Artist from "@/entities/Artist";

const isLoggedIn = ref<boolean>(false);

onMounted(async () => {
  LoginService.isLoggedIn().then((result) => {
    isLoggedIn.value = result;
  });
});

function buttonClick(): void {
  if (isLoggedIn.value) {
    LoginService.getCurrentUser().then((user: Artist) => {
      router.push(`/accountpage/${user.name}`);
    });
  } else {
    login();
  }
}

function login(): void {
  window.location.replace("/login/Login");
}
</script>
