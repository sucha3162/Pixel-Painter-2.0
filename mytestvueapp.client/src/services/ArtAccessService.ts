import Art from "../entities/Art";
import Codec from "@/utils/Codec1";

export default class ArtAccessService {
  public static async getAllArt(): Promise<Art[]> {
    try {
      const response = await fetch("/artaccess/GetAllArt");
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

      artpiece.pixelGrid.backgroundColor = "#ffffff";
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

      const request = art.artistId.length > 1? "/artaccess/SaveArtCollab" : "/artaccess/SaveArt";

      const response = await fetch(request, {
        method: "POST",
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

  public static async deleteArt(ArtId: number): Promise<void> {
    try {
      const response = await fetch(`/artaccess/DeleteArt?ArtId=${ArtId}`);

      if (!response.ok) {
        throw new Error("Error: Bad response");
      }
    } catch (error) {
      console.error;
      throw error;
    }
  }
  public static async deleteContributingArtist: Promise<void>(
    ArtistId: number
  ): Promise<void> {
    try {
      const response = await fetch(
        `/artaccess/DeleteContributingArtist?ArtId=${ArtistId}`
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
