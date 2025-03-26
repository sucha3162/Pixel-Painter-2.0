namespace MyTestVueApp.Server.Entities
{
    public class Like
    {
        public int ArtistId { get; set; }
        public string Artist { get; set; }
        public int ArtId { get; set; }
        public string Artwork { get; set; }
        public bool Viewed { get; set; }
        public DateTime LikedOn { get; set; }
    }
}
