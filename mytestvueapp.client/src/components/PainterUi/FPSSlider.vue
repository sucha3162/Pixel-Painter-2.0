<template>
    <FloatingCard position="bottom"
                  header="GIF Speed"
                  button-icon="pi pi-forward"
                  button-label=""
                  width=""
                  :default-open="false">

        <div>
            <Button class="align-items-center justify-content-center mr-2" icon="pi pi-minus" size="small" rounded @click="decreaseFPS()" />
            <label class="align-items-center justify-content-center bg-primary text-lg">{{fps}} FPS</label>
            <Button class="align-items-center justify-content-center ml-2" icon="pi pi-plus" size="small" rounded @click="increaseFPS()" />
        </div>

        <div class="w-56">
            <Slider v-model="fps" :step="1" :min="1" :max="10" class="mt-3" />
        </div>
    </FloatingCard>
</template>

<script setup lang="ts">
import Button from "primevue/button";
import Tag from "primevue/tag";
import FloatingCard from "./FloatingCard.vue";
import { ref, onBeforeMount } from 'vue';
import Slider from 'primevue/slider';
import InputText from 'primevue/inputtext';

    const fps = defineModel<number>("fps", { default: 4 });

    function increaseFPS() {
        if (fps.value < 10) {
            fps.value++;
        }
    }

    function decreaseFPS() {
        if (fps.value > 1) {
            fps.value--;
        }
    }
</script>