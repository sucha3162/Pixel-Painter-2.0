<template>
  <div class="justify-content-center flex w-full h-full align-items-center">
    <div v-if="!art.isGif && art" class="border-2">
      <MyCanvas
        v-model="squareColor"
        v-if="!art.isGif && art"
        :key="art.id"
        :art="art"
        :pixelSize="20"
        :canvas-number="1"
      />
    </div>
    <div><img v-if="GifURL" :src="GifURL" alt="" /></div>
    <Card class="w-20rem ml-5">
      <template #content>
        <h3 class="flex">
          {{ art.title }}
        </h3>

        <div>
          By
          <div
            :style="{
              textDecoration: hover ? 'underline' : 'none',
              cursor: hover ? 'pointer' : 'none'
            }"
            v-for="(artist, index) in art.artistName"
            :key="index"
            class="py-1 font-semibold"
            @click="router.push(`/accountpage/${artist}`)"
            v-on:mouseover="hover = true"
            v-on:mouseleave="hover = false"
          >
            {{ artist }}
          </div>
        </div>
        <div>Uploaded on {{ uploadDate.toLocaleDateString() }}</div>

        <div class="flex flex-column gap-2 mt-4">
          <div class="flex gap-2">
            <LikeButton
              class=""
              :art-id="id"
              :likes="art.numLikes"
            ></LikeButton>
            <SaveImageToFile
              :art="art"
              :fps="0"
              :selectedLayer="-1"
            ></SaveImageToFile>
            <Button
              icon="pi pi-ellipsis-h"
              rounded
              text
              severity="secondary"
              @click="showFilters = !showFilters"
            />
          </div>
          <div class="flex gap-2">
            <Button
              v-if="art.currentUserIsOwner"
              label="Edit"
              icon="pi pi-pencil"
              severity="secondary"
              @click="editArt()"
            ></Button>
            <DeleteArtButton v-if="art.currentUserIsOwner || user" :art="art">
            </DeleteArtButton>
          </div>
          <div v-if="showFilters == true" class="">
            <h3>Filters</h3>
            <div>
              <Button
                @click="GreyScaleFilter"
                :disabled="filtered && greyscale == false"
                :severity="greyscale ? 'primary' : 'secondary'"
                >GreyScale</Button
              >
              <Button
                @click="ShowTones = !ShowTones"
                :severity="duotone ? 'primary' : 'secondary'"
                >DuoTone</Button
              >
              <Button
                @click="SepiaFilter"
                :disabled="filtered && sepia == false"
                :severity="sepia ? 'primary' : 'secondary'"
                >Sepia</Button
              >
              <Button
                @click="ProtanopeFilter"
                :disabled="filtered && prota == false"
                :severity="prota ? 'primary' : 'secondary'"
                >Protonope</Button
              >
              <Button
                @click="DeuFilter"
                :disabled="filtered && Deu == false"
                :severity="Deu ? 'primary' : 'secondary'"
                >Deuteranope</Button
              >
            </div>
            <div v-if="ShowTones" class="flex flex-column gap-2 mt-4">
              <h4 class="m-auto">Color 1</h4>
              <h4 class="m-auto">{{ toneOne }}</h4>
              <input
                type="color"
                id="tone1"
                v-model="toneOne"
                class="flex gap-2 w-auto h-2rem"
              />
              <h4 class="m-auto">Color 2</h4>
              <h4 class="m-auto">{{ toneTwo }}</h4>
              <input
                type="color"
                id="tone2"
                v-model="toneTwo"
                class="flex gap-2 w-auto h-2rem"
              />
              <Button
                :disabled="filtered && duotone == false"
                :severity="duotone ? 'primary' : 'secondary'"
                @click="DuoToneFilter(toneOne, toneTwo)"
                >Generate</Button
              >
            </div>
            <Button
              class="w-full flex gap-2"
              :disabled="filtered == false"
              severity="danger"
              @click="ResetFilters"
              >Reset</Button
            >
          </div>
        </div>
      </template>
    </Card>
  </div>

  <h2 class="px-4">{{ totalNumComments }} Comments</h2>

  <div class="px-6">
    <!-- Initial comment. Reply to image -->
    <NewComment
      @newComment="updateComments"
      class="mb-4"
      :allComments="allComments"
    ></NewComment>
    <CommentOnArt
      v-for="Comment in allComments"
      :key="Comment.id"
      :comment="Comment"
      @delete-comment="updateComments"
    ></CommentOnArt>
  </div>
