<template>
  <div>
    <h1 class="flex align-items-center gap-3">Notifications</h1>

    <div v-for="notification in notifications" v-bind:key="notification.art">
      {{ notification.user }} has
      <span v-if="notification.type == 0">liked</span
      ><span v-else>commented</span> on your
      <span v-if="notification.type == 3">comment</span
      ><span v-else>artwork, "{{ notification.art }}"</span>
    </div>
  </div>
</template>
<script setup lang="ts">
import { ref, onMounted } from "vue";

import Notification from "@/entities/Notification";
import NotificationService from "@/services/NotificationService";
import LoginService from "@/services/LoginService";

const notifications = ref<Notification[]>([]);

onMounted(() => {
  LoginService.GetCurrentUser().then((data) => {
    NotificationService.getNotifications(data.id).then((data) => {
      notifications.value = data;
    });
  });
});
</script>
