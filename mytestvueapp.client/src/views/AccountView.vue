<template>
  <div class="m-4">
    <div class="flex gap-4 justify-content-center">
      <Card class="h-fit">
        <template #content>
          <Avatar icon="pi pi-user" class="mr-2" size="xlarge" shape="circle" />
          <div class="text-3xl p-font-bold">{{ artist.name }}</div>
          <div class="">{{ artist.email }}</div>
          <div class="flex mt-4 p-2 gap-2 flex-column">
            <Button
              :severity="route.hash == '#settings' ? 'primary' : 'secondary'"
              @click="changeHash('#settings')"
              >Account Settings</Button
            >
            <Button
              :severity="route.hash == '#art' ? 'primary' : 'secondary'"
              @click="changeHash('#art')"
              >My Art</Button
            >
          </div>
        </template>
      </Card>

      <div v-if="route.hash == '#settings'">
        <h2>Account Settings</h2>
        <Card>
          <template #content>
            <div class="mb-4">
              <label for="username">Username</label>
              <div class="flex gap-1 flex-row align-items-center">
                <div class="flex flex-column gap-2">
                  <InputText
                    :disabled="!isEditing"
                    class="mr-1"
                    v-model="newUsername"
                    variant="filled"
                  />
                </div>
                <Button
                  v-if="!isEditing"
                  severity="secondary"
                  rounded
                  icon="pi pi-pencil"
                  @click="isEditing = true"
                />
                <span v-else class="">
                  <Button
                    severity="danger"
                    text
                    rounded
                    icon="pi pi-times"
                    @click="cancelEdit()"
                  />
                  <Button
                    severity="success"
                    text
                    rounded
                    icon="pi pi-check"
                    @click="updateUsername()"
                    :disabled="errorMessage != ''"
                  />
                </span>
              </div>
              <Message
                v-if="errorMessage != ''"
                severity="error"
                variant="simple"
                size="small"
                class="mt-2"
                >{{ errorMessage }}</Message
              >
            </div>
            <div class="align-items-stretch flex">
              <Button
                class="block m-2"
                label="logout"
                icon="pi pi-sign-out"
                @click="logout()"
              />
              <DeleteArtistButton :artist="artist" />
            </div>
          </template>
        </Card>
      </div>

      <div v-if="route.hash == '#art'">
        <h2>My Art</h2>
        <div class="flex flex-wrap">
          <ArtCard
            v-for="art in myArt"
            :key="art.id"
            :art="art"
            :size="10"
            :position="art.id"
          />
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from "vue";
import LoginService from "@/services/LoginService";
import ArtAccessService from "@/services/ArtAccessService";
import router from "@/router";
import { useToast } from "primevue/usetoast";
import Card from "primevue/card";
import Button from "primevue/button";
import InputText from "primevue/inputtext";
import Artist from "@/entities/Artist";
import Avatar from "primevue/avatar";
import Message from "primevue/message";
import { useRoute } from "vue-router";
import type Art from "@/entities/Art";
import ArtCard from "@/components/Gallery/ArtCard.vue";
import DeleteArtistButton from "@/components/DeleteArtistButton.vue";

const toast = useToast();
const route = useRoute();

var artist = ref<Artist>(new Artist());
var isEditing = ref<boolean>(false);
var newUsername = ref<string>("");

var myArt = ref<Art[]>([]);

onMounted(async () => {
  LoginService.getCurrentUser().then((user: Artist) => {
    if (user.id == 0) {
      router.push("/");
      toast.add({
        severity: "error",
        summary: "Warning",
        detail: "User must be logged in to view account page",
        life: 3000
      });
    }
    newUsername.value = user.name;
    artist.value = user;
  });

  ArtAccessService.getCurrentUsersArt().then((art) => {
    myArt.value = art;
  });

  if (route.hash != "#settings" && route.hash != "#art") {
    router.push("/account#settings");
  }
});

async function logout(): Promise<void> {
  LoginService.logout().then(() => {
    window.location.replace(`/`);
    toast.add({
      severity: "success",
      summary: "Success",
      detail: "User logged out",
      life: 3000
    });
  });
}

function cancelEdit(): void {
  isEditing.value = false;
  newUsername.value = artist.value.name;
}

const errorMessage = computed<string>(() => {
  if (newUsername.value.length > 16) {
    return "Username is too long. Max of 16 characters.";
  }

  if (newUsername.value.length < 4) {
    return "Username is too short. Min of 4 characters.";
  }

  return "";
});

async function updateUsername(): Promise<void> {
  LoginService.updateUsername(newUsername.value)
    .then((success) => {
      if (success) {
        toast.add({
          severity: "success",
          summary: "Success",
          detail: "Username successfully changed",
          life: 3000
        });
        artist.value.name = newUsername.value;
        isEditing.value = false;
      } else {
        toast.add({
          severity: "error",
          summary: "Error",
          detail: "Username is already taken. Try another",
          life: 3000
        });
      }
    })
    .catch(() => {
      toast.add({
        severity: "error",
        summary: "Error",
        detail: "An error occurred. Please try again later",
        life: 3000
      });
    });
}

function changeHash(hash: string): void {
  window.location.hash = hash;
}
</script>
