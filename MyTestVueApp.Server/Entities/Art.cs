namespace MyTestVueApp.Server.Entities
{
    public class Art
    {
        //Required
        public int id { get; set; }
        public int[] artistId { get; set; }
        public string title { get; set; }
        public bool isPublic { get; set; }
        public DateTime creationDate { get; set; }
        public PixelGrid pixelGrid { get; set; }


        //Optional external values
        public string[] artistName { get; set; }
        public bool IsGif { get; set; }
        public int gifID { get; set; }
        public int gifFrameNum { get; set; }
        public int numLikes { get; set; }
        public int numComments { get; set; }
        public bool currentUserIsOwner { get; set; } = false;

        public void SetArtists(Artist[] artist)
        {
            List<int> Ids;
            List<String> names;
            Ids = new List<int>();
            names = new List<string>();
            for (int i = 0; i < artist.Length; i++)
            {
                Ids.Add(artist[i].id);
                names.Add(artist[i].name);
            }
            artistId = Ids.ToArray();
            artistName = names.ToArray();
        }

         public bool currentUserIsAdmin { get; set; } = false;

    }
}