</template>
<script setup lang="ts">
import SaveImageToFile from "@/components/PainterUi/SaveImageToFile.vue";
import DeleteArtButton from "@/components/DeleteArtButton.vue";
import Art from "@/entities/Art";
import MyCanvas from "@/components/MyCanvas/MyCanvas.vue";
import Comment from "@/entities/Comment";
import CommentOnArt from "@/components/Comment/CommentOnArt.vue";
import ArtAccessService from "../services/ArtAccessService";
import { useRoute } from "vue-router";
import CommentAccessService from "../services/CommentAccessService";
import NewComment from "@/components/Comment/NewComment.vue";
import Card from "primevue/card";
import LikeButton from "@/components/LikeButton.vue";
import Button from "primevue/button";
import router from "@/router";
import { useToast } from "primevue/usetoast";
import LoginService from "../services/LoginService";
import GIFCreationService from "@/services/GIFCreationService";
import { useLayerStore } from "@/store/LayerStore";
import { onMounted, ref } from "vue";

const layerStore = useLayerStore();

//filters
const greyscale = ref<boolean>(false);
const filtered = ref<boolean>(false);
const duotone = ref<boolean>(false);
const sepia = ref<boolean>(false);
const prota = ref<boolean>(false);
const Deu = ref<boolean>(false);
const hover = ref(false);

const route = useRoute();
const toast = useToast();
const art = ref<Art>(new Art());
const allComments = ref<Comment[]>([]);
const totalNumComments = ref<number>(0);
const id = Number(route.params.id);
const uploadDate = ref(new Date());
const user = ref<boolean>(false);
const showFilters = ref(false);
const ShowTones = ref(false);
const Names = ref<String[]>([]);
const GifURL = ref<string>("");
const urls = ref<string[]>([]);

onMounted(() => {
  ArtAccessService.getArtById(id)
    .then((promise: Art) => {
      art.value = promise as Art;
      uploadDate.value = new Date(promise.creationDate);
      Names.value = art.value.artistName;
      if (promise.isGif) {
        GifDisplay();
      }
    })
    .catch(() => {
      router.push("/gallery");
      toast.add({
        severity: "error",
        summary: "Error",
        detail: "Art not found",
        life: 3000
      });
    });
  updateComments();
  getIsAdmin();
});

function editArt() {
  layerStore.empty();
  layerStore.clearStorage();
  layerStore.pushGrid(art.value.pixelGrid);
  router.push(`/paint/${id}`);
}

function updateComments() {
  CommentAccessService.getCommentsById(id).then((promise: Comment[]) => {
    allComments.value = buildCommentTree(promise);
  });
}

function getIsAdmin() {
  LoginService.GetIsAdmin().then((promise: boolean) => {
    user.value = promise;
  });
}

function buildCommentTree(comments: Comment[]): Comment[] {
  totalNumComments.value = 0;
  const commentMap: { [id: number]: Comment } = {};
  const roots: Comment[] = [];

  // Create a map of comments by their ID
  for (const comment of comments) {
    commentMap[comment.id!] = { ...comment, replies: [] }; // Ensure `replies` is initialized
    totalNumComments.value++;
  }

  // Build the tree by associating replies with their parents
  for (const comment of comments) {
    const currentComment = commentMap[comment.id!];
    if (!comment.replyId || comment.replyId === 0) {
      // No parent, so it's a root-level comment
      roots.push(currentComment);
    } else {
      // Add as a reply to its parent
      const parentComment = commentMap[comment.replyId];
      if (parentComment) {
        parentComment.replies!.push(currentComment);
      } else {
        console.warn(
          `Parent with ID ${comment.replyId} not found for comment ID ${comment.id}`
        );
      }
    }
  }

  return roots;
}
const squareColor = ref<string>("blue");
const toneOne = ref<string>("#ff0000");
const toneTwo = ref<string>("#0000ff");

