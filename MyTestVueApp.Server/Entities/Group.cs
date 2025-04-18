using System.Drawing;

namespace MyTestVueApp.Server.Entities
{

    public class Group(string groupName)
    {
        public string Name { get; set; } = groupName;
        public int CanvasSize { get; set; } = 32;
        public string BackgroundColor { get; set; } = "#FFFFFF";
        public List<Artist> CurrentMembers { get; set; } = new();
        public List<Artist> MemberRecord { get; set; } = new();
        public List<string[][]> Pixels { get; set; } = new List<string[][]>();

        
        public Group(string groupName, string[][][] canvas, int canvasSize, string backgroundColor): this(groupName)
        {
            Name = groupName;
            foreach(string[][] grid in canvas){
              Pixels.Add(grid);
            }
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

        public void PaintPixels(int layer, string color, Coordinate[] coords)
        {
            foreach (Coordinate coord in coords)
            {
                Pixels[layer][coord.X][coord.Y] = color;
            }
        }

        public List<List<Pixel>> GetPixelsAsList()
        {
            List<List<Pixel>> pixelVec = new();
            for (int l = 0; l < Pixels.Count; l++) 
            {
                List<Pixel> row = new();
                for (int i = 0; i < CanvasSize; i++)
                {
                    for (int j = 0; j < CanvasSize; j++)
                    {
                        string color = Pixels[l][i][j];
                        if (Pixels[l][i][j] != null)
                        {
                            row.Add(new Pixel(color, i, j));
                        } 
                    }
                }
                pixelVec.Add(row);
            }
            return pixelVec;
        }

        public int GetCurrentMemberCount()
        {
            return CurrentMembers.Count;
        }
    }

}
