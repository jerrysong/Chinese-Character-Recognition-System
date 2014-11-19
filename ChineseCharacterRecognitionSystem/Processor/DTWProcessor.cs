using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChineseCharacterRecognitionSystem
{
    class DTWProcessor
    {
        private static double Resolution = Math.Pow(20000, 0.5);

        private static double Min(double a, double b, double c)
        {
            return Math.Min(Math.Min(a, b), c);
        }

        public static double DTWDistance(Stroke strokeA, Stroke strokeB)
        {
            return DTWDistance(strokeA.Content(), strokeB.Content());
        }

        public static double DTWDistance(List<Point> series, Stroke Stroke)
        {
            return DTWDistance(series, Stroke.Content());
        }

        public static double DTWDistance(List<Point> seriesA, List<Point> seriesB)
        {
            double[,] DTW = new double[seriesA.Count(), seriesB.Count()];
            DTW[0, 0] = 0;

            if (seriesA.Count() == 1 || seriesB.Count() == 1)
            {
                double distance = 0;
                if (seriesA.Count() == 1)
                {                    
                    for (int i = 0; i < seriesB.Count; i++)
                    {
                        distance += GetPointDistance(seriesA[0], seriesB[i]);
                    }
                }
                else if (seriesB.Count() == 1)
                {
                    for (int i = 0; i < seriesA.Count; i++)
                    {
                        distance += GetPointDistance(seriesA[i], seriesB[0]);
                    }                   
                }
                return distance;
            }

            //double lengthA, ZA, lengthB, ZB, netCost;
            for (int i = 1; i < seriesA.Count(); i++)
            {
                DTW[i, 0] = Algorithms.Infinity;
            }

            for (int i = 1; i < seriesB.Count(); i++)
            {
                DTW[0, i] = Algorithms.Infinity;
            }            

            for (int i = 1; i < seriesA.Count(); i++)
            {
                for (int j = 1; j < seriesB.Count(); j++)
                {
                    double distance = GetPointDistance(seriesA[i], seriesB[j]);
                    double min = Min(DTW[i - 1, j], DTW[i, j - 1], DTW[i - 1, j - 1]);
                    DTW[i, j] = distance + min;
                }

            }

            double finalDistance = DTW[seriesA.Count() - 1, seriesB.Count() - 1];
            return finalDistance;
        }

        private static double GetPointDistance(Point pointA, Point pointB)
        {
            double netDistance = 0, deltaX = 0, deltaY = 0, deltaAngle = 0;
            deltaX = Math.Pow(pointA.X - pointB.X, 2);
            deltaY = Math.Pow(pointA.Y - pointB.Y, 2);
            deltaAngle = Math.Abs(pointA.oritentationAngle - pointB.oritentationAngle);
            deltaAngle = 300 * Math.Pow(Math.Min(deltaAngle, 2 * Math.PI - deltaAngle), 2);
            netDistance = Math.Pow(deltaX + deltaY + deltaAngle, 0.5);
            netDistance /= Resolution;

            if (netDistance >= Algorithms.Infinity)
            {
                throw new Exception();
            }

            return netDistance;
        }
    }
}
