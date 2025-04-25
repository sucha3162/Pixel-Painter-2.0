<template>
  <div class="w-9 mx-auto my-0">
    <header>
      <h1 class="flex align-items-center gap-3">
        Search for Art
        <InputText
          class="mt-2"
          v-model.trim="search"
          type="text"
          placeholder="Search title..."
        />
        <InputText
          class="mt-2 w-2"
          v-model.trim="filter"
          type="text"
          placeholder="Search artists..."
        />
        <Dropdown
          class="pl mt-2 text-base w-1.5 font-normal"
          v-model="sortType"
          :options="sortBy"
          optionLabel="sort"
          optionValue="code"
          placeholder="Sort by"
        />
        <ToggleButton
          v-if="isSorted"
          id="toggle"
          class="mt-2 text-base w-0 font-normal"
          v-model="checkAscending"
          onLabel="Ascending"
          onIcon="pi pi-arrow-up"
          offLabel="Descending"
          offIcon="pi pi-arrow-down"
          @click="handleCheckBox()"
        />

        <ToggleButton
          v-if="isSortedByDate"
          id="toggle"
          class="mt-2 text-base w-0 font-normal"
          v-model="checkAscending"
          onLabel="Oldest First"
          onIcon="pi pi-arrow-up"
          offLabel="Newest First"
          offIcon="pi pi-arrow-down"
          @click="handleCheckBox()"
        />
      </h1>
      <div style="display: inline-flex">
        <p>Art per page: &nbsp;</p>
        <Dropdown
          class="pl my-2 text-base w-1.5 font-normal"
          v-model="perPage"
          :options="paginationOptions"
        />
      </div>
    </header>
    <div class="shrink-limit flex flex-wrap" v-if="!loading">
      <ArtCard
        v-for="index in displayAmount"
        :key="index"
        :art="displayArt[index + offset]"
        :size="6"
        :position="index"
      />
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
const search = ref("");
const filter = ref("");
const loading = ref(true);
const sortBy = ref([
  { sort: "Likes", code: "L" },
  { sort: "Comments", code: "C" },
  { sort: "Date", code: "D" }
]);
const paginationOptions = ref<Number[]>([12, 24, 36]);
const sortType = ref("D"); // Value binded to sort drop down
const isSorted = ref(false); // Renders the Descending checkbox while true
const isSortedByDate = ref(true);
const checkAscending = ref(false);
const isModified = ref(false);
const currentPage = ref<number>(1);
const perPage = ref<number>(12);
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
const flicker = ref<boolean>(true);
watch(perPage, () => {
  flicker.value = false;
  changePage(1);
  flicker.value = true;
});

onMounted(() => {
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

function searchAndFilter() {
  if (displayArt.value) {
    displayArt.value = displayArt.value.filter((Art) =>
      Art.artistName
        .toString()
        .toLowerCase()
        .includes(filter.value.toLowerCase())
    );

    displayArt.value = displayArt.value.filter((Art) =>
      Art.title.toLowerCase().includes(search.value.toLowerCase())
    );
  }
}

function handleCheckBox() {
  // Gets called when the ascending checkbox is clicked
  sortGallery();
}

function sortGallery() {
  var sortCode = sortType.value;
  isModified.value = true;

  if (sortCode == "D") {
    isSorted.value = false;
    isSortedByDate.value = true;
  } else {
    isSorted.value = true;
    isSortedByDate.value = false;
  }

  switch (sortCode) {
    case "L": {
      // Likes
      ArtAccessService.getArtByLikes(checkAscending.value)
        .then((data) => {
          publicArt.value = data;
          displayArt.value = data;
        })
        .finally(() => {
          loading.value = false;
          if (publicArt.value) {
            searchAndFilter();
          }
        });
      break;
    }
    case "C": {
      // Commments
      ArtAccessService.getArtByComments(checkAscending.value)
        .then((data) => {
          publicArt.value = data;
          displayArt.value = data;
        })
        .finally(() => {
          loading.value = false;
          if (publicArt.value) {
            searchAndFilter();
          }
        });
      break;
    }
    case "D": {
      // Date
      ArtAccessService.getArtByDate(checkAscending.value)
        .then((data) => {
          publicArt.value = data;
          displayArt.value = data;
        })
        .finally(() => {
          loading.value = false;
          if (publicArt.value) {
            searchAndFilter();
          }
        });
      break;
    }
  }
}
</script>
