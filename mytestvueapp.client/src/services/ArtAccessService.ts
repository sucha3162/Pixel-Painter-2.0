import Art from "../entities/Art";
import Codec from "@/utils/Codec";

export default class ArtAccessService {
  public static async getAllArt(): Promise<Art[]> {
    try {
      const response = await fetch("/artaccess/GetAllArt");
      if (!response.ok) {
        throw new Error("Error grabbign art");
      }
      const json = await response.json();
      const allArt: Art[] = [];

      for (const jsonArt of json) {
        allArt.push(jsonArt as Art);
      }

      return allArt;
    } catch (error) {
      console.error;
      throw error;
    }
  }

  public static async getAllArtByUserID(id: number): Promise<Art[]> {
    try {
      const response = await fetch(`/artaccess/GetAllArtByUserID?id=${id}`);
      const json = await response.json();

      const allArt: Art[] = [];

      for (const jsonArt of json) {
        allArt.push(jsonArt as Art);
      }

      return allArt;
    } catch (error) {
      console.error;
      throw error;
    }
  }

  public static async getLikedArt(artistId: number): Promise<Art[]> {
    try {
      const response = await fetch(
        `/artaccess/GetLikedArt?artistId=${artistId}`
      );
      const json = await response.json();
      const allArt: Art[] = [];
      for (const jsonArt of json) {
        allArt.push(jsonArt as Art);
      }
      return allArt;
    } catch (error) {
      console.error;
      throw error;
    }
  }
  public static async getCurrentUsersArt(): Promise<Art[]> {
    try {
      const response = await fetch("/artaccess/GetCurrentUsersArt");

      if (!response.ok) {
        throw new Error("Error: Bad response");
      }

      const json = await response.json();
      const allArt: Art[] = [];

      for (const jsonArt of json) {
        allArt.push(jsonArt as Art);
      }

      return allArt;
    } catch (error) {
      console.error;
      throw error;
    }
  }

  public static async getArtById(artId: number): Promise<Art> {
    try {
      const response = await fetch(`/artaccess/GetArtById?id=${artId}`);

      if (!response.ok) {
        throw new Error("Error: Bad response");
      }

      const json = await response.json();

      const artpiece = json as Art;

      artpiece.pixelGrid.backgroundColor = "FFFFFF";
      artpiece.pixelGrid.grid = Codec.Decode(
        artpiece.pixelGrid.encodedGrid || "",
        artpiece.pixelGrid.height,
        artpiece.pixelGrid.width,
        artpiece.pixelGrid.backgroundColor
      ).grid;

      return artpiece;
    } catch (error) {
      console.error;
      throw error;
    }
  }

  public static async saveArt(art: Art): Promise<Art> {
    try {
      art.creationDate = new Date().toISOString();

      const request = "/artaccess/SaveArt";

      const response = await fetch(request, {
        method: "PUT",
        body: JSON.stringify(art),
        headers: { "Content-Type": "application/json" }
      });
      const json = await response.json();
      const artpiece = json as Art;

      return artpiece;
    } catch (error) {
      console.error(error);
      throw error;
    }
  }

  public static async saveGif(art: Art[]): Promise<Art> {
    try {
      for (let i = 0; i < art.length; i++) {
        art[i].creationDate = new Date().toISOString();
      }
      const request = "/artaccess/SaveGif";

      const response = await fetch(request, {
        method: "PUT",
        body: JSON.stringify(art),
        headers: { "Content-Type": "application/json" }
      });
      const json = await response.json();

      if (Array.isArray(json)) {
        if (json.length === 0) {
          throw new Error("SaveGif returned an empty array");
        }
        return json[0] as Art;
      }


      return json;
    } catch (error) {
      console.error(error);
      throw error;
    }
  }
  public static async getGif(GifId: number): Promise<Art[]> {
    try {
      const response = await fetch(`/artaccess/GetGifById?id=${GifId}`);

      if (!response.ok) {
        throw new Error("Error: Bad response");
      }

      const json = await response.json();
      const GifArt: Art[] = [];

      for (const jsonArt of json) {
        let art = new Art();
        art = jsonArt as Art;

        GifArt.push(art);
      }

      return GifArt;
    } catch (error) {
      console.error;
      throw error;
    }
  }

  public static async deleteArt(ArtId: number): Promise<void> {
    try {
      const response = await fetch(`/artaccess/DeleteArt?ArtId=${ArtId}`, {
        method: "DELETE",
        headers: { "Content-Type": "application/json" }
      });

      if (!response.ok) {
        throw new Error("Error: Bad response");
      }
    } catch (error) {
      console.error;
      throw error;
    }
  }
  public static async deleteContributingArtist(
    ArtistId: number
  ): Promise<void> {
    try {
      const response = await fetch(
        `/artaccess/DeleteContributingArtist?ArtId=${ArtistId}`,
        {
          method: "DELETE",
          headers: { "Content-Type": "application/json" }
        }
      );

      if (!response.ok) {
        throw new Error("Error: Bad response");
      }
    } catch (error) {
      console.error;
      throw error;
    }
  }
}
