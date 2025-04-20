namespace MyTestVueApp.Server.Entities
{ // ArtId, ArtName, ArtistId, Width, ArtLength, Encode, CreationDate, IsPublic
    public class Comment
    { // Art Table
        public int Id { get; set; }
        public int ArtistId { get; set; }
        public int ArtId { get; set; }
        public string Message { get; set; }
        public string CommenterName { get; set; }
        public DateTime CreationDate { get; set; }
        public bool Viewed { get; set; }

        //Represents the id that this comment is replying to
        public int ReplyId { get; set; }
        public bool CurrentUserIsOwner { get; set; } = false;
    }
}