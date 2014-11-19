using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChineseCharacterRecognitionSystem
{
    class Preprocessor
    {
        private const int OriginalScale = 400;
        private const int NormalizedScale = 100;
        private const int MaxPixelPitch = 5;
        private const int MeanPixelPitch = 4;
        private const int MinPixelPitch = 3;
        private const int SampleFrequency = 3;
        private List<Stroke> rawStrokeList;
        private List<Stroke> normalizedStrokeList;       
        private List<Stroke> featureStrokeList;

        public Preprocessor(Character rawCharacter)
        {
            this.rawStrokeList = rawCharacter.Content();
            this.normalizedStrokeList = new List<Stroke>();
            this.featureStrokeList = new List<Stroke>();
        }

        public Preprocessor(List<Stroke> strokeList)
        {
            this.rawStrokeList = strokeList;
            this.normalizedStrokeList = new List<Stroke>();
            this.featureStrokeList = new List<Stroke>();
        }

        public void RemoveIsolatedPoints()
        {
            for (int i = 0; i < rawStrokeList.Count(); i++)
            {
                if (rawStrokeList[i].Count() <= MeanPixelPitch)
                {
                    rawStrokeList.RemoveAt(i);
                    i--;
                }
            }
        }

        public void Deformation()
        {
            if (rawStrokeList.Count < 3)
            {
                return;
            }

            Dictionary<int, int> topEdgeRank = new Dictionary<int, int>();
            Dictionary<int, int> lowermostEdgeRank = new Dictionary<int, int>();
            Dictionary<int, int> rightmostEdgeRank = new Dictionary<int, int>();
            Dictionary<int, int> leftmostEdgeRank = new Dictionary<int, int>();

            for (int i = 0; i < rawStrokeList.Count(); i++)
            {
                int localTopEdge, localLowermostEdge, localRightmostEdge, localLeftmostEdge;
                rawStrokeList[i].GetEdgeCoordinate(out localTopEdge, out localLowermostEdge, out localRightmostEdge, out localLeftmostEdge);
               
                topEdgeRank.Add(localTopEdge, i);
                lowermostEdgeRank.Add(localLowermostEdge, i);
                rightmostEdgeRank.Add(localRightmostEdge, i);
                leftmostEdgeRank.Add(localLeftmostEdge, i);
            }
            
            var sortedTopEdgeRank = Algorithms.SortDictionaryDescendingByKey(topEdgeRank);
            KeyValuePair<int, int> globalTopEdge = ((Dictionary<int, int>)sortedTopEdgeRank).ElementAt(0);
            KeyValuePair<int, int> globalSecondTopEdge = ((Dictionary<int, int>)sortedTopEdgeRank).ElementAt(1);

            if (globalTopEdge.Key > globalSecondTopEdge.Key * 1.4)
            {
                int strokeIndex = ((Dictionary<int, int>)sortedTopEdgeRank).ElementAt(0).Value;
                Stroke stroke = rawStrokeList[strokeIndex];
                if (stroke.Last().Y == ((Dictionary<int, int>)sortedTopEdgeRank).ElementAt(0).Key)
                {
                    List<Point> pointList = stroke.Content();
                    int length = pointList.Count;
                }
            }

        }

        public List<Stroke> Normalize()
        {
            Interpolate(rawStrokeList, MaxPixelPitch);
            Filter(rawStrokeList, MinPixelPitch);

            List<Point> strokePointList = new List<Point>();
            for (int i = 0; i < rawStrokeList.Count(); i++)
            {
                strokePointList.AddRange(rawStrokeList[i].Content());
            }

            int leftmostCoordinate, downestCoordinate, rightmostCoordinate, toppestCoordinate;
            GetLayoutCorners(strokePointList, out leftmostCoordinate, out downestCoordinate, out  rightmostCoordinate, out toppestCoordinate);

            int deltaX = leftmostCoordinate;
            int deltaY = downestCoordinate;
            double ScaleX = (double)NormalizedScale / (double)(rightmostCoordinate - leftmostCoordinate);
            double ScaleY = (double)NormalizedScale / (double)(toppestCoordinate - downestCoordinate);

            int normalizedX, normalizedY;
            StrokeType type;
            Point currentPoint, previousPoint;
            for (int j = 0; j < rawStrokeList.Count(); j++)
            {
                Stroke normalizedStroke = new Stroke(StrokeType.Online);
                for (int i = 0; i < rawStrokeList[j].Count(); i++)
                {
                    normalizedX = (int)((rawStrokeList[j][i].X - deltaX) * ScaleX);
                    normalizedY = (int)((rawStrokeList[j][i].Y - deltaY) * ScaleY);
                    type = rawStrokeList[j][i].type;
                    currentPoint = new Point(normalizedX, normalizedY, type);
                    if (i == 0)
                    {
                        normalizedStroke.Add(currentPoint);
                        continue;
                    }

                    normalizedX = (int)((rawStrokeList[j][i - 1].X - deltaX) * ScaleX);
                    normalizedY = (int)((rawStrokeList[j][i - 1].Y - deltaY) * ScaleY);
                    type = rawStrokeList[j][i - 1].type;
                    previousPoint = new Point(normalizedX, normalizedY, type);

                    if (!normalizedStroke.Contains(currentPoint))
                    {
                        currentPoint.oritentationAngle = Algorithms.GetRadians(previousPoint, currentPoint);
                        normalizedStroke.Add(currentPoint);
                    }
                }
                normalizedStrokeList.Add(normalizedStroke);
            }

            Interpolate(normalizedStrokeList, MaxPixelPitch);
            Filter(normalizedStrokeList, MinPixelPitch);
            return normalizedStrokeList;
        }

        public Character GetFeaturedCharacter()
        {
            for (int j = 0; j < normalizedStrokeList.Count; j++)
            {
                Stroke featureStroke = new Stroke(StrokeType.Online);
                int pointCount = normalizedStrokeList[j].Count();

                Point currentPoint = normalizedStrokeList[j][0];
                featureStroke.Add(currentPoint);
                if (normalizedStrokeList[j].Count() == 1)
                {
                    featureStrokeList.Add(featureStroke);
                    continue;
                }

                Point nextPoint = normalizedStrokeList[j][1];                
                for (int i = 1; i < pointCount - 1; i++)
                {
                    currentPoint = nextPoint;
                    nextPoint = normalizedStrokeList[j][i + 1];

                    if (i % SampleFrequency == 0)
                    {
                        featureStroke.Add(currentPoint);
                    }
                    else if (IsCriticalPoint(nextPoint, currentPoint))
                    {
                        featureStroke.Add(nextPoint);
                    }
                }

                if (Algorithms.EuclidDistance(featureStroke.Last(), normalizedStrokeList[j].Last()) > MeanPixelPitch * SampleFrequency / 2)
                {
                    featureStroke.Add(normalizedStrokeList[j].Last());
                }
                else
                {
                    featureStroke.RemoveLast();
                    featureStroke.Add(normalizedStrokeList[j].Last());
                }
                
                featureStrokeList.Add(featureStroke);
            }

            Character featuredCharacter = new Character(featureStrokeList);
            int pointsCount = 0;
            for (int j = 0; j < featuredCharacter.StrokeCount(); j++)
            {
                Stroke featureStroke = featuredCharacter[j];
                pointsCount += featureStroke.Count();
                if (j != 0)
                {
                    int offLinePointsCount = Algorithms.GetOffLinePointsCount(featuredCharacter[j].First(), featuredCharacter[j - 1].Last());
                    pointsCount += offLinePointsCount;
                }
            }
            featuredCharacter.PointsCount = pointsCount;
            return featuredCharacter;
        }

        private void Interpolate(List<Stroke> strokeList, int maxPixelPitch)
        {
            for (int j = 0; j < strokeList.Count(); j++)
            {
                Point currentPoint, nextPoint, insertPoint;
                if (strokeList[j].Count() > 1)
                {
                    for (int i = 0; i < strokeList[j].Count() - 1; i++)
                    {
                        currentPoint = strokeList[j][i];
                        nextPoint = strokeList[j][i + 1];
                        if (Algorithms.EuclidDistance(currentPoint, nextPoint) > maxPixelPitch)
                        {
                            insertPoint = new Point();
                            insertPoint.X = ((currentPoint.X + nextPoint.X) / 2);
                            insertPoint.Y = ((currentPoint.Y + nextPoint.Y) / 2);
                            if (currentPoint.type == StrokeType.Online || nextPoint.type == StrokeType.Online)
                            {
                                insertPoint.type = StrokeType.Online;
                            }
                            else
                            {
                                insertPoint.type = StrokeType.Offline;
                            }
                            strokeList[j].Insert(i + 1, insertPoint);
                            // Stay i unchanged and check the distance between the current point and the new inserted point
                            i--;
                        }
                    }
                }
            }
        }

        private void Filter(List<Stroke> strokeList, int minPixelPitch)
        {
            for (int j = 0; j < strokeList.Count(); j++)
            {
                Point currentPoint, nextPoint;
                if (strokeList[j].Count() > 2)
                {
                    for (int i = 0; i < strokeList[j].Count() - 1; i++)
                    {
                        // Advoid the point list degenerate to only one point
                        if (strokeList[j].Count() == 2)
                        {
                            break;
                        }
                        currentPoint = strokeList[j][i];
                        nextPoint = strokeList[j][i + 1];
                        if (Algorithms.EuclidDistance(currentPoint, nextPoint) < minPixelPitch)
                        {
                            strokeList[j].RemoveAt(i + 1);
                            i--;
                        }
                    }
                }
            }
        }      

        private void GetLayoutCorners(List<Point> strokePointList, out int leftmostCoordinate, out int downestCoordinate, out int rightmostCoordinate, out int toppestCoordinate)
        {
            leftmostCoordinate = OriginalScale;
            downestCoordinate = OriginalScale;
            rightmostCoordinate = 0;
            toppestCoordinate = 0;

            foreach (Point p in strokePointList)
            {
                if (p.X < leftmostCoordinate)
                {
                    leftmostCoordinate = p.X;
                }
                if (p.X > rightmostCoordinate)
                {
                    rightmostCoordinate = p.X;
                }
                if (p.Y < downestCoordinate)
                {
                    downestCoordinate = p.Y;
                }
                if (p.Y > toppestCoordinate)
                {
                    toppestCoordinate = p.Y;
                }
            }
        }

        private bool IsCriticalPoint(Point nextPoint, Point currentPoint)
        {
            if (nextPoint.type == StrokeType.Offline)
            {
                return false;
            }

            double orientationDifference = Math.Min(2 * Math.PI - Math.Abs(nextPoint.oritentationAngle - currentPoint.oritentationAngle), Math.Abs(nextPoint.oritentationAngle - currentPoint.oritentationAngle));
            if (orientationDifference < Math.PI * 3 /4)
            {
                return false;
            }

            return true;
        }
    }
}
