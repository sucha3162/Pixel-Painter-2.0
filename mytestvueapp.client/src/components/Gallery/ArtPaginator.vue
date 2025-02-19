<template>
  <div class="pagination" v-if="pages > 1">
    <!--Only have pagination if there are more than one page-->
    <button
      v-if="pages != 2"
      :disable="page == 1 ? true : false"
      @click="page = 1">
      &lt;&lt;
    </button>
    <button :disable="page == 1 ? true : false" @click="page--">&lt;</button>
    <div v-if="pages < 16" class="buttonHolder">
      <!--Lists off all the pages-->
      <button
        @click="page = index"
        v-for="index in pages"
        :key="index"
        :class="index == page ? 'selected' : ''">
        {{ index }}
      </button>
    </div>
    <div v-else-if="startIndex == 1" class="buttonHolder">
      <!--Lists off all the pages-->
      <button
        @click="page = index"
        v-for="index in maxButtons + 1"
        :key="index"
        :class="index == page ? 'selected' : ''">
        {{ index }}
      </button>
      <button disabled="true">...</button>
    </div>
    <div v-else-if="startIndex >= pages - 13" class="buttonHolder">
      <!--Lists off all the pages-->
      <button disabled="true">...</button>
      <button
        @click="page = index + startIndex - 1"
        v-for="index in maxButtons + 1"
        :key="index"
        :class="index + startIndex - 1 == page ? 'selected' : ''">
        {{ index + startIndex - 1 }}
      </button>
    </div>
    <div v-else class="buttonHolder">
      <!--Lists off all the pages-->
      <button disabled="true">...</button>
      <button
        @click="page = index + startIndex - 1"
        v-for="index in maxButtons"
        :key="index"
        :class="index + startIndex - 1 == page ? 'selected' : ''">
        {{ index + startIndex - 1 }}
      </button>
      <button disabled="true">...</button>
    </div>
    <button :disable="page == pages ? true : false" @click="page++">
      &gt;
    </button>
    <button
      v-if="pages != 2"
      :disable="page == pages ? true : false"
      @click="page = props.pages">
      &gt;&gt;
    </button>
  </div>
</template>

<script setup lang="ts">
import { ref, defineProps, watch, defineEmits, computed } from "vue";
const props = defineProps<{
  pages: number;
}>();
const emit = defineEmits(["pageChange"]);
const page = ref<number>(1);
const maxButtons = ref<number>(13);
const startIndex = computed(() => {
  if (props.pages > 15 && page.value > 7) {
    if (page.value + 7 >= props.pages) {
      return props.pages - 13;
    }
    return page.value - 6;
  }
  return 1;
});
watch(props, () => {
  page.value = 1;
});
watch(page, () => {
  emit("pageChange", page.value);
});
</script>
<style scoped>
.pagination {
  width: 100%;
  display: flex;
  justify-content: center;
  align-items: center;
  padding: 1rem;
  gap: 0.5rem; /* Space between buttons */
}
.buttonHolder {
  display: flex;
  width: auto;
  gap: 0.5rem;
}
.selected {
  background-color: grey;
}
</style>
