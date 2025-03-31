<template>
    <FloatingCard position="bottom"
                  header="Frames"
                  button-icon="pi pi-images"
                  button-label=""
                  width=""
                  :default-open="true">

        <Button class="mr-1" icon="pi pi-minus" size="small" rounded @click="removeFrame()" />

        <template v-for="(frame, index) in frames" :key="frame.id">
            <Button :icon="frame.icon" 
                    :class="frame.class" 
                    :severity="frame.severity"
                    @click="switchFrame(frame.id)" 
                    />
        </template>

        <Button class="ml-1" icon="pi pi-plus" size="small" rounded @click="addFrame()" />
    </FloatingCard>
</template>

<script setup lang="ts">
import Button from "primevue/button";
import Tag from "primevue/tag";
import FloatingCard from "./FloatingCard.vue";
import { ref, onBeforeMount } from 'vue';

    let selectedFrame = defineModel<number>("selFrame", { default: 1 });
    let oldFrame = defineModel<number>("lastFrame", { default: 1 });
    let index = defineModel<number>("frameIndex", { default: 2 });

    let frameCount = 1;
    const frames = ref([
        { id: 1, icon: 'pi pi-image', class: 'm-1', severity: 'secondary' }
    ]);

    onBeforeMount(() => {
        var count = 2;
        while (localStorage.getItem(`frame${count}`) != null) {
            frames.value.push({
                id: count,
                icon: 'pi pi-image',
                class: 'mr-1',
                severity: 'secondary'
            });

            frameCount++;
            count++;
            index.value++;
        }
        frames.value[0].severity = 'primary';
    });

    function addFrame() {
        frameCount++;
        index.value++;
        frames.value.push({
            id: frameCount,
            icon: 'pi pi-image',
            class: 'mr-1',
            severity: 'secondary'
        });
    }

    function removeFrame() {
        if (frames.value.length > 1) {
            frames.value.pop();
            frameCount--;
            index.value--;

            if (selectedFrame.value == frameCount + 1) {
                localStorage.getItem(`frame${frameCount}`)
            }
        }

        if (localStorage.getItem(`frame${frameCount + 1}`) != null) {
            localStorage.removeItem(`frame${frameCount + 1}`);
        }
    }

    function switchFrame(frameID: number) {
        frames.value.forEach((nFrame) => {
            nFrame.severity = 'secondary';
        });
        frames.value[frameID - 1].severity = 'primary';

        oldFrame.value = selectedFrame.value;
        selectedFrame.value = frameID;
    }

</script>

<style>
    .p-dialog.p-component {
        margin-bottom: 75px !important;
    }
</style>