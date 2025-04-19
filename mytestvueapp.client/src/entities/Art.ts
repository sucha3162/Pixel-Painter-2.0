import { PixelGrid } from "./PixelGrid";

export default class Art {
  //required
  id: number;
  artistId: number[];
  title: string;
  isPublic: boolean;
  creationDate: string;

  //may be null if new
  pixelGrid: PixelGrid;

  //optional
  artistName: string[];
  numLikes: number;
  numComments: number;
  currentUserIsOwner: boolean;

  IsGif: boolean;
  gifId: number;
  gifFrameNum: number;

  constructor() {
    this.id = 0;
    this.title = "";
    this.artistId = [0];
    this.artistName = [""];

    this.creationDate = "";
    this.isPublic = false;
    this.numLikes = 0;
    this.numComments = 0;
    this.pixelGrid = new PixelGrid(1, 1, "FF0000", false);
    this.currentUserIsOwner = false;

    this.IsGif = false;
    this.gifFrameNum = 0;
    this.gifId = 0;
  }
}
