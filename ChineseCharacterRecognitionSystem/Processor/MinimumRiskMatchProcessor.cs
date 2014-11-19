using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChineseCharacterRecognitionSystem
{
    class MinimumRiskMatchProcessor
    {
        private const double ClashFactor = 0.4;

        #region For Constraint Free Mode

        public Dictionary<int, StrokeSegment> GetLeastRiskMatchOrder(Character referenceCharacter, List<StrokeSegment>[][] matchRisksMatrix)
        {
            Dictionary<int, StrokeSegment> matchOrder = new Dictionary<int, StrokeSegment>();
            int referenceCharacterStrokeCount = referenceCharacter.StrokeCount();
            List<int> matchedStrokeIndexList = new List<int>();
            List<StrokeSegment> matchedSegments = new List<StrokeSegment>();

            while (matchOrder.Count != referenceCharacterStrokeCount)
            {
                double maxNotMatchRisk = -1;
                int matchStrokeIndexOfReferenceCharacter = 0;
                StrokeSegment matchSegmentOfInputCharacter = new StrokeSegment();
                KeyValuePair<StrokeSegment, double> notMatchRiskPair;

                for (int i = 0; i < referenceCharacterStrokeCount; i++)
                {
                    if (matchedStrokeIndexList.Contains(i))
                    {
                        continue;
                    }

                    notMatchRiskPair = ComputeNotMatchFirstRisk(matchRisksMatrix[i], matchedSegments);
                    if (notMatchRiskPair.Value > maxNotMatchRisk)
                    {
                        maxNotMatchRisk = notMatchRiskPair.Value;
                        matchStrokeIndexOfReferenceCharacter = i;
                        matchSegmentOfInputCharacter = new StrokeSegment(notMatchRiskPair.Key);
                    }
                }

                matchOrder.Add(matchStrokeIndexOfReferenceCharacter, matchSegmentOfInputCharacter);
                matchedStrokeIndexList.Add(matchStrokeIndexOfReferenceCharacter);
                matchedSegments.Add(matchSegmentOfInputCharacter);
            }

            return matchOrder;
        }
       
        private KeyValuePair<StrokeSegment, double> ComputeNotMatchFirstRisk(List<StrokeSegment>[] matchRisksArray, List<StrokeSegment> matchedSegments)
        {
            StrokeSegment leastRiskMatchSegment = new StrokeSegment();
            StrokeSegment secondRiskMatchSegment = new StrokeSegment();

            for (int i = 0; i < matchRisksArray.Count(); i++)
            {
                for (int j = 0; j < matchRisksArray[i].Count(); j++)
                {
                    if (matchRisksArray[i][j].MatchRisk < leastRiskMatchSegment.MatchRisk)
                    {
                        if (!IsClash(matchRisksArray[i][j], matchedSegments))
                        {
                            secondRiskMatchSegment = new StrokeSegment(leastRiskMatchSegment);
                            leastRiskMatchSegment = new StrokeSegment(matchRisksArray[i][j]);
                        }
                    }
                    else if (matchRisksArray[i][j].MatchRisk > leastRiskMatchSegment.MatchRisk && matchRisksArray[i][j].MatchRisk < secondRiskMatchSegment.MatchRisk)
                    {
                        if (!IsClash(matchRisksArray[i][j], matchedSegments))
                        {
                            secondRiskMatchSegment = new StrokeSegment(matchRisksArray[i][j]);
                        }
                    }
                }
            }

            double notMatchRisk = secondRiskMatchSegment.MatchRisk - leastRiskMatchSegment.MatchRisk;
            KeyValuePair<StrokeSegment, double> notMatchRiskPair = new KeyValuePair<StrokeSegment, double>(leastRiskMatchSegment, notMatchRisk);
            return notMatchRiskPair;
        }
   
        private bool IsClash(StrokeSegment newSegment, List<StrokeSegment> matchedSegments)
        {
            foreach (StrokeSegment matchedSegment in matchedSegments)
            {
                if (newSegment.StrokeIndex == matchedSegment.StrokeIndex)
                {
                    int newMatchSegmentMinIndex = newSegment.MinIndex;
                    int newMatchSegmentMaxIndex = newSegment.MaxIndex;
                    int matchedSegmentMinIndex = matchedSegment.MinIndex;
                    int matchedSegmentMaxIndex = matchedSegment.MaxIndex;

                    if (newMatchSegmentMaxIndex > matchedSegmentMinIndex && newMatchSegmentMinIndex <= matchedSegmentMinIndex)
                    {
                        double overlappedLength = newMatchSegmentMaxIndex - matchedSegmentMinIndex;
                        double smallerSegmentLength = Math.Min(newSegment.Length, matchedSegment.Length);
                        if (overlappedLength / smallerSegmentLength >= ClashFactor)
                        {
                            return true;
                        }
                    }
                    else if (matchedSegmentMaxIndex > newMatchSegmentMinIndex && matchedSegmentMinIndex <= newMatchSegmentMinIndex)
                    {
                        double overlappedLength = matchedSegmentMaxIndex - newMatchSegmentMinIndex;
                        double smallerSegmentLength = Math.Min(newSegment.Length, matchedSegment.Length);
                        if (overlappedLength / smallerSegmentLength >= ClashFactor)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        #endregion

        #region For Ligature Free Mode

        public Dictionary<int, int> GetLeastRiskMatchOrder(Character referenceCharacter, double[][] matchRiskMatrix)
        {
            Dictionary<int, int> matchOrder = new Dictionary<int, int>();
            List<int> matchedColumns = new List<int>();
            List<int> matchedRows = new List<int>();
            int inputCharacterStrokeCount = matchRiskMatrix.Count();

            while (matchOrder.Count != inputCharacterStrokeCount)
            {
                double maxNotMatchRisk = -1;
                int inputCharacterStrokeIndex = 0;
                int referenceCharacterStrokeIndex = 0;
                KeyValuePair<int, double> notMatchFirstRisk;
                for (int i = 0; i < inputCharacterStrokeCount; i++)
                {
                    if (matchedRows.Contains(i))
                    {
                        continue;
                    }
                    notMatchFirstRisk = ComputeNotMatchFirstRisk(matchRiskMatrix[i], matchedColumns);
                    if (notMatchFirstRisk.Value > maxNotMatchRisk)
                    {
                        maxNotMatchRisk = notMatchFirstRisk.Value;
                        inputCharacterStrokeIndex = i;
                        referenceCharacterStrokeIndex = notMatchFirstRisk.Key;
                    }
                }
                matchOrder.Add(inputCharacterStrokeIndex, referenceCharacterStrokeIndex);
                matchedRows.Add(inputCharacterStrokeIndex);
                matchedColumns.Add(referenceCharacterStrokeIndex);
            }
            return matchOrder;
        }

        private KeyValuePair<int, double> ComputeNotMatchFirstRisk(double[] matchRiskArray, List<int> matchedColumns)
        {
            int leastRiskIndex = 0;
            double notMatchRisk = 0;
            if (matchedColumns.Count < matchRiskArray.Count() - 1)
            {
                double leastRisk = Algorithms.Infinity;
                double secondLeastRisk = Algorithms.Infinity;
                for (int i = 0; i < matchRiskArray.Count(); i++)
                {
                    if (matchedColumns.Contains(i))
                    {
                        continue;
                    }
                    double localRisk = matchRiskArray[i];
                    if (localRisk < leastRisk)
                    {
                        secondLeastRisk = leastRisk;
                        leastRisk = localRisk;
                        leastRiskIndex = i;
                    }
                    else if (leastRisk < localRisk && localRisk < secondLeastRisk)
                    {
                        secondLeastRisk = localRisk;
                    }
                }
                notMatchRisk = secondLeastRisk - leastRisk;
            }
            else
            {
                for (int i = 0; i < matchRiskArray.Count(); i++)
                {
                    if (!matchedColumns.Contains(i))
                    {
                        leastRiskIndex = i;
                        notMatchRisk = 0;
                    }
                }
            }
            KeyValuePair<int, double> notMatchRiskPair = new KeyValuePair<int, double>(leastRiskIndex, notMatchRisk);
            return notMatchRiskPair;
        }

        #endregion
    }
}
