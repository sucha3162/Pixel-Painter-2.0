import GroupAdvert from "@/entities/GroupAdvert";
export default class SocketService {
  public static async getAllGroups() {
    try {
      const response = await fetch("/socket/GetGroups");
      const json = await response.json();

      const allGroups: GroupAdvert[] = [];

      for (const jsonGroup of json) {
        let group = new GroupAdvert();
        group = jsonGroup as GroupAdvert;
        allGroups.push(group);
      }
  
      return allGroups;
    } catch (error) {
      console.error;
      throw error;
    }
  }
}
