namespace MyTestVueApp.Server.Entities
{
    public class Artist
    {
        public int Id { get; set; } //Public id
        public string SubId { get; set; } //SECRET ID
        public string Name { get; set; }
        public bool IsAdmin { get; set; } = false;
        public bool PrivateProfile { get; set; } = false;
        public DateTime CreationDate { get; set; }
        public string Email { get; set; } = "";
    }
}
