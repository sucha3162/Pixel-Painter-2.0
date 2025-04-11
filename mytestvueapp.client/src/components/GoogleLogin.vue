<template>
  <Button
    :label="isLoggedIn ? 'Account' : 'Login'"
    rounded
    @click="buttonClick()"
    icon="pi pi-google" />
</template>
<script setup lang="ts">
import Button from "primevue/button";
import { onMounted, ref } from "vue";
import router from "@/router";
import LoginService from "@/services/LoginService";

const isLoggedIn = ref<boolean>(false);

onMounted(async () => {
  LoginService.isLoggedIn().then((result) => {
    isLoggedIn.value = result;
  });
});

function buttonClick() {
  if (isLoggedIn.value) {
    router.push("/account#settings");
  } else {
    login();
  }
}

function login() {
  window.location.replace("/login/Login");
}
</script>

