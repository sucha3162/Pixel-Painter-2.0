namespace MyTestVueApp.Server.Entities
{
    public class Pixel(string color, int x, int y)
    {
        public string Color { get; set; } = color;
        public int X { get; set; } = x;
        public int Y { get; set; } = y;
    }
}
