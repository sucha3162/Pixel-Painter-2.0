namespace MyTestVueApp.Server.Entities
{
    public class Artist
    {
        public int id { get; set; } //Public id
        public string subId { get; set; } //SECRET ID
        public string name { get; set; }
        public bool isAdmin { get; set; } = false;
        public bool privateProfile { get; set; } = false;
        public DateTime creationDate { get; set; }
        public string email { get; set; }
    }
}
