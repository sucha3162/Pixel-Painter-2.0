<template>
  <Button
    :label="isLoggedIn ? 'Account' : 'Login'"
    @click="buttonClick()"
    icon="pi pi-google"
  ></Button>
</template>
<script setup lang="ts">
import Button from "primevue/button";
import { onMounted, ref } from "vue";
import router from "@/router";
import LoginService from "@/services/LoginService";

const isLoggedIn = ref(false);

onMounted(() => {
  LoginService.isLoggedIn().then((result) => {
    isLoggedIn.value = result;
  });
});

function buttonClick() {
  if (isLoggedIn.value) {
    router.push("/account");
  } else {
    Login();
  }
}

function Login() {
  var url = new URL(window.location.href);
  window.location.href = `login/Login?returnUrl=${url.origin}/LoginRedirect`;
}
</script>
