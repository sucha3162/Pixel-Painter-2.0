import Notification from "../entities/Notification";

export default class NotificationService {
  public static async getNotifications(artistId: number): Promise<Notification[]> {
    try {
      console.log("Call Controller");
      const response = await fetch(
        `/notification/GetNotifications?artId=${artistId}`
      )
      const jsonNotifications = await response.json();
      console.log(jsonNotifications);

      const allNotifications: Notification[] = [];
      for (const jsonComment of jsonNotifications) {
        let notification = new Notification();
        notification = jsonComment as Notification;
        allNotifications.push(notification);
      }

      return allNotifications;
    } catch (error) {
      console.error;
      return [];
    }
  }
}