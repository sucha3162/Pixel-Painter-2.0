import type Artist from "@/entities/Artist";

export default class LoginService {
  public static async isLoggedIn(): Promise<boolean> {
    try {
      const response = await fetch("/login/IsLoggedIn");
      if (!response.ok) {
        throw new Error("Network response was not ok");
      }
      const isLoggedIn: boolean = (await response.json()) as boolean;
      return isLoggedIn;
    } catch (error) {
      console.error("Error checking login status:", error);
      return false;
    }
  }

  public static async logout(): Promise<void> {
    try {
      const response = await fetch("/login/Logout");
      if (!response.ok) {
        throw new Error("Network response was not ok");
      }
    } catch (error) {
      console.error("Error logging out:", error);
    }
  }

  public static async GetArtistByName(name: string) {
    try {
      const response = await fetch(`/login/GetArtistByName?name=${name}`);
      const json = await response.json();

      return json as Artist;
    } catch (error) {
      console.error;
      throw error;
    }
  }

  public static async GetAllArtists(): Promise<Artist[]> {
    try {
      const response = await fetch(`/login/GetAllArtists`);
      const json = await response.json();

      const allArtists: Artist[] = [];

      for (const jsonArtist of json) {
        allArtists.push(jsonArtist as Artist);
      }

      return allArtists;
    } catch (error) {
      console.error;
      throw error;
    }
  }

  public static async getCurrentUser(): Promise<Artist> {
    try {
      const response = await fetch("/login/GetCurrentUser");

      if (!response.ok) {
        throw new Error("Error retrieving user");
      }

      const data = await response.json();
      const user: Artist = data;

      return user;
    } catch (error) {
      console.error(error);
      throw error;
    }
  }

  public static async getIsAdmin(): Promise<boolean> {
    try {
      const response = await fetch("/login/GetIsAdmin");

      if (!response.ok) {
        throw new Error("Error: Bad response");
      }

      const data = await response.json();
      const isAdmin: boolean = data;

      return isAdmin;
    } catch (error) {
      console.error(error);
      throw error;
    }
  }

  public static async updateUsername(newUsername: any): Promise<boolean> {
    try {
      const response = await fetch(
        `login/UpdateUsername?newUsername=${newUsername}`,
        {
          method: "PUT",
          headers: { "Content-Type": "application/json" }
        }
      );

      if (!response.ok) {
        throw new Error("Error: Bad response");
      }

      const data = await response.json();
      const success: boolean = data;

      return success;
    } catch (error) {
      console.error;
      return false;
    }
  }
  public static async privateSwitchChange(artistId: Number): Promise<void> {
    try {
      const response = await fetch(`/login/privateSwitchChange`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(artistId)
      });

      if (!response.ok) {
        throw new Error(`Error: ${response.status} - ${response.statusText}`);
      }
    } catch (error) {
      console.error;
      throw error;
    }
  }
  public static async deleteArtist(id: number): Promise<void> {
    try {
      const response = await fetch(`/login/DeleteArtist?id=${id}`, {
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
}
