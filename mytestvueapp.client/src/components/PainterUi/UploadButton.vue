<template>
  <Button
    :label="isEditing ? 'Save Changes' : 'Upload'"
    :icon="isEditing ? 'pi pi-save' : 'pi pi-upload'"
    @click="ToggleModal()"
  ></Button>

  <Dialog v-model:visible="visible" modal :style="{ width: '26rem' }">
    <template #header>
      <h1 class="mr-2">
        {{ isEditing ? "Save Your Changes?" : "Upload Your Art?" }}
      </h1>
    </template>
    <div class="flex flex-column gap-3 justify-content-center">
      <div class="flex align-items-center gap-3">
        <span>Title: </span>
        <InputText
          v-model="newName"
          placeholder="Title"
          class="w-full"
        ></InputText>
      </div>
      <div class="flex align-items-center gap-3">
        <span>Privacy:</span>
        <ToggleButton
          v-model="newPrivacy"
          onLabel="Public"
          onIcon="pi pi-globe"
          offLabel="Private"
          offIcon="pi pi-lock"
          class="w-36"
          aria-label="Do you confirm"
        />
        <span class="font-italic">*Visibility on gallery page*</span>
      </div>
    </div>
    <template #footer>
      <Button
        label="Cancel"
        text
        severity="secondary"
        @click="visible = false"
        autofocus
      />
      <Button
        :label="isEditing ? 'Save' : 'Upload'"
        severity="secondary"
        @click="Upload()"
        autofocus
      />
    </template>
  </Dialog>
</template>

<script setup lang="ts">
import Button from "primevue/button";
import Dialog from "primevue/dialog";
import InputText from "primevue/inputtext";
import { computed, onMounted, ref, watch } from "vue";
import Art from "@/entities/Art";
import ToggleButton from "primevue/togglebutton";
import ArtAccessService from "@/services/ArtAccessService";
import { useToast } from "primevue/usetoast";
import router from "@/router";
import LoginService from "@/services/LoginService";
import { HubConnection, HubConnectionState } from "@microsoft/signalr";
import Artist from "@/entities/Artist";

const toast = useToast();
const visible = ref(false);
const loading = ref(false);

const newName = ref("");
const newPrivacy = ref(false);

const contributors = ref<Artist[]>([]);

const props = defineProps<{
  art: Art;
  connection: signalR.HubConnection;
  connected: boolean;
  groupName: string;
}>();

const isEditing = computed(() => {
  return props.art.id != 0;
});

const emit = defineEmits(["OpenModal"]);

watch(visible, () => {
  emit("OpenModal", visible.value);
});

// WHY CANT I JUST WATCH props.connection.state !!!!!!!
// I even tried using computed and {deep: true}!!!!
watch(() => props.connected, () => {
  if (props.connection.state == HubConnectionState.Connected) {
    props.connection.invoke("GetContributingArtists", props.groupName);
  } 
});

onMounted(() => {
  if (props.connection.state == HubConnectionState.Connected){
    props.connection.invoke("GetContributingArtists", props.groupName);
  } 
});

props.connection.on("ContributingArtists", (allArtists: Artist[]) => {
  contributors.value = allArtists;
});

function ToggleModal() {
  visible.value = !visible.value;
  newName.value = props.art.title;
  if (newName.value == "") {
    newName.value = "Untitled";
  }
  newPrivacy.value = props.art.isPublic;
}

function flattenArt(): string {
  let encode = "";
  for (let i = 0; i < props.art.pixelGrid.height; i++) {
    for (let j = 0; j < props.art.pixelGrid.width; j++) {
      let hex = props.art.pixelGrid.grid[i][j];
      hex = hex[0] === "#" ? hex.slice(1) : hex;
      encode += hex === "empty" ? props.art.pixelGrid.backgroundColor : hex;
    }
  }
  return encode;
}

function Upload() {
  loading.value = true;

  LoginService.isLoggedIn().then((isLoggedIn) => {
    if (isLoggedIn) {
      const newArt = new Art();
      newArt.title = newName.value;
      newArt.isPublic = newPrivacy.value;
      newArt.pixelGrid.DeepCopy(props.art.pixelGrid);
      newArt.id = props.art.id;
      newArt.encodedGrid = flattenArt();
      if (props.connected){
        newArt.artistName = contributors.value.map((artist) => artist.name);
        newArt.artistId = contributors.value.map((artist) => artist.id);
      }

      ArtAccessService.SaveArt(newArt)
        .then((data: Art) => {
          if (data.id != undefined) {
            toast.add({
              severity: "success",
              summary: "Success",
              detail: "Art uploaded successfully",
              life: 3000,
            });
            localStorage.clear();
            router.push("/art/" + data.id);
          } else {
            toast.add({
              severity: "error",
              summary: "Error",
              detail: "Failed to upload art",
              life: 3000,
            });
          }
        })
        .catch((error) => {
          console.error(error);
          toast.add({
            severity: "error",
            summary: "Error",
            detail: "Failed to upload art",
            life: 3000,
          });
        })
        .finally(() => {
          loading.value = false;
          visible.value = false;
        });
    } else {
      toast.add({
        severity: "error",
        summary: "Error",
        detail: "You must be logged in to upload art",
      });
      loading.value = false;
      visible.value = false;
      return;
    }
  });
}
</script>
