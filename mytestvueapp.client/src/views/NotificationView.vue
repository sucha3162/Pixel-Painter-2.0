<template>
  <div>
    <h1 class="flex align-items-center gap-3 ml-4">Notifications</h1>

    <div v-if="store.Theme === 'light'">
      <div
        v-for="(notification, index) in notifications"
        v-bind:key="index"
        :class="notification.viewed ? 'lCardV' : 'lCard'"
        @click="MarkViewed(notification)">
        {{ notification.user }} has
        <span v-if="notification.type == 1">liked</span
        ><span v-else>commented</span> on your
        <span v-if="notification.type == 3">comment</span
        ><span v-else>artwork, "{{ notification.artName }}"</span>
        <div icon="pi pi-info upside-down"></div>
      </div>
    </div>
    <div v-else>
      <div
        v-for="(notification, index) in notifications"
        v-bind:key="index"
        :class="notification.viewed ? 'dCardV' : 'dCard'"
        @click="notification.viewed = true">
        {{ notification.user }} has
        <div>
          <span v-if="notification.type === 1">liked</span>
          <span v-else-if="notification.type === 0">commented on</span>
          <span v-else-if="notification.type === 3">replied to</span>
        </div>
        your
        <div>
          <span v-if="notification.type == 3">comment</span
          ><span v-else>artwork, "{{ notification.artName }}"</span>
        </div>
        <i class="pi pi-exclamation-circle"></i>
      </div>
    </div>
  </div>
</template>
<script setup lang="ts">
import { ref, onMounted } from "vue";

import Notification from "@/entities/Notification";
import NotificationService from "@/services/NotificationService";
import LoginService from "@/services/LoginService";

import { useThemeStore } from "@/store/ThemeStore";

const notifications = ref<Notification[]>([]);
const store = useThemeStore();

onMounted(() => {
  LoginService.GetCurrentUser().then((data) => {
    NotificationService.getNotifications(data.id).then((data) => {
      notifications.value = data;
      console.log(notifications.value);
    });
  });
});

async function MarkViewed(notification: Notification) {
  if (notification.commentId === null) {
    notification.viewed = await MarkLike(
      notification.artId,
      notification.artistId
    );
  } else {
    notification.viewed = await MarkComment(notification.commentId);
  }
}

async function MarkComment(commentId: number): Promise<boolean> {
  return await NotificationService.markCommentViewed(commentId);
}
async function MarkLike(artId: number, artistId: number): Promise<boolean> {
  return await NotificationService.markLikeViewed(artId, artistId);
}
</script>
<style scoped>
.lCard {
  width: 80vw;
  border: 1px solid #ddd;
  border-radius: 10px;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
  overflow: hidden;
  transition: transform 0.3s ease;
  padding: 3px;
  padding-left: 5px;
  padding-right: 5px;
  margin-left: 20px;
  margin-right: 20px;
  margin-bottom: 5px;
}
.lCardV {
  width: 80vw;
  border: 1px solid #ddd;
  border-radius: 10px;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
  overflow: hidden;
  transition: transform 0.3s ease;
  background-color: #d3d3d3;
  color: #7f7f7f;
  padding: 3px;
  padding-left: 5px;
  padding-right: 5px;
  margin-left: 20px;
  margin-right: 20px;
  margin-bottom: 5px;
}

.lCard:hover {
  background-color: #d3d3d3;
}

.dCard {
  width: 80vw;
  border: 1px solid #ddd;
  border-radius: 10px;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
  overflow: hidden;
  transition: transform 0.3s ease;
  padding: 3px;
  padding-left: 5px;
  padding-right: 5px;
  margin-left: 20px;
  margin-right: 20px;
  margin-bottom: 5px;
}

.dCardV {
  width: 80vw;
  border: 1px solid #ddd;
  border-radius: 10px;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
  overflow: hidden;
  background-color: #7f7f7f;
  color: #d3d3d3;
  transition: transform 0.3s ease;
  padding: 3px;
  padding-left: 5px;
  padding-right: 5px;
  margin-left: 20px;
  margin-right: 20px;
  margin-bottom: 5px;
}

.dCard:hover {
  background-color: #7f7f7f;
}

.upside-down {
  transform: rotate(180deg); /* Flips the icon upside down */
  display: inline-block;
}
</style>

