import { PixelGrid } from "@/entities/PixelGrid";

export default class Codec {
  public static Encode(inputGrid: PixelGrid): string {
    let EncodedPicture: string = "";

    for (let i = 0; i < inputGrid.height; i++) {
      for (let j = 0; j < inputGrid.width; j++) {
        let tmp = inputGrid.grid[i][j];
        if (tmp === "empty") tmp = inputGrid.backgroundColor;
        EncodedPicture = EncodedPicture + tmp;
      }
    }

    let newStr = "";

    for (let i = 0; i < EncodedPicture.length; i++) {
      if (EncodedPicture[i] !== "#") {
        newStr += EncodedPicture[i];
      }
    }

    return newStr;
  }

  public static Decode(
    encodedString: String,
    height: number,
    width: number,
    backgroundColor: string
  ): PixelGrid {
    if (localStorage.getItem("backgroundColor") === null) {
      backgroundColor = "FFFFFF";
    }

    const decodedPicture: PixelGrid = new PixelGrid(
      width,
      height,
      backgroundColor,
      false
    );
    decodedPicture.createGrid(height, width);
    let k = 0;

    if (encodedString === "") {
      return decodedPicture;
    }
    for (let i = 0; i < height; i++) {
      for (let j = 0; j < width; j++) {
        decodedPicture.grid[i][j] = encodedString.slice(k, k + 6);
        k = k + 6;
      }
    }

    return decodedPicture;
  }
}
