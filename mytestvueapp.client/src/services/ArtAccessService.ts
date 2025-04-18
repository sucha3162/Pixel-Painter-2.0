import type Artist from "../entities/Artist";
import Art from "../entities/Art";
import Comment from "../entities/Comment";
import codec from "@/utils/codec";

export default class ArtAccessService {
  public static async getAllArt(): Promise<Art[]> {
    try {
      const response = await fetch("/artaccess/GetAllArt");
      const json = await response.json();

      const allArt: Art[] = [];

      for (const jsonArt of json) {
        let art = new Art();
        art = jsonArt as Art;

        allArt.push(art);
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
        let art = new Art();
        art = jsonArt as Art;

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
        let art = new Art();
        art = jsonArt as Art;

        allArt.push(art);
      }

      return allArt;
    } catch (error) {
      console.error;
      throw error;
    }
  }

  public static async getArtByLikes(isAscending: boolean): Promise<Art[]> {
    try {
      const response = await fetch(
        `/artaccess/GetArtByLikes?isAscending=${isAscending}`
      );
      const json = await response.json();

      const allArt: Art[] = [];

      for (const jsonArt of json) {
        let art = new Art();
        art = jsonArt as Art;

        allArt.push(art);
      }

      return allArt;
    } catch (error) {
      console.error;
      throw error;
    }
  }

  public static async getArtByComments(isAscending: boolean): Promise<Art[]> {
    try {
      const response = await fetch(
        `/artaccess/GetArtByComments?isAscending=${isAscending}`
      );
      const json = await response.json();

      const allArt: Art[] = [];

      for (const jsonArt of json) {
        let art = new Art();
        art = jsonArt as Art;

        allArt.push(art);
      }

      return allArt;
    } catch (error) {
      console.error;
      throw error;
    }
  }

  public static async getArtByDate(isAscending: boolean): Promise<Art[]> {
    try {
      const response = await fetch(
        `/artaccess/GetArtByDate?isAscending=${isAscending}`
      );
      const json = await response.json();

      const allArt: Art[] = [];

      for (const jsonArt of json) {
        let art = new Art();
        art = jsonArt as Art;

        allArt.push(art);
      }

      return allArt;
    } catch (error) {
      console.error;
      throw error;
    }
  }

  public static async GetCurrentUsersArt(): Promise<Art[]> {
    try {
      const response = await fetch("/artaccess/GetCurrentUsersArt");

      if (!response.ok) {
        throw new Error("Error: Bad response");
      }

      const json = await response.json();
      const allArt: Art[] = [];

      for (const jsonArt of json) {
        let art = new Art();
        art = jsonArt as Art;

        allArt.push(art);
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
      artpiece.pixelGrid.grid = codec.Decode(
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

  public static async SaveArt(art: Art): Promise<Art> {
    try {
      art.creationDate = new Date().toISOString();

      let request = "/artaccess/SaveArt";
      console.log("#Artists: " + art.artistId.length);
      if (art.artistId.length > 1) {
        request = "/artaccess/SaveArtCollab";
      }

      const response = await fetch(request, {
        method: "POST",
        body: JSON.stringify(art),
        headers: { "Content-Type": "application/json" },
      });
      const json = await response.json();

      const artpiece = json as Art;

      return artpiece;
    } catch (error) {
      console.error(error);
      throw error;
    }
  }

  public static async DeleteArt(ArtId: number): Promise<void> {
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
  public static async DeleteContributingArtist(
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
  public static async GetArtists(ArtId: number): Promise<any> {
    try {
      const response = await fetch(`/artaccess/GetArtists?ArtId=${ArtId}`);

      if (!response.ok) {
        throw new Error("Error: Bad response");
      }
    } catch (error) {
      console.error;
      throw error;
    }
    return;
  }
}
