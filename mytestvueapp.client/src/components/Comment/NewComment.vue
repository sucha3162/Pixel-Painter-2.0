<template>
  <div>
    <!-- {{ comment }} -->
    <InputText
      v-model:="comment.message"
      :placeholder="placeholder"
      class="w-full"
      @click="open = true"
      :disabled="!loggedIn"
      @keyup.enter="postComment()"></InputText>
    <div class="flex flex-row-reverse mt-2" v-if="open || parentComment">
      <Button class="ml-2" @click="postComment()">Post Comment</Button>
      <Button severity="secondary" @click="cancel()">Cancel</Button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from "vue";
import { useRoute } from "vue-router";
import CommentAccessService from "@/services/CommentAccessService";
import InputText from "primevue/inputtext";
import Button from "primevue/button";
import Comment from "@/entities/Comment";
import { useToast } from "primevue/usetoast";
import LoginService from "@/services/LoginService";

const toast = useToast();

const route = useRoute();
const open = ref<boolean>(false);
const comment = ref<Comment>(new Comment());
const loggedIn = ref<boolean>(false);

const props = defineProps<{
  parentComment?: Comment;
}>();

const emit = defineEmits(["newComment", "closeReply"]);

const placeholder = computed(() => {
  if (!loggedIn.value) {
    return "Login to comment";
  } else if (props.parentComment) {
    return "Reply to comment";
  } else {
    return "Add a comment...";
  }
});

async function postComment() {
  comment.value.artId = parseInt(route.params.id as string);
  comment.value.replyId = props.parentComment?.id;
  CommentAccessService.postComment(comment.value)
    .then(() => {
      emit("newComment");
      cancel();
    })
    .catch(() => {
      toast.add({
        severity: "error",
        summary: "Error",
        detail: "Failed to post comment",
        life: 3000
      });
    });
}

onMounted(async () => {
  LoginService.getCurrentUser().then((user) => {
    if (user != null) {
      loggedIn.value = true;
    }
  });
});

function cancel() {
  open.value = false;
  comment.value.message = "";
  emit("closeReply");
}
</script>

