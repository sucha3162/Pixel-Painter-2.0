<template>
  <Button
    rounded
    :severity="liked ? 'primary' : 'secondary'"
    :icon="liked ? 'pi pi-heart-fill' : 'pi pi-heart'"
    :label="(likes + localLike).toString()"
    @click.stop="likedClicked()"
  />
</template>
<script setup lang="ts">
import Button from "primevue/button";
import LoginService from "@/services/LoginService";
import LikeService from "@/services/LikeService";
import { useToast } from "primevue/usetoast";

import { ref, onMounted, watch } from "vue";

const props = defineProps<{
  likes: number;
  artId: number;
}>();

const localLike = ref(0);
const liked = ref(false);
const loggedIn = ref(false);

const toast = useToast();

onMounted(async () => {
  await LoginService.isLoggedIn().then((value) => (loggedIn.value = value));
  isLiked();
  localLike.value = 0;
});

async function isLiked() {
  if (loggedIn.value)
    LikeService.isLiked(props.artId).then((value) => (liked.value = value));
}

watch(
  () => props.artId,
  async () => {
    await isLiked();
  }
);

async function likedClicked() {
  if (!loggedIn.value) {
    // Route to login page
    toast.add({
      severity: "error",
      summary: "Warning",
      detail: "User must be logged in to like art!",
      life: 3000
    });
    return;
  }
  if (liked.value) {
    // Try to unlike
    LikeService.removeLike(props.artId).then((value) => {
      if (value) {
        liked.value = false;
      }
      if (localLike.value >= 0) {
        localLike.value--;
      }
    });
  } else {
    // Try to Like
    LikeService.insertLike(props.artId).then((value) => {
      if (value) {
        liked.value = true;
      }
      if (localLike.value <= 0) {
        localLike.value++;
      }
    });
  }
  // Calculate new number of likes
}
</script>
