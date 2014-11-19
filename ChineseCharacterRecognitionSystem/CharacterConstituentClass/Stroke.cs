using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChineseCharacterRecognitionSystem
{
    public class Stroke
    {
        private List<Point> pointList;
        public StrokeType StrokeType
        {
            get;
            set;
        }

        public Stroke(StrokeType StrokeType)
        {
            this.pointList = new List<Point>();
            this.StrokeType = StrokeType;
        }          

        // Soft copy
        public Stroke(List<Point> pointList)
        {
            this.pointList = pointList;
        }          

        public Point this[int i]
        {
            get 
            {
                if (pointList.Count == 0)
                {
                    return new Point();
                }
                else
                {
                    return pointList[i];                   
                }
            }
            set { pointList[i] = value; }
        }

        public void Add(Point p)
        {
            pointList.Add(p);
        }

        public bool Contains(Point p)
        {
            return pointList.Contains(p);
        }

        public int Count()
        {
            return pointList.Count;
        }

        public Point Last()
        {
            return pointList.Last();
        }

        public Point First()
        {
            return pointList.First();
        }

        public List<Point> Content()
        {
            return pointList;
        }

        public void Insert(int i, Point insertPoint)
        {
            pointList.Insert(i, insertPoint);
        }

        public void RemoveAt(int i)
        {
            pointList.RemoveAt(i);
        }

        public void RemoveLast()
        {
            pointList.RemoveAt(pointList.Count - 1);
        }

        public Stroke Reverse()
        {
            List<Point> reversePointList = new List<Point>();
            for (int i = pointList.Count - 1; i >= 0; i--)
            {
                Point p = new Point();
                p.X = pointList[i].X;
                p.Y = pointList[i].Y;
                p.type = pointList[i].type;
                if (reversePointList.Count() != 0)
                {
                    p.oritentationAngle = Algorithms.GetRadians(reversePointList.Last(), p);
                }
                reversePointList.Add(p);
            }
            return new Stroke(reversePointList);
        }

        public void GetEdgeCoordinate(out int topEdge, out int lowermostEdge, out int rightmostEdge, out int leftmostEdge)
        {
            topEdge = 0;
            rightmostEdge = 0;
            lowermostEdge = Algorithms.Infinity;
            leftmostEdge = Algorithms.Infinity;

            foreach (Point point in pointList)
            {
                if (point.Y > topEdge)
                {
                    topEdge = point.Y;
                }
                if (point.Y < lowermostEdge)
                {
                    lowermostEdge = point.Y;
                }
                if (point.X > rightmostEdge)
                {
                    rightmostEdge = point.X;
                }
                if (point.X < leftmostEdge)
                {
                    leftmostEdge = point.X;
                }
            }
        }

        // Deep copy
        public static Stroke Clone(Stroke s)
        {
            Stroke newStroke = new Stroke(s.StrokeType);
            for (int i = 0; i < s.Count(); i++)
            {
                newStroke.Add(s[i]);
            }

            return newStroke;
        }   
    }
}