const GreyScaleFilter = () => {
  ArtAccessService.getArtById(id).then((promise: Art) => {
    if (promise.pixelGrid.encodedGrid) {
      if (greyscale.value == false) {
        squareColor.value = FilterGreyScale(promise.pixelGrid.encodedGrid);
        greyscale.value = true;
        filtered.value = true;
        return;
      } else {
        squareColor.value = promise.pixelGrid.encodedGrid;
        greyscale.value = false;
        filtered.value = false;
        return;
      }
    }
  });
};

function HEXtoRGB(hex: string): number[] {
  let rgb: number[] = [];
  let r = parseInt(hex.slice(0, 2), 16),
    g = parseInt(hex.slice(2, 4), 16),
    b = parseInt(hex.slice(4, 6), 16);

  rgb[0] = r;
  rgb[1] = g;
  rgb[2] = b;

  return rgb;
}
const rgbToHex = (r: number, g: number, b: number) =>
  [Math.round(r), Math.round(g), Math.round(b)]
    .map((x) => {
      const hex = x.toString(16);
      return hex.length === 1 ? "0" + hex : hex;
    })
    .join("");
function rgbToGrayscale(red: number, green: number, blue: number) {
  let r = red * 0.3; // ------> Red is low
  let g = green * 0.59; // ---> Green is high
  let b = blue * 0.11; // ----> Blue is very low
  r = Math.round(r);
  g = Math.round(g);
  b = Math.round(b);

  var gray = r + g + b;

  return [gray, gray, gray];
}

