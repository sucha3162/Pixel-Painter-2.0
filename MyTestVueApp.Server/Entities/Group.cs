namespace MyTestVueApp.Server.Entities
{

    public class Group(string groupName)
    {
        public string Name { get; set; } = groupName;
        public int CanvasSize { get; set; } = 32;
        public string BackgroundColor { get; set; } = "#FFFFFF";
        public List<Artist> CurrentMembers { get; set; } = new();
        public List<Artist> MemberRecord { get; set; } = new();
        public string[][] Pixels { get; set; } = new string[32][];

        
        public Group(string groupName, string[][] canvas, int canvasSize, string backgroundColor): this(groupName)
        {
            Name = groupName;
            Pixels = canvas;
            CanvasSize = canvasSize;
            BackgroundColor = backgroundColor;
            CurrentMembers = new();
        }

        public bool IsEmpty()
        {
            return CurrentMembers.Count == 0;
        }

        public void AddMember(Artist member)
        {
            if (!CurrentMembers.Any(cm => cm.id == member.id))
            {
                CurrentMembers.Add(member);
            }
            if (!MemberRecord.Any(mr => mr.id == member.id))
            {
                MemberRecord.Add(member);
            }
        }

        public void RemoveMember(Artist member)
        {
            CurrentMembers.Remove(member);
        }

        public void PaintPixels(string color, Coordinate[] coords)
        {
            foreach (Coordinate coord in coords)
            {
                Pixels[coord.X][coord.Y] = color;
            }
        }

        public List<Pixel> GetPixelsAsList()
        {
            List<Pixel> pixelVec = new();
            for (int i = 0; i < Pixels.GetLength(0); i++)
            {
                for (int j = 0; j < Pixels.GetLength(0); j++)
                {
                    if (Pixels[i][j] != null)
                    {
                        pixelVec.Add(new Pixel(Pixels[i][j], i, j));
                    } else
                    {
                        pixelVec.Add(new Pixel(BackgroundColor, i, j));
                    }
                }
            }
            return pixelVec;
        }

        public int GetCurrentMemberCount()
        {
            return CurrentMembers.Count;
        }
    }

}
