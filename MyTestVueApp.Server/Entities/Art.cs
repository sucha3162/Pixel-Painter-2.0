namespace MyTestVueApp.Server.Entities
{
    public class Art
    {
        //Required
        public int id { get; set; }
        public int artistId { get; set; }
        public string title { get; set; }
        public bool isPublic { get; set; }
        public DateTime creationDate { get; set; }
        public PixelGrid pixelGrid { get; set; }


        //Optional external values
        public string artistName { get; set; }
        public int numLikes { get; set; }
        public int numComments { get; set; }
        public bool currentUserIsOwner { get; set; } = false;

         public bool currentUserIsAdmin { get; set; } = false;

    }

}
