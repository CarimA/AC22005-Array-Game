using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArrayGame
{
    public struct Vector2
    {
        public int X;
        public int Y;

        public Vector2(int X, int Y)
        {
            this.X = X;
            this.Y = Y;
        }
        
        public override bool Equals(object obj)
        {
            if (obj is Vector2)
            {
                return Equals(this); 
            }
            return false;
        }

        public bool Equals(Vector2 vec)
        {
            return (this.X == vec.X) && (this.Y == vec.Y);
        }

        public static Vector2 operator +(Vector2 vec1, Vector2 vec2)
        {
            vec1.X += vec2.X;
            vec1.Y += vec2.Y;
            return vec1;
        }
    }
}
