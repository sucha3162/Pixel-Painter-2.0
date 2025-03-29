namespace MyTestVueApp.Server.Entities
{ // ArtId, ArtName, ArtistId, Width, ArtLength, Encode, CreationDate, IsPublic
    public class Comment
    { // Art Table
        
        public int id { get; set; }
        public int artistId { get; set; }
        public int artId { get; set; }
        public string message { get; set; }
        public string commenterName { get; set; }
        public DateTime creationDate { get; set; }
        public bool Viewed { get; set; }

        //Represents the id that this comment is replying to
        public int replyId { get; set; }
        public bool currentUserIsOwner { get; set; } = false;
    }
}