import Notification from "../entities/Notification";

export default class NotificationService {
  public static async getNotifications(artistId: number): Promise<any> {
    try {
      const response = await fetch(
        `/notification/GetNotifications?userId=${artistId}`
      );
      if(!response.ok){
        throw new Error("Problem getting notifications");
      }
      const data = await response.json();
      

      const allNotifications: Notification[] = [];
      for (const newNotification of data) {
        let notification = new Notification();
        notification = newNotification as Notification;
        allNotifications.push(notification);
      }

      return allNotifications;
    } catch (error) {
      console.error;
      throw error;
    }
  }
}