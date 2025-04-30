<template>
  <div class="w-9 mx-auto my-0">
    <header>
      <h1 class="flex align-items-center gap-3">
        Search for Art
        <InputText
          class="mt-2"
          v-model.trim="search"
          type="text"
          placeholder="Search title..." />
        <InputText
          class="mt-2 w-2"
          v-model.trim="filter"
          type="text"
          placeholder="Search artists..." />
        <Dropdown
          class="pl mt-2 text-base w-1.5 font-normal"
          v-model="sortType"
          :options="sortBy"
          optionLabel="sort"
          optionValue="code"
          placeholder="Sort by" />
        <ToggleButton
          v-if="isSortedByDate"
          id="toggle"
          class="mt-2 text-base w-0 font-normal"
          v-model="checkAscending"
          onLabel="Oldest First"
          onIcon="pi pi-arrow-up"
          offLabel="Newest First"
          offIcon="pi pi-arrow-down"
          @click="handleCheckBox()" />
        <ToggleButton
          v-else
          id="toggle"
          class="mt-2 text-base w-0 font-normal"
          v-model="checkAscending"
          onLabel="Ascending"
          onIcon="pi pi-arrow-up"
          offLabel="Descending"
          offIcon="pi pi-arrow-down"
          @click="handleCheckBox()" />
      </h1>
      <div style="display: inline-flex">
        <p>Art per page: &nbsp;</p>
        <Dropdown
          class="pl my-2 text-base w-1.5 font-normal"
          v-model="perPage"
          :options="paginationOptions" />
      </div>
    </header>
    <div class="shrink-limit flex flex-wrap" v-if="!loading">
      <ArtCard
        v-for="index in displayAmount"
        :key="index"
        :art="displayArt[index + offset]"
        :size="6"
        :position="index" />
    </div>
    <ArtPaginator :pages="pages" @page-change="changePage" />
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref, watch, computed } from "vue";
import ArtCard from "@/components/Gallery/ArtCard.vue";
import Art from "@/entities/Art";
import ArtAccessService from "@/services/ArtAccessService";
import InputText from "primevue/inputtext";
import Dropdown from "primevue/dropdown";
import ToggleButton from "primevue/togglebutton";
import ArtPaginator from "@/components/Gallery/ArtPaginator.vue";

const publicArt = ref<Art[]>([]);
const displayArt = ref<Art[]>([]);
const search = ref<string>("");
const filter = ref<string>("");
const loading = ref<boolean>(true);
const sortBy = ref([
  { sort: "Likes", code: "L" },
  { sort: "Comments", code: "C" },
  { sort: "Date", code: "D" }
]);
const paginationOptions = ref<Number[]>([12, 24, 36]);
const sortType = ref<string>("D"); // Value binded to sort drop down
const isSortedByDate = ref<boolean>(true);
const checkAscending = ref<boolean>(false);
const isModified = ref<boolean>(false);
const currentPage = ref<number>(1);
const perPage = ref<number>(12);
const flicker = ref<boolean>(true);
const pages = computed(() => {
  return Math.ceil(displayArt.value.length / perPage.value);
});
const displayAmount = computed(() => {
  if (currentPage.value == pages.value) {
    if (displayArt.value.length % perPage.value == 0) return perPage.value;
    return displayArt.value.length % perPage.value;
  }
  return perPage.value;
});
const offset = computed(() => {
  return perPage.value * (currentPage.value - 1) - 1;
});
watch(perPage, () => {
  flicker.value = false;
  changePage(1);
  flicker.value = true;
});

onMounted(async () => {
  ArtAccessService.getAllArt() // Get All Art
    .then((data) => {
      publicArt.value = data;
      displayArt.value = data;
    })
    .finally(() => {
      loading.value = false;
    });
});

watch(search, () => {
  if (publicArt.value) {
    isModified.value = true;
    displayArt.value = publicArt.value.filter((Art) =>
      Art.artistName
        .toString()
        .toLowerCase()
        .includes(filter.value.toLowerCase())
    );

    displayArt.value = displayArt.value.filter((Art) =>
      Art.title.toLowerCase().includes(search.value.toLowerCase())
    );
  }
});

watch(sortType, () => {
  sortGallery();
});

watch(filter, () => {
  if (publicArt.value) {
    isModified.value = true;
    displayArt.value = publicArt.value.filter((Art) =>
      Art.title.toLowerCase().includes(search.value.toLowerCase())
    );

    displayArt.value = displayArt.value.filter((Art) =>
      Art.artistName
        .toString()
        .toLowerCase()
        .includes(filter.value.toLowerCase())
    );
  }
});

function changePage(page: number) {
  currentPage.value = page;
}

function handleCheckBox() {
  // Gets called when the ascending checkbox is clicked
  sortGallery();
}

function sortGallery() {
  var sortCode = sortType.value;
  isModified.value = true;

  isSortedByDate.value = false;
  if (sortCode == "L") {
    // Sort By Likes
    if (checkAscending.value) {
      publicArt.value.sort((artA, artB) => artA.numLikes - artB.numLikes); // Sort to Descending
    } else {
      publicArt.value.sort((artA, artB) => artB.numLikes - artA.numLikes); // Sort to Ascending
    }
  } else if (sortCode == "C") {
    // Sort By Comments
    if (checkAscending.value) {
      publicArt.value.sort((artA, artB) => artA.numComments - artB.numComments); // Sort to Descending
    } else {
      publicArt.value.sort((artA, artB) => artB.numComments - artA.numComments); // Sort to Ascending
    }
  } else if (sortCode == "D") {
    if (checkAscending.value) {
      publicArt.value.sort(
        (artA, artB) =>
          new Date(artA.creationDate).getTime() -
          new Date(artB.creationDate).getTime()
      ); // Sort to Descending
    } else {
      publicArt.value.sort(
        (artA, artB) =>
          new Date(artB.creationDate).getTime() -
          new Date(artA.creationDate).getTime()
      ); // Sort to Ascending
    }
  }
}
</script>

