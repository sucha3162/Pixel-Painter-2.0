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
  isLiked: boolean;

  isGif: boolean;
  gifID: number;
  gifFrameNum: number;
  gifFps: number;

  constructor() {
    this.id = 0;
    this.title = "";
    this.artistId = [];
    this.artistName = [];

    this.creationDate = "";
    this.isPublic = false;
    this.numLikes = 0;
    this.numComments = 0;
    this.pixelGrid = new PixelGrid(1, 1, "FF0000", false);
    this.currentUserIsOwner = false;

    this.isGif = false;
    this.gifFrameNum = 0;
    this.gifID = 0;
    this.gifFps = 0;
    this.isLiked = false;
  }
}