function FilterGreyScale(currentGrid: string): string {
  let newGrid: string = "";
  let currentcolorrgb: number[] = [];
  let newrgb: number[] = [];
  let newhexcolor: string = "";
  for (var i = 0; i <= currentGrid.length - 6; i += 6) {
    var currentcolor = currentGrid.substring(i, i + 6);
    currentcolorrgb = HEXtoRGB(currentcolor);
    newrgb = rgbToGrayscale(
      currentcolorrgb[0],
      currentcolorrgb[1],
      currentcolorrgb[2]
    );
    newhexcolor = rgbToHex(newrgb[0], newrgb[1], newrgb[2]);
    newGrid += newhexcolor;
  }
  return newGrid;
}
function GenerateGradient(toneOne: string, toneTwo: string): number[] {
  let rgb1: number[] = HEXtoRGB(toneOne.substring(1, 7));
  let rgb2: number[] = HEXtoRGB(toneTwo.substring(1, 7));
  let gradient: number[] = [];
  for (var i = 0; i < 256 * 3; i += 3) {
    gradient[i] = Math.round(
      ((256 - i / 4) * rgb1[0] + (i / 4) * rgb2[0]) / 256
    );
    gradient[i + 1] = Math.round(
      ((256 - i / 4) * rgb1[1] + (i / 4) * rgb2[1]) / 256
    );
    gradient[i + 2] = Math.round(
      ((256 - i / 4) * rgb1[2] + (i / 4) * rgb2[2]) / 256
    );
  }
  return gradient;
}
function DuoTone(
  currentGrid: string,
  toneOne: string,
  toneTwo: string
): string {
  let j = 0;
  let newGrid: string = "";
  let gradient: number[] = GenerateGradient(toneOne, toneTwo);
  let gradientGrid: number[][] = [];
  currentGrid = FilterGreyScale(currentGrid);
  for (let i = 0; i <= currentGrid.length - 6; i += 6) {
    gradientGrid[j] = HEXtoRGB(currentGrid.substring(i, i + 6));
    j++;
  }
  for (var k = 0; k < gradientGrid.length; k++) {
    let r = gradientGrid[k][0];
    let g = gradientGrid[k][1];
    let b = gradientGrid[k][2];
    let brightness = Math.round(r * 0.2126 + g * 0.7152 + b * 0.0722);
    gradientGrid[k][0] = gradient[brightness * 3];
    gradientGrid[k][1] = gradient[brightness * 3 + 1];
    gradientGrid[k][2] = gradient[brightness * 3 + 2];
    newGrid += rgbToHex(
      gradientGrid[k][0],
      gradientGrid[k][1],
      gradientGrid[k][2]
    );
  }
  return newGrid;
}
const DuoToneFilter = (toneOne: string, toneTwo: string) => {
  ArtAccessService.getArtById(id).then((promise: Art) => {
    if (promise.pixelGrid.encodedGrid) {
      if (duotone.value == false) {
        squareColor.value = DuoTone(
          promise.pixelGrid.encodedGrid,
          toneOne,
          toneTwo
        );
        duotone.value = true;
        filtered.value = true;
        return;
      } else {
        squareColor.value = promise.pixelGrid.encodedGrid;
        duotone.value = false;
        filtered.value = false;
        return;
      }
    }
  });
};
function ResetFilters() {
  filtered.value = false;
  greyscale.value = false;
  duotone.value = false;
  prota.value = false;
  Deu.value = false;
  sepia.value = false;
  ArtAccessService.getArtById(id).then((promise: Art) => {
    if (promise.isGif) {
      ArtAccessService.GetGif(promise.id).then((promiseGif: Art[]) => {
        urls.value = ArtToGif(promiseGif);
        GIFCreationService.createGIFcode(urls.value, promiseGif[0].gifFps).then(
          (Blob) => {
            //console.log(Blob);
            GifURL.value = Blob;
          }
        );
      });
    }

    if (promise.pixelGrid.encodedGrid)
      squareColor.value = promise.pixelGrid.encodedGrid;
  });
}
function SepiaTone(R: number, G: number, B: number): number[] {
  let newColors: number[] = [];
  let newRed = Math.round(0.393 * R + 0.769 * G + 0.189 * B);
  if (newRed > 255) newRed = 255;
  let newGreen = Math.round(0.349 * R + 0.686 * G + 0.168 * B);
  if (newGreen > 255) newGreen = 255;
  let newBlue = Math.round(0.272 * R + 0.534 * G + 0.131 * B);
  if (newBlue > 255) newBlue = 255;
  newColors[0] = newRed;
  newColors[1] = newGreen;
  newColors[2] = newBlue;

  return newColors;
}
function FilterSepia(currentGrid: string): string {
  let newGrid: string = "";
  let currentcolorrgb: number[] = [];
  let newrgb: number[] = [];
  let newhexcolor: string = "";
  for (var i = 0; i <= currentGrid.length - 6; i += 6) {
    var currentcolor = currentGrid.substring(i, i + 6);
    currentcolorrgb = HEXtoRGB(currentcolor);
    newrgb = rgbToGrayscale(
      currentcolorrgb[0],
      currentcolorrgb[1],
      currentcolorrgb[2]
    );
    newrgb = SepiaTone(newrgb[0], newrgb[1], newrgb[2]);
    newhexcolor = rgbToHex(newrgb[0], newrgb[1], newrgb[2]);
    newGrid += newhexcolor;
  }
  return newGrid;
}
const SepiaFilter = () => {
  ArtAccessService.getArtById(id).then((promise: Art) => {
    if (promise.pixelGrid.encodedGrid) {
      if (sepia.value == false) {
        squareColor.value = FilterSepia(promise.pixelGrid.encodedGrid);
        sepia.value = true;
        filtered.value = true;
        return;
      } else {
        squareColor.value = promise.pixelGrid.encodedGrid;
        sepia.value = false;
        filtered.value = false;
        return;
      }
    }
  });
};
function GammaCorrection(OldColor: number): number {
  let NewColor = (OldColor / 255) ** 2.2;
  return NewColor;
}
function InverseGammaCorrection(OldColor: number): number {
  // console.log(OldColor);
  if (OldColor < 0) {
    Math.abs(OldColor);
  }
  let expo = 1 / 2.2;
  let NewColor = Math.pow(OldColor, expo);
  NewColor = NewColor * 255;
  if (NewColor > 255) {
    NewColor = 255;
  }
  return NewColor;
}
function RGBtoLMS(rgbcolors: number[]): number[][] {
  let newrgbcolors: number[][] = [[], [], []];
  newrgbcolors[0][0] = rgbcolors[0];
  newrgbcolors[1][0] = rgbcolors[1];
  newrgbcolors[2][0] = rgbcolors[2];

  let LMSColors: number[][] = [];
  const LMSCalc: number[][] = [
    [17.8824, 43.5161, 4.11935],
    [3.45565, 27.1554, 3.86714],
    [0.0299566, 0.184309, 1.46709]
  ];
  var LMScolumns = LMSCalc[0].length;
  var LMSRows = LMSCalc.length;
  var RGBcolumns = newrgbcolors[0].length;
  for (let i = 0; i < LMSRows; i++) {
    LMSColors[i] = [];
    for (let j = 0; j < RGBcolumns; j++) {
      let sum = 0;
      for (let k = 0; k < LMScolumns; k++) {
        sum += LMSCalc[i][k] * newrgbcolors[k][j];
      }
      LMSColors[i][j] = sum;
    }
  }
  return LMSColors;
}
function LMStoProtanopes(LMScolors: number[][]): number[][] {
  let ProtanopeColors: number[][] = [];
  const ProtanopeCalc: number[][] = [
    [0, 2.02344, -2.52581],
    [0, 1, 0],
    [0, 0, 1]
  ];
  let PTPcolumns = ProtanopeCalc[0].length;
  let PTPRows = ProtanopeCalc.length;
  let LMScolumns = LMScolors[0].length;
  for (let i = 0; i < PTPRows; i++) {
    ProtanopeColors[i] = [];
    for (let j = 0; j < LMScolumns; j++) {
      let sum = 0;
      for (let k = 0; k < PTPcolumns; k++) {
        sum += ProtanopeCalc[i][k] * LMScolors[k][j];
      }
      ProtanopeColors[i][j] = sum;
    }
  }

  return ProtanopeColors;
}
function LMStoDeuteranopes(LMScolors: number[][]): number[][] {
  let DeuteranopesColors: number[][] = [];
  const DeuteranopesCalc: number[][] = [
    [1, 0, 0],
    [0.494207, 0, 1.24827],
    [0, 0, 1]
  ];
  let DEUcolumns = DeuteranopesCalc[0].length;
  let DEURows = DeuteranopesCalc.length;
  let LMScolumns = LMScolors[0].length;
  for (let i = 0; i < DEURows; i++) {
    DeuteranopesColors[i] = [];
    for (let j = 0; j < LMScolumns; j++) {
      let sum = 0;
      for (let k = 0; k < DEUcolumns; k++) {
        sum += DeuteranopesCalc[i][k] * LMScolors[k][j];
      }
      DeuteranopesColors[i][j] = sum;
      //console.log(i, j, sum);
    }
  }

  return DeuteranopesColors;
}
function LMStoRGB(LMScolors: number[][]): number[] {
  let RGBcolors: number[][] = [];
  let reformatedcolors: number[] = [];
  const RGBCal: number[][] = [
    [0.080944, -0.130504, 0.116721],
    [-0.0102485, 0.0540194, -0.113615],
    [-0.000365294, -0.00412163, 0.693513]
  ];
  let LMScolumns = LMScolors[0].length;
  let RGBRows = RGBCal.length;
  let RGBcolumns = RGBCal.length;

  for (let i = 0; i < RGBRows; i++) {
    RGBcolors[i] = [];
    for (let j = 0; j < LMScolumns; j++) {
      let sum = 0;
      for (let k = 0; k < RGBcolumns; k++) {
        sum += RGBCal[i][k] * LMScolors[k][j];
      }
      if (sum < 0) sum = 0;
      RGBcolors[i][j] = sum;
    }
  }
  reformatedcolors[0] = RGBcolors[0][0];
  reformatedcolors[1] = RGBcolors[1][0];
  reformatedcolors[2] = RGBcolors[2][0];
  return reformatedcolors;
}

