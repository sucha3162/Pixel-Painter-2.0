<template>
  <div class="justify-content-center flex w-full h-full align-items-center">
    <div class="border-2">
      <my-canvas
        v-if="art"
        :key="art.id"
        :art="art"
        :pixelSize="20"
        :canvas-number="1"
      ></my-canvas>
    </div>
    <Card class="w-20rem ml-5">
      <template #content>
        <h3 class="flex">
          {{ art.title }}
        </h3>

        <div>By {{ art.artistName }}</div>
        <div>Uploaded on {{ uploadDate.toLocaleDateString() }}</div>

        <div class="flex flex-column gap-2 mt-4">
          <div class="flex gap-2">
            <LikeButton
              class=""
              :art-id="id"
              :likes="art.numLikes"
            ></LikeButton>
            <SaveImageToFile :art="art"></SaveImageToFile>
          </div>
          <div class="flex gap-2">
            <Button
              v-if="art.currentUserIsOwner"
              label="Edit"
              icon="pi pi-pencil"
              severity="secondary"
              @click="router.push(`/paint/${id}`)"
            ></Button>
            <DeleteArtButton v-if="art.currentUserIsOwner || user" :art="art">
            </DeleteArtButton>
          </div>
        </div>
      </template>
    </Card>
  </div>

  <h2 class="px-4">{{ allComments.length }} Comments</h2>

  <div class="px-6">
    <!-- Initial comment. Reply to image -->
    <NewComment
      @newComment="updateComments"
      class="mb-4"
      :allComments="allComments"
    ></NewComment>
    <CommentOnArt
      v-for="Comment in allComments"
      :key="Comment.id"
      :comment="Comment"
      @delete-comment="updateComments"
    ></CommentOnArt>
  </div>
</template>
<script setup lang="ts">
import SaveImageToFile from "@/components/PainterUi/SaveImageToFile.vue";
import DeleteArtButton from "@/components/DeleteArtButton.vue";
import Art from "@/entities/Art";
import MyCanvas from "@/components/MyCanvas/MyCanvas.vue";
import { ref, onMounted } from "vue";
import Comment from "@/entities/Comment";
import CommentOnArt from "@/components/Comment/CommentOnArt.vue";
import ArtAccessService from "../services/ArtAccessService";
import { useRoute } from "vue-router";
import CommentAccessService from "../services/CommentAccessService";
import NewComment from "@/components/Comment/NewComment.vue";
import Card from "primevue/card";
import LikeButton from "@/components/LikeButton.vue";
import Button from "primevue/button";
import router from "@/router";
import { useToast } from "primevue/usetoast";
import LoginService from "../services/LoginService";

const route = useRoute();
const toast = useToast();

const art = ref<Art>(new Art());
const allComments = ref<Comment[]>([]);
const id = Number(route.params.id);
const uploadDate = ref(new Date());
const user = ref<boolean>(false);
var numberTotalComments = Number(0);

onMounted(() => {
  ArtAccessService.getArtById(id)
    .then((promise: Art) => {
      art.value = promise as Art;
      uploadDate.value = new Date(promise.creationDate);
    })
    .catch(() => {
      router.push("/gallery");
      toast.add({
        severity: "error",
        summary: "Error",
        detail: "Art not found",
        life: 3000,
      });
    });
  updateComments();
  getIsAdmin();
  //console.log(user.value);
});

function updateComments() {
  CommentAccessService.getCommentsById(id).then((promise: Comment[]) => {
    allComments.value = buildCommentTree(promise);
  });
}

function getIsAdmin() {
  LoginService.GetIsAdmin().then((promise: boolean) => {
    user.value = promise;
  });
}

function buildCommentTree(comments: Comment[]): Comment[] {
  const commentMap: { [id: number]: Comment } = {};
  const roots: Comment[] = [];

  // Create a map of comments by their ID
  for (const comment of comments) {
    commentMap[comment.id!] = { ...comment, replies: [] }; // Ensure `replies` is initialized
  }

  // Build the tree by associating replies with their parents
  for (const comment of comments) {
    const currentComment = commentMap[comment.id!];
    if (!comment.replyId || comment.replyId === 0) {
      // No parent, so it's a root-level comment
      roots.push(currentComment);
    } else {
      // Add as a reply to its parent
      const parentComment = commentMap[comment.replyId];
      if (parentComment) {
        parentComment.replies!.push(currentComment);
      } else {
        console.warn(
          `Parent with ID ${comment.replyId} not found for comment ID ${comment.id}`
        );
      }
    }
  }

  return roots;
}

// function hide-comments() {
//   CommentAccessService.getCommentsById(id).then((promise: Comment[]) =>{
//     if (allComments.value)
//   });
// }
</script>
