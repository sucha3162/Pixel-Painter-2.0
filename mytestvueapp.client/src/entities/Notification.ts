export default class notification {
    type: number; //0 for comment, 1 for likes
    user: string; //user who did the action
    viewed: boolean;
    art: string;

    constructor() {
    this.type = -1;
    this.user = "";
    this.viewed = false;
    this.art = "";
  }
}