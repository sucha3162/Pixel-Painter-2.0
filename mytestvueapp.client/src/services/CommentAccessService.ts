import Comment from "../entities/Comment";

export default class CommentAccessService {
  public static async getCommentsById(artId: number): Promise<any> {
    try {
      const response = await fetch(
        `/comment/GetCommentsByArtId?artId=${artId}`
      );
      const jsonComments = await response.json();

      const allComments: Comment[] = [];
      for (const jsonComment of jsonComments) {
        let comment = new Comment();
        comment = jsonComment as Comment;
        allComments.push(comment);
      }

      return allComments;
    } catch (error) {
      console.error;
    }
  }

  public static async getCommentsByReplyId(ReplyId: number): Promise<any> {
    try {
      const response = await fetch(
        `/comment/GetCommentsByReplyId?replyId=${ReplyId}`
      );
      const jsonComments = await response.json();

      const allComments: Comment[] = [];
      for (const jsonComment of jsonComments) {
        let comment = new Comment();
        comment = jsonComment as Comment;
        allComments.push(comment);
      }

      return allComments;
    } catch (error) {
      console.error;
    }
  }

  public static async PostComment(comment: Comment): Promise<Comment> {
    try {
      comment.creationDate = new Date().toISOString();

      const response = await fetch("/comment/CreateComment", {
        method: "POST",
        body: JSON.stringify(comment),
        headers: { "Content-Type": "application/json" },
      });
      if (!response.ok) {
        throw new Error("Response was false.");
      }

      const json = await response.json();
      const result: Comment = json as Comment;
      return result;
    } catch (error) {
      console.error;
      throw error;
    }
  }
  public static async EditComment(
    commentId: number,
    newMessage: string
  ): Promise<any> {
    try {
      const response = await fetch(
        `/comment/EditComment?commentId=${commentId}&newMessage=${newMessage}`
      );
    } catch (error) {
      console.error;
    }
  }
  public static async DeleteComment(commentId: number): Promise<any> {
    try {
      const response = await fetch(
        `/comment/DeleteComment?commentId=${commentId}`
      );
    } catch (error) {
      console.error;
    }
  }
  public static async postComment(
    comment: string,
    ArtId: Number
  ): Promise<any> {
    try {
      const response = await fetch(
        `/comment/postComment?comment=${comment}&ArtId=${ArtId}`
      );
    } catch (error) {
      console.error;
    }
  }
}
