namespace MyTestVueApp.Server.Entities
{
    public class Notification
    {
        public int commentId { get; set; }
        public int ArtistId { get; set; }
        public int ArtId { get; set; }
        public int type { get; set; }
        public string user { get; set; }
        public bool viewed { get; set; }
        public string artName { get; set; }
    }
}
