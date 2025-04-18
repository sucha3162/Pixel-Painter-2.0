import Comment from "../entities/Comment";

export default class CommentAccessService {
  public static async getCommentsByArtId(artId: number): Promise<Comment[]> {
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
      return [];
    }
  }

  public static async getCommentsByReplyId(ReplyId: number): Promise<Comment[]> {
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
      return [];
    }
  }

  public static async postComment(comment: Comment): Promise<Comment> {
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
  public static async editComment(
    comment: Comment,
    newMessage: string
  ): Promise<void> {
    try {
      const altComment: Comment = {
        ...comment,
        message: newMessage
      }
      await fetch(
        '/comment/EditComment?commentId', {
          method: "PUT",
          body: JSON.stringify(altComment),
          headers: { "Content-Type": "application/json" }
        }
      );
    } catch (error) {
      console.error;
    }
  }
  public static async deleteComment(commentId: number): Promise<void> {
    try {
      await fetch(
        `/comment/DeleteComment?commentId=${commentId}`, {
          method: "DELETE",
          headers: { "Content-Type": "application/json" }
        }
      );
    } catch (error) {
      console.error;
    }
  }
}
