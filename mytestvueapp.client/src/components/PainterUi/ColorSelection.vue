<template>
  <FloatingCard
    position="left"
    :header="props.isBackground ? 'Background Select' : 'Brush Settings'"
    width="13rem"
    :button-icon="props.isBackground ? 'pi pi-tablet' : 'pi pi-palette'"
    button-label=""
    :default-open="!props.isBackground"
  >
    <Tabs value="0">
      <TabList>
        <Tab value="0" @click="setCurrentPallet(0)">Default</Tab>
        <Tab
          value="1"
          @click="
            setCurrentPallet(1);
            setSaved();
          "
          v-tooltip.right="
            'Click to add custom color. Double click to remove color.'
          "
          v-if="!props.isBackground"
          >Custom</Tab
        >
      </TabList>
      <TabPanels>
        <TabPanel value="0">
          <div class="flex flex-wrap">
            <div
              v-for="color in DefaultColor.getDefaultColors()"
              :key="color.hex"
            >
              <div
                @click="selectedColor = color.hex"
                class="border-1 m-1 w-2rem h-2rem border-round-md"
                :style="{ backgroundColor: '#' + color.hex }"
                v-tooltip.bottom="props.isBackground ? '' : color.shortcut"
              ></div>
            </div>
            <!-- @input="updateColorFromHex" -->
            <div class="parent">
              <input
                class="pl-1"
                v-model="hexColor"
                placeholder="#000000"
                style="width: 54%"
                @focus="inputActive"
                @blur="inputInactive"
                @input="validateHex"
                @keydown.enter="handleEnter"
              />
            </div>
            <ColorPicker class="m-1" v-model="selectedColor" />
          </div>
          <div class="mt-1" v-if="!props.isBackground">Size: {{ size }}</div>

          <div class="px-2" v-if="!props.isBackground">
            <Slider
              class="mt-2"
              v-model="size"
              :min="1"
              :max="32"
              v-tooltip.bottom="'Decrease(q),Increase(w)'"
            />
          </div>
        </TabPanel>
        <TabPanel value="1" v-if="!props.isBackground">
          <div class="flex flex-wrap">
            <div
              id="1"
              @click="updateColors('1', 0)"
              @dblclick="deleteColor('1', 0)"
              class="border-1 m-1 w-2rem h-2rem border-round-md"
              v-tooltip.bottom="'Shortcut: 1'"
            ></div>

            <div
              id="2"
              @click="updateColors('2', 1)"
              @dblclick="deleteColor('2', 1)"
              class="border-1 m-1 w-2rem h-2rem border-round-md"
              v-tooltip.bottom="'Shortcut: 2'"
            ></div>
            <div
              id="3"
              @click="updateColors('3', 2)"
              @dblclick="deleteColor('3', 2)"
              class="border-1 m-1 w-2rem h-2rem border-round-md"
              v-tooltip.bottom="'Shortcut: 3'"
            ></div>
            <div
              id="4"
              @click="updateColors('4', 3)"
              @dblclick="deleteColor('4', 3)"
              class="border-1 m-1 w-2rem h-2rem border-round-md"
              v-tooltip.bottom="'Shortcut: 4'"
            ></div>
            <div
              id="5"
              @click="updateColors('5', 4)"
              @dblclick="deleteColor('5', 4)"
              class="border-1 m-1 w-2rem h-2rem border-round-md"
              v-tooltip.bottom="'Shortcut: 5'"
            ></div>
            <div
              id="6"
              @click="updateColors('6', 5)"
              @dblclick="deleteColor('6', 5)"
              class="border-1 m-1 w-2rem h-2rem border-round-md"
              v-tooltip.bottom="'Shortcut: 6'"
            ></div>
            <div
              id="7"
              @click="updateColors('7', 6)"
              @dblclick="deleteColor('7', 6)"
              class="border-1 m-1 w-2rem h-2rem border-round-md"
              v-tooltip.bottom="'Shortcut: 7'"
            ></div>
            <div
              id="8"
              @click="updateColors('8', 7)"
              @dblclick="deleteColor('8', 7)"
              class="border-1 m-1 w-2rem h-2rem border-round-md"
              v-tooltip.bottom="'Shortcut: 8'"
            ></div>
            <div
              id="9"
              @click="updateColors('9', 8)"
              @dblclick="deleteColor('9', 8)"
              class="border-1 m-1 w-2rem h-2rem border-round-md"
              v-tooltip.bottom="'Shortcut: 9'"
            ></div>
            <div
              id="0"
              @click="updateColors('0', 9)"
              @dblclick="deleteColor('0', 9)"
              class="border-1 m-1 w-2rem h-2rem border-round-md"
              v-tooltip.bottom="'Shortcut: 0'"
            ></div>
            <div
              id="-"
              @click="updateColors('-', 10)"
              @dblclick="deleteColor('-', 10)"
              class="border-1 m-1 w-2rem h-2rem border-round-md"
              v-tooltip.bottom="'Shortcut: -'"
            ></div>
            <div
              id="="
              @click="updateColors('=', 11)"
              @dblclick="deleteColor('=', 11)"
              class="border-1 m-1 w-2rem h-2rem border-round-md"
              v-tooltip.bottom="'Shortcut: ='"
            ></div>
            <div class="parent">
              <input
                class="pl-1"
                v-model="hexColor"
                placeholder="#000000"
                style="width: 54%"
                @focus="inputActive"
                @blur="inputInactive"
                @input="validateHex"
                @keydown.enter="handleEnter"
              />
            </div>
            <ColorPicker class="m-1" v-model="selectedColor"></ColorPicker>
          </div>
          <div class="mt-1">Size: {{ size }}</div>

          <div class="px-2">
            <Slider
              class="mt-2"
              v-model="size"
              :min="1"
              :max="32"
              v-tooltip.bottom="'Decrease(q),Increase(w)'"
            />
          </div>
        </TabPanel>
      </TabPanels>
    </Tabs>
  </FloatingCard>
