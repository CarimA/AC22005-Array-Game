using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArrayGame
{
    public class Player
    {
        public Vector2 Position;
        public int Floor;
        public int Points;

        public Player()
        {
            this.Position = new Vector2();
            this.Floor = 1;
            this.Points = 0;
        }

        public void AddPoints(int Points)
        {
            this.Points += Points;
        }
    }
}
