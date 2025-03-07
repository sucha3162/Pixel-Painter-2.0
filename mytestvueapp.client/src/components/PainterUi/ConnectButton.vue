<template>
    <Button
        :label="connected ? 'Disconnect' : 'Connect'"
        :severity="connected ? 'danger': 'primary'"
        icon="pi pi-wifi"
        @click="ToggleModal()"
    />

    <Dialog v-model:visible="visible" modal :style="{width:'25rem'}">
        <template #header>
            <h1 class="mr-2">Connect?</h1>
        </template>

        <div class="flex align-items-center gap-3">
            <span>Group: </span>
            <InputText
            v-model="groupname"
            placeholder="group-name"
            class="w-full"
            ></InputText>
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
                label="Connect"
                severity="secondary"
                @click="connect()"
                autofocus
            />
        </template>
    </Dialog>
</template>

<script setup lang="ts">
    import { ref, watch } from "vue";
    import Button from "primevue/button";
    import Dialog from "primevue/dialog";
    import InputText from "primevue/inputtext";

    const emit = defineEmits(["OpenModal","Connect", "Disconnect"]);

    const props = defineProps<{
        connected: boolean;
    }>();

    const visible = ref(false);
    const groupname = ref("");

    function ToggleModal() {
        if (!props.connected) {
            visible.value = !visible.value;
        } else {
            disconnect();
        }
    }

    function connect() {
        emit("Connect", groupname.value);
        visible.value = !visible.value;
    }

    function disconnect() {
        emit("Disconnect", groupname.value);
        if (!props.connected) {
            ToggleModal();
        }
    }

    watch(visible, () => {
        emit("OpenModal", visible.value);
    });
</script>
