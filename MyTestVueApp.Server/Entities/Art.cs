namespace MyTestVueApp.Server.Entities
{
    public class Art
    {
        //Required
        public int Id { get; set; }
        public int[] ArtistId { get; set; }
        public string Title { get; set; }
        public bool IsPublic { get; set; }
        public DateTime CreationDate { get; set; }
        public PixelGrid PixelGrid { get; set; }


        //Optional external values
        public string[] ArtistName { get; set; }
        public int NumLikes { get; set; }
        public int NumComments { get; set; }
        public bool CurrentUserIsOwner { get; set; } = false;

        public void SetArtists(Artist[] artist)
        {
            List<int> ids;
            List<String> names;
            ids = new List<int>();
            names = new List<string>();
            for (int i = 0; i < artist.Length; i++)
            {
                ids.Add(artist[i].id);
                names.Add(artist[i].name);
            }
            ArtistId = ids.ToArray();
            ArtistName = names.ToArray();
        }

         public bool currentUserIsAdmin { get; set; } = false;

    }
}
