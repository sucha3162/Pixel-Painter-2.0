export default class notification {
  artId: number;
  artistId: number;
  commentId: number;
  type: number; //0 for comment, 1 for likes
  user: string; //user who did the action
  viewed: boolean;
  artName: string;

  constructor() {
    this.artId = -1;
    this.artistId = -1;
    this.commentId = -1;
    this.type = -1;
    this.user = "";
    this.viewed = false;
    this.artName = "";
  }
}