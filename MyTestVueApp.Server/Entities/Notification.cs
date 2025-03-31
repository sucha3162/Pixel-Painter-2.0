namespace MyTestVueApp.Server.Entities
{
    public class Notification
    {
        public int CommentId { get; set; }
        public int ArtistId { get; set; }
        public int ArtId { get; set; }
        public int Type { get; set; }
        public string User { get; set; }
        public bool Viewed { get; set; }
        public string ArtName { get; set; }
    }
}