</template>
<script setup lang="ts">
import { ref, defineEmits, watch } from "vue";
import FloatingCard from "./FloatingCard.vue";
import ColorPicker from "primevue/colorpicker";
import Slider from "primevue/slider";
import DefaultColor from "@/entities/DefaultColors";
import Tabs from "primevue/tabs";
import TabList from "primevue/tablist";
import Tab from "primevue/tab";
import TabPanels from "primevue/tabpanels";
import TabPanel from "primevue/tabpanel";

const props = defineProps<{
  isBackground: boolean;
}>();

const selectedColor = defineModel<string>("color", { default: "000000" });
const size = defineModel<number>("size", { default: 1 });
const hexColor = ref<string>("#000000");
const emit = defineEmits(["DisableKeyBinds", "EnableKeyBinds"]);

const customColors = ref<string[]>(new Array(12));
const arrayDefault = ref<string[]>(new Array(12));
for (let i = 0; i < arrayDefault.value.length; i++) {
  arrayDefault.value[i] = DefaultColor.getDefaultColors()[i].hex;
}
let temp = localStorage.getItem("customPallet");
if (temp) {
  customColors.value = JSON.parse(temp);
}
setCurrentPallet(0);

watch(selectedColor, () => {
  hexColor.value = "#" + selectedColor.value;
});

function setSaved(): void {
  let tempColor = document.getElementById("1");
  if (tempColor) tempColor.style.backgroundColor = "#" + customColors.value[0];
  tempColor = document.getElementById("2");
  if (tempColor) tempColor.style.backgroundColor = "#" + customColors.value[1];
  tempColor = document.getElementById("3");
  if (tempColor) tempColor.style.backgroundColor = "#" + customColors.value[2];
  tempColor = document.getElementById("4");
  if (tempColor) tempColor.style.backgroundColor = "#" + customColors.value[3];
  tempColor = document.getElementById("5");
  if (tempColor) tempColor.style.backgroundColor = "#" + customColors.value[4];
  tempColor = document.getElementById("6");
  if (tempColor) tempColor.style.backgroundColor = "#" + customColors.value[5];
  tempColor = document.getElementById("7");
  if (tempColor) tempColor.style.backgroundColor = "#" + customColors.value[6];
  tempColor = document.getElementById("8");
  if (tempColor) tempColor.style.backgroundColor = "#" + customColors.value[7];
  tempColor = document.getElementById("9");
  if (tempColor) tempColor.style.backgroundColor = "#" + customColors.value[8];
  tempColor = document.getElementById("0");
  if (tempColor) tempColor.style.backgroundColor = "#" + customColors.value[9];
  tempColor = document.getElementById("-");
  if (tempColor) tempColor.style.backgroundColor = "#" + customColors.value[10];
  tempColor = document.getElementById("=");
  if (tempColor) tempColor.style.backgroundColor = "#" + customColors.value[11];
}

function updateColors(id: string, index: number): void {
  let unique = true;
  for (let i = 0; i < customColors.value.length; i++) {
    if (customColors.value[i] === selectedColor.value) unique = false;
  }
  if (unique) {
    if (!customColors.value[index]) {
      customColors.value[index] = selectedColor.value;
      const currentCustom = document.getElementById(id);
      if (currentCustom) {
        currentCustom.style.backgroundColor = "#" + selectedColor.value;
      }
    }
  }
  if (customColors.value[index]) {
    selectedColor.value = customColors.value[index];
  }
  setCurrentPallet(1);

  localStorage.setItem("customPallet", JSON.stringify(customColors));
}

function deleteColor(id: string, index: number): void {
  customColors.value[index] = "";
  const currentCustom = document.getElementById(id);
  if (currentCustom) {
    currentCustom.style.backgroundColor = "transparent";
  }
  localStorage.setItem("customPallet", JSON.stringify(customColors));
}

function setCurrentPallet(tab: number): void {
  if (tab === 0) {
    localStorage.setItem("currentPallet", JSON.stringify(arrayDefault));
  } else localStorage.setItem("currentPallet", JSON.stringify(customColors));
}

function handleEnter(event: KeyboardEvent): void {
  (event.target as HTMLElement).blur();
}
function inputActive(): void {
  emit("DisableKeyBinds");
}
function inputInactive(): void {
  emit("EnableKeyBinds");
  if (hexColor.value.length == 7) {
    selectedColor.value = hexColor.value.substring(1, 7);
  }
}
function validateHex(): void {
  hexColor.value =
    "#" + hexColor.value.replace(/[^0-9a-fA-F]/g, "").toUpperCase();
  if (hexColor.value.length > 7) {
    hexColor.value = hexColor.value.substring(0, 7);
  }
}
</script>
<style scoped>
.p-colorpicker-preview {
  width: 7rem !important;
  height: 2rem !important;
  border: solid 1px !important;
}
.parent {
  display: flex;
  align-items: center;
  justify-content: center;
}
</style>
