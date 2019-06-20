using System;
using System.Text;

namespace Maze
{
    public class MazeData
    {
        public string Id { get; }
        public (int width, int height) Dimensions { get; }
        public bool[,] Map { get; }

        public MazeData(string id, (int, int) dimensions, bool[,] map)
        {
            Id = id;
            Dimensions = dimensions;
            Map = map;
        }

        public string ToMapString()
        {
            var sb = new StringBuilder();
            int rowLength = Map.GetLength(0);
            int colLength = Map.GetLength(1);


            for (int j = colLength - 1; j >= 0; j--)
            {
                for (int i = 0; i < rowLength; i++)
                {
                    sb.Append($"{(Map[i, j] ? 1 : 0)} ");
                }

                sb.Append(Environment.NewLine);
            }

            return sb.ToString();
        }
    }
}