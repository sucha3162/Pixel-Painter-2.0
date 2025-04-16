export default class Artist {
  //required
  id: number;
  name: string;
  isAdmin: boolean;
  privateProfile: boolean;
  creationDate: string;
  email: string;

  constructor() {
    this.id = 0;
    this.name = "";
    this.isAdmin = false;
    this.privateProfile = false;
    this.creationDate = "";
    this.email = "";
  }
}
