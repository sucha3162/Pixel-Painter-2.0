<template>
  <FloatingCard
    position="left"
    header="Brush Settings"
    width="13rem"
    button-icon="pi pi-palette"
    button-label=""
    :default-open="true">
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
          >Custom</Tab
        >
      </TabList>
      <TabPanels>
        <TabPanel value="0">
          <div class="flex flex-wrap">
            <div
              v-for="color in DefaultColor.getDefaultColors()"
              :key="color.hex">
              <div
                @click="selectedColor = color.hex"
                class="border-1 m-1 w-2rem h-2rem border-round-md"
                :style="{ backgroundColor: '#' + color.hex }"
                v-tooltip.bottom="color.shortcut"></div>
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
                @input="ValidateHex"
                @keydown.enter="handleEnter" />
            </div>
            <ColorPicker class="m-1" v-model="selectedColor" />
          </div>
          <div class="mt-1">Size: {{ size }}</div>

          <div class="px-2">
            <Slider
              class="mt-2"
              v-model="size"
              :min="1"
              :max="32"
              v-tooltip.bottom="'Decrease(q),Increase(w)'" />
          </div>
        </TabPanel>
        <TabPanel value="1">
          <div class="flex flex-wrap">
            <div
              id="1"
              @click="updateColors('1', 0)"
              @dblclick="deleteColor('1', 0)"
              class="border-1 m-1 w-2rem h-2rem border-round-md"
              v-tooltip.bottom="'Shortcut: 1'"></div>

            <div
              id="2"
              @click="updateColors('2', 1)"
              @dblclick="deleteColor('2', 1)"
              class="border-1 m-1 w-2rem h-2rem border-round-md"
              v-tooltip.bottom="'Shortcut: 2'"></div>
            <div
              id="3"
              @click="updateColors('3', 2)"
              @dblclick="deleteColor('3', 2)"
              class="border-1 m-1 w-2rem h-2rem border-round-md"
              v-tooltip.bottom="'Shortcut: 3'"></div>
            <div
              id="4"
              @click="updateColors('4', 3)"
              @dblclick="deleteColor('4', 3)"
              class="border-1 m-1 w-2rem h-2rem border-round-md"
              v-tooltip.bottom="'Shortcut: 4'"></div>
            <div
              id="5"
              @click="updateColors('5', 4)"
              @dblclick="deleteColor('5', 4)"
              class="border-1 m-1 w-2rem h-2rem border-round-md"
              v-tooltip.bottom="'Shortcut: 5'"></div>
            <div
              id="6"
              @click="updateColors('6', 5)"
              @dblclick="deleteColor('6', 5)"
              class="border-1 m-1 w-2rem h-2rem border-round-md"
              v-tooltip.bottom="'Shortcut: 6'"></div>
            <div
              id="7"
              @click="updateColors('7', 6)"
              @dblclick="deleteColor('7', 6)"
              class="border-1 m-1 w-2rem h-2rem border-round-md"
              v-tooltip.bottom="'Shortcut: 7'"></div>
            <div
              id="8"
              @click="updateColors('8', 7)"
              @dblclick="deleteColor('8', 7)"
              class="border-1 m-1 w-2rem h-2rem border-round-md"
              v-tooltip.bottom="'Shortcut: 8'"></div>
            <div
              id="9"
              @click="updateColors('9', 8)"
              @dblclick="deleteColor('9', 8)"
              class="border-1 m-1 w-2rem h-2rem border-round-md"
              v-tooltip.bottom="'Shortcut: 9'"></div>
            <div
              id="0"
              @click="updateColors('0', 9)"
              @dblclick="deleteColor('0', 9)"
              class="border-1 m-1 w-2rem h-2rem border-round-md"
              v-tooltip.bottom="'Shortcut: 0'"></div>
            <div
              id="-"
              @click="updateColors('-', 10)"
              @dblclick="deleteColor('-', 10)"
              class="border-1 m-1 w-2rem h-2rem border-round-md"
              v-tooltip.bottom="'Shortcut: -'"></div>
            <div
              id="="
              @click="updateColors('=', 11)"
              @dblclick="deleteColor('=', 11)"
              class="border-1 m-1 w-2rem h-2rem border-round-md"
              v-tooltip.bottom="'Shortcut: ='"></div>
            <div class="parent">
              <input
                class="pl-1"
                v-model="hexColor"
                placeholder="#000000"
                style="width: 54%"
                @focus="inputActive"
                @blur="inputInactive"
                @input="ValidateHex"
                @keydown.enter="handleEnter" />
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
              v-tooltip.bottom="'Decrease(q),Increase(w)'" />
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

