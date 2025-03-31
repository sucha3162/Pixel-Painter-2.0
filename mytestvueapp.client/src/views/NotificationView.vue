<template>
  <div>
    <h1 class="flex align-items-center gap-3 ml-4">Notifications</h1>

    <div style="overflow-y: auto; height: 60vh">
        <div v-if="notificationStore.notifications.length == 0" class="SadText">
            <p>You do not have any notifications at this time</p>
        </div>
        <div v-if="store.Theme === 'light'">
            <div v-for="(notification, index) in notifications"
                 v-bind:key="index"
                 :class="notification.viewed ? 'lCardV' : 'lCard'"
                 @click="MarkViewed(notification)">
                {{ notification.user }} has
                <span v-if="notification.type == 1">liked</span><span v-else>commented on</span> your
                <span v-if="notification.type == 3">comment</span><span v-else>artwork, "{{ notification.artName }}"</span>
            </div>
        </div>
        <div v-else>
            <div v-for="(notification, index) in notifications"
                 v-bind:key="index"
                 :class="notification.viewed ? 'dCardV' : 'dCard'"
                 @click="MarkViewed(notification)">
                {{ notification.user }} has
                <span v-if="notification.type === 1">liked</span>
                <span v-else-if="notification.type === 0">commented on</span>
                <span v-else-if="notification.type === 3">replied to</span>
                your
                <span v-if="notification.type == 3">comment</span><span v-else>artwork, "{{ notification.artName }}"</span>
            </div>
        </div>
    </div>
  </div>
</template>
<script setup lang="ts">
import { onMounted, computed } from "vue";

import Notification from "@/entities/Notification";
import NotificationService from "@/services/NotificationService";
import LoginService from "@/services/LoginService";

import { useThemeStore } from "@/store/ThemeStore";
import { useNotificationStore } from "@/store/NotificationStore";

const notifications = computed(() => {
  return notificationStore.notifications;
});
const store = useThemeStore();
const notificationStore = useNotificationStore();

onMounted(() => {
  if (notificationStore.notifications.length === 0) {
    LoginService.GetCurrentUser().then((data) => {
      NotificationService.getNotifications(data.id).then((data) => {
        notificationStore.notifications = data;
      });
    });
  }
});

async function MarkViewed(notification: Notification) {
  if (notification.commentId != -1) {
    notification.viewed = await MarkComment(notification.commentId);
  } else {
    notification.viewed = await MarkLike(
      notification.artId,
      notification.artistId
    );
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
.SadText {
  margin: 40px;
}
</style>

