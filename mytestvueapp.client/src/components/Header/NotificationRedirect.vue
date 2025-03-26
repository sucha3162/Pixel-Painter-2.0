<template>
  <div id="notificationParent">
    <div id="redicon" v-if="notificationCount > 0">{{ notificationCount }}</div>
    <Button
      id="notificationButton"
      rounded
      @click="buttonClick()"
      icon="pi pi-bell"></Button>
  </div>
</template>

<script setup lang="ts">
import Button from "primevue/button";
import { ref, onMounted } from "vue";
import router from "@/router";
//services
import LoginService from "@/services/LoginService";
import NotificationService from "@/services/NotificationService";

const notificationCount = ref<number>(0);

onMounted(() => {
  LoginService.GetCurrentUser().then((data) => {
    NotificationService.getNotifications(data.id).then((data) => {
      notificationCount.value = data.length;
    });
  });
});

function buttonClick() {
  router.push("/notification");
}
</script>
<style scoped>
#redicon {
  position: absolute;
  top: -6px;
  right: -2px;
  color: white;
  font-size: 0.7rem;
  height: 20px;
  width: 20px;
  z-index: 2;
  background: red;
  padding: 10px;
  box-sizing: border-box;
  border-radius: 100%;
  display: flex;
  justify-content: center;
  align-items: center;
}
#notificationParent {
  position: relative;
}
</style>