const selectedColor = defineModel<string>("color", { default: "000000" });
const size = defineModel<number>("size", { default: 1 });
const hexColor = ref<string>("#000000");
const emit = defineEmits(["DisableKeyBinds", "EnableKeyBinds"]);

let customColors: string[] = new Array(12);
let arrayDefault: string[] = new Array(12);
for (let i = 0; i < arrayDefault.length; i++) {
  arrayDefault[i] = DefaultColor.getDefaultColors()[i].hex;
}
let temp = localStorage.getItem("customPallet");
if (temp) {
  customColors = JSON.parse(temp);
}
setCurrentPallet(0);

watch(selectedColor, () => {
  hexColor.value = "#" + selectedColor.value;
});

function setSaved() {
  let tempColor = document.getElementById("1");
  if (tempColor) tempColor.style.backgroundColor = "#" + customColors[0];
  tempColor = document.getElementById("2");
  if (tempColor) tempColor.style.backgroundColor = "#" + customColors[1];
  tempColor = document.getElementById("3");
  if (tempColor) tempColor.style.backgroundColor = "#" + customColors[2];
  tempColor = document.getElementById("4");
  if (tempColor) tempColor.style.backgroundColor = "#" + customColors[3];
  tempColor = document.getElementById("5");
  if (tempColor) tempColor.style.backgroundColor = "#" + customColors[4];
  tempColor = document.getElementById("6");
  if (tempColor) tempColor.style.backgroundColor = "#" + customColors[5];
  tempColor = document.getElementById("7");
  if (tempColor) tempColor.style.backgroundColor = "#" + customColors[6];
  tempColor = document.getElementById("8");
  if (tempColor) tempColor.style.backgroundColor = "#" + customColors[7];
  tempColor = document.getElementById("9");
  if (tempColor) tempColor.style.backgroundColor = "#" + customColors[8];
  tempColor = document.getElementById("0");
  if (tempColor) tempColor.style.backgroundColor = "#" + customColors[9];
  tempColor = document.getElementById("-");
  if (tempColor) tempColor.style.backgroundColor = "#" + customColors[10];
  tempColor = document.getElementById("=");
  if (tempColor) tempColor.style.backgroundColor = "#" + customColors[11];
}

function updateColors(id: string, index: number) {
  let unique = true;
  for (let i = 0; i < customColors.length; i++) {
    if (customColors[i] === selectedColor.value) unique = false;
  }
  if (unique) {
    if (!customColors[index]) {
      customColors[index] = selectedColor.value;
      const currentCustom = document.getElementById(id);
      if (currentCustom) {
        currentCustom.style.backgroundColor = "#" + selectedColor.value;
      }
    }
  }
  if (customColors[index]) {
    selectedColor.value = customColors[index];
  }
  setCurrentPallet(1);

  localStorage.setItem("customPallet", JSON.stringify(customColors));
}

function deleteColor(id: string, index: number) {
  customColors[index] = "";
  const currentCustom = document.getElementById(id);
  if (currentCustom) {
    currentCustom.style.backgroundColor = "transparent";
  }
  localStorage.setItem("customPallet", JSON.stringify(customColors));
}

function setCurrentPallet(tab: number) {
  if (tab === 0) {
    localStorage.setItem("currentPallet", JSON.stringify(arrayDefault));
  } else localStorage.setItem("currentPallet", JSON.stringify(customColors));
}

function handleEnter(event: KeyboardEvent) {
  (event.target as HTMLElement).blur();
}
function inputActive() {
  emit("DisableKeyBinds");
}
function inputInactive() {
  emit("EnableKeyBinds");
  if (hexColor.value.length == 7) {
    selectedColor.value = hexColor.value.substring(1, 7);
  }
}
function ValidateHex() {
  hexColor.value =
    "#" + hexColor.value.replace(/[^0-9a-fA-F]/g, "").toUpperCase();
  if (hexColor.value.length > 7) {
    hexColor.value = hexColor.value.substring(0, 7);
  }
}
</script>

<style>
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
