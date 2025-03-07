namespace MyTestVueApp.Server.Entities
{
    public class Notification
    {
        public int type { get; set; }
        public string user { get; set; }
        public bool viewed { get; set; }
        public string artName { get; set; }
    }
}
