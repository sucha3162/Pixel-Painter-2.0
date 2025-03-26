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
      console.error(error);
    }
  }
  public static async markCommentViewed(commentId: number): Promise<any> {
    try {
      const response = await fetch(`/notification/markCommentViewed`, {method: 'PUT', body: JSON.stringify(commentId), headers: { "Content-Type": "application/json" }});
      if(response.status === 200){
        return true
      } else {
        throw new Error("Problem marking notification viewed in database");
      }
    } catch (error){
      console.error(error);
    }
  }
  public static async markLikeViewed(artId: number, artistId: number): Promise<any> {
    try {
      const response = await fetch(`/notification/markCommentViewed`, {method: 'PUT', body: JSON.stringify({artId: artId, artistId: artistId}), headers: { "Content-Type": "application/json" }});
      if(response.status === 200){
        return true
      } else {
        throw new Error("Problem marking notification viewed in database");
      }
    } catch (error){
      console.error(error);
    }
  }
}