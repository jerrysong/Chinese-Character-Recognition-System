using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChineseCharacterRecognitionSystem
{
    public struct Point
    {
        public int X;
        public int Y;
        public StrokeType type;
        public double oritentationAngle;

        public Point(int X, int Y, StrokeType type)
        {
            this.X = X;
            this.Y = Y;
            this.type = type;
            this.oritentationAngle = 0;
        }

        public System.Windows.Point ToSystemPoint()
        {
            return new System.Windows.Point(X, Y);
        }

        public bool Equals(Point p)
        {
            if (this.X == p.X && this.Y == p.Y)
                return true;
            else
                return false;
        }
    }
}
