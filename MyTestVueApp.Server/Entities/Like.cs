namespace MyTestVueApp.Server.Entities
{
    public class Like
    {
        public string Artist { get; set; }
        public string Artwork { get; set; }
        public bool Viewed { get; set; }
        public DateTime LikedOn { get; set; }
    }
}
