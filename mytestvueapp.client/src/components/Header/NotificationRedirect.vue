<template>
  <div id="notificationParent">
    <div id="redicon" v-if="notificationsCount > 0">
      {{ notificationsCount }}
    </div>
    <Button
      id="notificationButton"
      rounded
      @click="buttonClick()"
      icon="pi pi-bell"></Button>
  </div>
</template>

<script setup lang="ts">
import Button from "primevue/button";
import { ref, onMounted, computed } from "vue";
import router from "@/router";
import Notification from "@/entities/Notification";
//services
import LoginService from "@/services/LoginService";
import NotificationService from "@/services/NotificationService";
//stores
import { useNotificationStore } from "@/store/NotificationStore";

const notificationStore = useNotificationStore();
const notificationsCount = computed(() => {
  return notificationStore.notifications.reduce(
    (count, data) => count + (data.viewed ? 0 : 1),
    0
  );
});

onMounted(() => {
  if (notificationStore.notifications.length === 0) {
    LoginService.GetCurrentUser().then((user) => {
      NotificationService.getNotifications(user.id).then((data) => {
        notificationStore.notifications = data;
      });
    });
  }
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

