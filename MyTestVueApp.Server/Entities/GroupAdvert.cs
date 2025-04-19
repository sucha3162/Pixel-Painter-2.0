namespace MyTestVueApp.Server.Entities
{
    public class GroupAdvert(string name, int count)
    {
        public string GroupName { get; set; } = name;
        public int MemberCount { get; set; } = count;
    }
}
