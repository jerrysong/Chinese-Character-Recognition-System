using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChineseCharacterRecognitionSystem
{
    public class StrokeSegment
    {
        public int StrokeIndex;
        public int StartIndex;
        public int EndIndex;
        public int MinIndex;
        public int MaxIndex;
        public int Length;
        public double MatchRisk;

        public StrokeSegment()
        {
            StrokeIndex = 0;
            StartIndex = 0;
            EndIndex = 0;
            MinIndex = 0;
            MaxIndex = 0;
            Length = 0;
            MatchRisk = Algorithms.Infinity;
        }

        public StrokeSegment(int StrokeIndex, int StartIndex, int EndIndex, double MatchRisk)
        {
            this.StrokeIndex = StrokeIndex;
            this.StartIndex = StartIndex;
            this.EndIndex = EndIndex;
            this.MinIndex = Math.Min(StartIndex, EndIndex);
            this.MaxIndex = Math.Max(StartIndex, EndIndex);
            this.MatchRisk = MatchRisk;
            this.Length = Math.Abs(EndIndex - StartIndex);
        }

        public StrokeSegment(StrokeSegment inputSegment)
        {
            this.StrokeIndex = inputSegment.StrokeIndex;
            this.StartIndex = inputSegment.StartIndex;
            this.EndIndex = inputSegment.EndIndex;
            this.MinIndex = inputSegment.MinIndex;
            this.MaxIndex = inputSegment.MaxIndex;
            this.MatchRisk = inputSegment.MatchRisk;
            this.Length = inputSegment.Length;
        }
    }
}