/*
 Calculations made from:
Digital Video Colourmaps for
Checking the Legibility of
Displays by Dichromats
Francoise Vie�not,Hans Brettel,John D. Mollon

https://vision.psychol.cam.ac.uk/jdmollon/papers/colourmaps.pdf
*/

function FilterProtanope(currentGrid: string): string {
  let newGrid: string = "";
  let currentcolorrgb: number[] = [];
  let currentcolorlms: number[][] = [];
  let newcolorlms: number[][] = [];
  let newrgb: number[] = [];
  let newhexcolor: string = "";
  for (var i = 0; i <= currentGrid.length - 6; i += 6) {
    var currentcolor = currentGrid.substring(i, i + 6);
    currentcolorrgb = HEXtoRGB(currentcolor);
    currentcolorrgb = [
      GammaCorrection(currentcolorrgb[0]),
      GammaCorrection(currentcolorrgb[1]),
      GammaCorrection(currentcolorrgb[2])
    ];

    //after gamma adjustment
    currentcolorrgb[0] = 0.992052 * currentcolorrgb[0] + 0.003974;
    currentcolorrgb[1] = 0.992052 * currentcolorrgb[1] + 0.003974;
    currentcolorrgb[2] = 0.992052 * currentcolorrgb[2] + 0.003974;

    currentcolorlms = RGBtoLMS(currentcolorrgb);
    newcolorlms = LMStoProtanopes(currentcolorlms);
    newrgb = LMStoRGB(newcolorlms);
    newrgb = [
      InverseGammaCorrection(newrgb[0]),
      InverseGammaCorrection(newrgb[1]),
      InverseGammaCorrection(newrgb[2])
    ];
    newhexcolor = rgbToHex(newrgb[0], newrgb[1], newrgb[2]);
    newGrid += newhexcolor;
  }
  return newGrid;
}
const ProtanopeFilter = () => {
  ArtAccessService.getArtById(id).then((promise: Art) => {
    if (promise.pixelGrid.encodedGrid) {
      if (prota.value == false) {
        squareColor.value = FilterProtanope(promise.pixelGrid.encodedGrid);
        prota.value = true;
        filtered.value = true;
        return;
      } else {
        squareColor.value = promise.pixelGrid.encodedGrid;
        prota.value = false;
        filtered.value = false;
        return;
      }
    }
  });
};
function FilterDeu(currentGrid: string): string {
  let newGrid: string = "";
  let currentcolorrgb: number[] = [];
  let currentcolorlms: number[][] = [];
  let newcolorlms: number[][] = [];
  let newrgb: number[] = [];
  let newhexcolor: string = "";
  for (var i = 0; i <= currentGrid.length - 6; i += 6) {
    var currentcolor = currentGrid.substring(i, i + 6);
    currentcolorrgb = HEXtoRGB(currentcolor);
    currentcolorrgb = [
      GammaCorrection(currentcolorrgb[0]),
      GammaCorrection(currentcolorrgb[1]),
      GammaCorrection(currentcolorrgb[2])
    ];
    //after gamma adjustment Deu
    currentcolorrgb[0] = 0.957237 * currentcolorrgb[0] + 0.0213814;
    currentcolorrgb[1] = 0.957237 * currentcolorrgb[1] + 0.0213814;
    currentcolorrgb[2] = 0.957237 * currentcolorrgb[2] + 0.0213814;

    currentcolorlms = RGBtoLMS(currentcolorrgb);
    newcolorlms = LMStoDeuteranopes(currentcolorlms);
    newrgb = LMStoRGB(newcolorlms);
    newrgb = [
      InverseGammaCorrection(newrgb[0]),
      InverseGammaCorrection(newrgb[1]),
      InverseGammaCorrection(newrgb[2])
    ];
    newhexcolor = rgbToHex(newrgb[0], newrgb[1], newrgb[2]);
    if (newhexcolor.length != 6) {
      console.error(`Found it! (${i}): ${newhexcolor}`);
    }
    newGrid += newhexcolor;
  }
  return newGrid;
}
async function DeuFilter() {
  ArtAccessService.getArtById(id).then((promise: Art) => {
    if (promise.pixelGrid.encodedGrid) {
      if (Deu.value == false) {
        squareColor.value = FilterDeu(promise.pixelGrid.encodedGrid);
        Deu.value = true;
        filtered.value = true;
        return;
      }
    }
    if (promise.pixelGrid.encodedGrid)
      if (Deu.value == true) {
        squareColor.value = promise.pixelGrid.encodedGrid;
        Deu.value = false;
        filtered.value = false;
        return;
      }
  });
}
function ArtToGif(Paintings: Art[]): string[] {
  let url: string[] = [];
  Paintings.forEach((element) => {
    const canvas = document.createElement("canvas");
    const context = canvas.getContext("2d");
    if (!context) {
      throw new Error("Could not get context");
    }
    const image = context.createImageData(
      element.pixelGrid.width,
      element.pixelGrid.height
    );
    var hexBegin = 0;
    var hexEnd = 6;
    canvas.width = element.pixelGrid.width;
    canvas.height = element.pixelGrid.height;

    for (let x = 0; x < element.pixelGrid.height; x++) {
      for (let y = 0; y < element.pixelGrid.width; y++) {
        let pixelHex;

        pixelHex = element.pixelGrid.encodedGrid?.substring(hexBegin, hexEnd);
        if (pixelHex) pixelHex = pixelHex.replace("#", "").toUpperCase();
        const index = (x + y * element.pixelGrid.width) * 4;
        if (pixelHex)
          image?.data.set(
            [
              parseInt(pixelHex.substring(0, 2), 16),
              parseInt(pixelHex.substring(2, 4), 16),
              parseInt(pixelHex.substring(4, 6), 16),
              255
            ],
            index
          );
        hexBegin += 6;
        hexEnd += 6;
      }
    }
    context?.putImageData(image, 0, 0);

    let upsizedCanvas = document.createElement("canvas");
    upsizedCanvas.width = 360;
    upsizedCanvas.height = 360;
    let upsizedContext = upsizedCanvas.getContext("2d");
    if (!upsizedContext) {
      throw new Error("Could not get context");
    }
    upsizedContext.imageSmoothingEnabled = false;
    upsizedContext.drawImage(
      canvas,
      0,
      0,
      upsizedCanvas.width,
      upsizedCanvas.height
    );
    let dataURL = upsizedCanvas.toDataURL("image/png");
    const strings = dataURL.split(",");
    url.push(strings[1]);
  });
  return url;
}

const GifDisplay = () => {
  ArtAccessService.getArtById(id).then((promise: Art) => {
    ArtAccessService.GetGif(promise.gifID).then((promiseGif: Art[]) => {
      urls.value = ArtToGif(promiseGif);
      GIFCreationService.createGIFcode(urls.value, promiseGif[0].gifFps).then(
        (Blob) => {
          GifURL.value = Blob;
        }
      );
    });
  });
};
</script>
