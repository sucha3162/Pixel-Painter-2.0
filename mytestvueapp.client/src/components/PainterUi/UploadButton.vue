<template>
  <Button
    :label="isEditing ? 'Save Changes' : 'Upload'"
    :icon="isEditing ? 'pi pi-save' : 'pi pi-upload'"
    @click="toggleModal()"
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
        @click="upload()"
        autofocus
      />
    </template>
  </Dialog>
</template>
<script setup lang="ts">
import Button from "primevue/button";
import Dialog from "primevue/dialog";
import InputText from "primevue/inputtext";
import { computed, ref, watch } from "vue";
import Art from "@/entities/Art";
import ToggleButton from "primevue/togglebutton";
import ArtAccessService from "@/services/ArtAccessService";
import { useToast } from "primevue/usetoast";
import router from "@/router";
import LoginService from "@/services/LoginService";
import { useLayerStore } from "@/store/LayerStore";
import { useArtistStore } from "@/store/ArtistStore";
const layerStore = useLayerStore();
const artistStore = useArtistStore();
const toast = useToast();
const visible = ref<boolean>(false);
const loading = ref<boolean>(false);

const newName = ref<string>("");
const newPrivacy = ref<boolean>(false);

const props = defineProps<{
  art: Art;
  fps: number;
}>();

const isEditing = computed(() => {
  return props.art.id != 0;
});

const emit = defineEmits(["openModal", "disconnect"]);

watch(visible, () => {
  emit("openModal", visible.value);
});

function toggleModal() {
  visible.value = !visible.value;
  newName.value = props.art.title;
  if (newName.value == "") {
    newName.value = "Untitled";
  }
  newPrivacy.value = props.art.isPublic;
}

function flattenArtEncode(): string {
  let width = layerStore.grids[0].width;
  let height = layerStore.grids[0].height;
  let arr: string[][] = Array.from({ length: height }, () =>
    Array(width).fill(layerStore.grids[0].backgroundColor.toLowerCase())
  );

  for (let length = 0; length < layerStore.grids.length; length++) {
    for (let i = 0; i < height; i++) {
      for (let j = 0; j < width; j++) {
        //only set empty cells to background color if its the first layer
        //layers above the first will just replace cells if they have a value
        if (layerStore.grids[length].grid[i][j] !== "empty") {
          arr[i][j] = layerStore.grids[length].grid[i][j];
        }
      }
    }
  }
  return arr.flat().join("");
}
function FlattenFrameEncode(index: number): string {
  let width = layerStore.grids[index].width;
  let height = layerStore.grids[index].height;
  let arr: string[][] = Array.from({ length: height }, () =>
    Array(width).fill(layerStore.grids[0].backgroundColor)
  );

  for (let i = 0; i < height; i++) {
    for (let j = 0; j < width; j++) {
      if (layerStore.grids[index].grid[i][j] !== "empty") {
        arr[i][j] = layerStore.grids[index].grid[i][j];
      }
    }
  }
  return arr.flat().join("");
}
function handleNotLoggedIn() {
  toast.add({
    severity: "error",
    summary: "Error",
    detail: "You must be logged in to upload art",
    life: 3000
  });
  loading.value = false;
  visible.value = false;
}

function finalizeUpload(success: boolean, artId?: number) {
  loading.value = false;
  visible.value = false;

  if (success && artId) {
    toast.add({
      severity: "success",
      summary: "Success",
      detail: "Art uploaded successfully",
      life: 3000
    });
    layerStore.empty();
    artistStore.empty();
    localStorage.clear();
    router.push("/art/" + artId);
  } else if (!success) {
    toast.add({
      severity: "error",
      summary: "Error",
      detail: "Failed to upload art",
      life: 3000
    });
  }
}
function upload() {
  emit("disconnect");
  loading.value = true;

  LoginService.isLoggedIn().then((isLoggedIn) => {
    if (!isLoggedIn) {
      handleNotLoggedIn();
      return;
    }

    if (props.art.isGif) {
      const paintings: Art[] = layerStore.grids.map((grid, i) => {
        const newArt = new Art();
        newArt.title = newName.value;
        newArt.isPublic = newPrivacy.value;
        newArt.pixelGrid.deepCopy(grid);
        newArt.id = props.art.id;
        newArt.gifFrameNum = i + 1;
        newArt.isGif = true;
        newArt.pixelGrid.encodedGrid = FlattenFrameEncode(i);
        newArt.artistId = props.art.artistId;
        newArt.artistName = props.art.artistName;
        newArt.gifFps = props.fps;
        return newArt;
      });

      ArtAccessService.saveGif(paintings)
        .then((data: Art) => finalizeUpload(!!data.id, data.id))
        .catch((error) => {
          console.error(error);
          finalizeUpload(false);
        });
    } else {
      const newArt = new Art();
      newArt.title = newName.value;
      newArt.isPublic = newPrivacy.value;
      newArt.pixelGrid.deepCopy(layerStore.grids[0]);
      newArt.id = props.art.id;
      newArt.pixelGrid.encodedGrid = flattenArtEncode();
      newArt.artistId = props.art.artistId;
      newArt.artistName = props.art.artistName;

      ArtAccessService.saveArt(newArt)
        .then((data: Art) => finalizeUpload(!!data.id, data.id))
        .catch((error) => {
          console.error(error);
          finalizeUpload(false);
        });
    }
  });
}
</script>
