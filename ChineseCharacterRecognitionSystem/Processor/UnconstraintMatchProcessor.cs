using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChineseCharacterRecognitionSystem
{
    class UnconstraintMatchProcessor
    {        
        private const double UpShiftWindowSizeScale = 1.25;
        private const double DownShiftWindowSizeScale = 0.75;
        private const double ShiftOffsetScale = 3;
        private const double PruneCriterionBasedOnOverallMatch = 1.5;
        private const double PruneCriterionBasedOnSingleStroke = 2;
        private Character inputCharacter;
        private int inputCharacterStrokeCount;
        // This is a m*n matrix, m stands for the stroke count of reference character, n stands for the stroke count of the handwriten character
        private List<StrokeSegment>[][] matchRisksMatrix;
        private MinimumRiskMatchProcessor minimumRiskMatchProcessor;

        public UnconstraintMatchProcessor(Character handWrittenInput)
        {
            inputCharacter = handWrittenInput;
            inputCharacterStrokeCount = handWrittenInput.StrokeCount();
            minimumRiskMatchProcessor = new MinimumRiskMatchProcessor();
        }

        public double GetMatchScoreAdvanced(Character referenceCharacter, double currentBestScore)
        {
            bool prune = ComputeMatchRisksForAllStrokes(referenceCharacter, currentBestScore);
            if (prune)
            {
                return 0;
            }
            
            Character reorderReferenceCharacter = GetReorderCharacter(referenceCharacter);
            List<Point> reorderReferencePoints = StrokesToPointsList(reorderReferenceCharacter);
            List<Point> inputCharacterPoints = StrokesToPointsList(inputCharacter);           

            //double unbalancePenalty = Math.Pow(referenceCharacter.StrokeCount() - reorderReferenceCharacter.StrokeCount(), 2);
            double netSource = DTWProcessor.DTWDistance(inputCharacterPoints, reorderReferencePoints);
            return netSource;
        }

        // Return true if prune criterion is satisfied
        public bool ComputeMatchRisksForAllStrokes(Character referenceCharacter, double currentBestScore)
        {
            int referenceCharacterStrokeCount = referenceCharacter.StrokeCount();
            double localCharacterBestScore = 0;
            matchRisksMatrix = new List<StrokeSegment>[referenceCharacterStrokeCount][];
            for (int i = 0; i < referenceCharacterStrokeCount; i++)
            {
                List<StrokeSegment>[] matchRisksArray = ComputeMatchRisksForSingleStroke(referenceCharacter[i]);
                double singleStrokeBestScore = Algorithms.Infinity;
                foreach (List<StrokeSegment> list in matchRisksArray)
                {
                    foreach (StrokeSegment segment in list)
                    {
                        if (segment.MatchRisk < singleStrokeBestScore)
                        {
                            singleStrokeBestScore = segment.MatchRisk;
                        }
                    }
                }

                localCharacterBestScore += singleStrokeBestScore;
                if (localCharacterBestScore > currentBestScore * PruneCriterionBasedOnOverallMatch)
                {
                    return true;
                }
                matchRisksMatrix[i] = matchRisksArray;                
            }
            return false;
        }

        public List<Point> GetReorderReferencePointsList(Character referenceCharacter)
        {
            Character reorderReferenceCharacter = GetReorderCharacter(referenceCharacter);
            return StrokesToPointsList(reorderReferenceCharacter);
        }

        private List<Point> StrokesToPointsList(Character reorderReferenceCharacter)
        {
            List<Point> pointsList = new List<Point>();
            for (int i = 0; i < reorderReferenceCharacter.StrokeCount(); i++)
            {
                Stroke currentStroke = reorderReferenceCharacter[i];
                for (int j = 0; j < currentStroke.Count(); j++)
                {
                    pointsList.Add(currentStroke[j]);
                }
                if (i != reorderReferenceCharacter.StrokeCount() - 1)
                {
                    Stroke offLineStroke = Algorithms.GetOffLineStroke(reorderReferenceCharacter[i + 1].First(), currentStroke.Last());
                    for (int j = 0; j < offLineStroke.Count(); j++)
                    {
                        pointsList.Add(offLineStroke[j]);
                    }
                }
            }

            return pointsList;
        }

        private Character GetReorderCharacter(Character referenceCharacter)
        {
            Dictionary<int, StrokeSegment> leastRiskMatchOrder = minimumRiskMatchProcessor.GetLeastRiskMatchOrder(referenceCharacter, matchRisksMatrix);

            List<Stroke> reorderStrokes = new List<Stroke>();
            for (int i = 0; i < inputCharacterStrokeCount; i++)
            {
                List<Stroke> nextStroke = GetMergedStrokes(leastRiskMatchOrder, i, referenceCharacter);
                reorderStrokes.AddRange(nextStroke);
            }

            Character reorderCharacter = new Character(reorderStrokes);
            return reorderCharacter;
        }

        private List<Stroke> GetMergedStrokes(Dictionary<int, StrokeSegment> leastRiskMatchOrder, int strokeIndex, Character referenceCharacter)
        {
            Dictionary<int, KeyValuePair<int, StrokeSegment>> matchedSegmentsToSpecificStroke = new Dictionary<int, KeyValuePair<int, StrokeSegment>>();
            foreach (KeyValuePair<int, StrokeSegment> item in leastRiskMatchOrder)
            {
                if (item.Value.StrokeIndex == strokeIndex)
                {
                    int minIndex = item.Value.MinIndex;
                    if (!matchedSegmentsToSpecificStroke.ContainsKey(minIndex))
                    {
                        matchedSegmentsToSpecificStroke.Add(minIndex, item);
                    }
                }
            }

            var sortedSegments = Algorithms.SortDictionaryAscendingByKey(matchedSegmentsToSpecificStroke);
            List<Stroke> mergedStrokes = new List<Stroke>();
            foreach (KeyValuePair<int, KeyValuePair<int, StrokeSegment>> pair in sortedSegments)
            {
                if (pair.Value.Value.EndIndex >= pair.Value.Value.StartIndex)
                {
                    mergedStrokes.Add(referenceCharacter[pair.Value.Key]);
                }
                else
                {
                    mergedStrokes.Add(referenceCharacter[pair.Value.Key].Reverse());
                }
            }

            return mergedStrokes;
        }

        private List<StrokeSegment>[] ComputeMatchRisksForSingleStroke(Stroke referenceStroke)
        {
            List<StrokeSegment>[] matchRisksArray = new List<StrokeSegment>[inputCharacterStrokeCount];
            int upShiftWindowSizes = Convert.ToInt32(referenceStroke.Count() * UpShiftWindowSizeScale);
            int downShiftWindowSizes = Convert.ToInt32(referenceStroke.Count() * DownShiftWindowSizeScale);
            int shiftOffset = Math.Max(Convert.ToInt32(referenceStroke.Count() / ShiftOffsetScale), 1);
            for (int i = 0; i < inputCharacterStrokeCount; i++)
            {
                matchRisksArray[i] = ComputeSimilarityBetweenTwoStrokes(inputCharacter[i], referenceStroke, i, upShiftWindowSizes, shiftOffset);
                matchRisksArray[i].AddRange(ComputeSimilarityBetweenTwoStrokes(inputCharacter[i], referenceStroke, i, downShiftWindowSizes, shiftOffset));
            }

            return matchRisksArray;
        }

        private List<StrokeSegment> ComputeSimilarityBetweenTwoStrokes(Stroke inputStroke, Stroke referenceStroke, int inputStrokeIndex, int shiftWindowSize, int shiftOffset)
        {
            double localMatchRisk, localReverseMatchRisk;
            double leastRisk = Algorithms.Infinity;
            int leastRiskStartIndex = 0, leastRiskEndIndex = 0;
            List<StrokeSegment> matchSegments = new List<StrokeSegment>();

            if (inputStroke.Count() <= shiftWindowSize)
            {
                localMatchRisk = DTWProcessor.DTWDistance(inputStroke, referenceStroke);
                localReverseMatchRisk = DTWProcessor.DTWDistance(inputStroke, referenceStroke.Reverse());
                if (localMatchRisk <= localReverseMatchRisk)
                {
                    leastRisk = localMatchRisk;
                    leastRiskStartIndex = 0;
                    leastRiskEndIndex = inputStroke.Count() - 1;
                }
                else
                {
                    leastRisk = localReverseMatchRisk;
                    leastRiskStartIndex = inputStroke.Count() - 1;
                    leastRiskEndIndex = 0;
                }
                matchSegments.Add(new StrokeSegment(inputStrokeIndex, leastRiskStartIndex, leastRiskEndIndex, leastRisk));
                return matchSegments;
            }

            for (int i = 0; i + shiftWindowSize - 1 < inputStroke.Count(); i += shiftOffset)
            {
                int localWindowSize = 0;
                if (i + shiftWindowSize > inputStroke.Count() - 1)
                {
                    localWindowSize = inputStroke.Count() - i;
                }
                else
                {
                    localWindowSize = shiftWindowSize;
                }

                List<Point> matchSegment = inputStroke.Content().GetRange(i, localWindowSize);
                localMatchRisk = DTWProcessor.DTWDistance(matchSegment, referenceStroke);
                localReverseMatchRisk = DTWProcessor.DTWDistance(matchSegment, referenceStroke.Reverse());

                if (localMatchRisk <= localReverseMatchRisk)
                {
                    leastRisk = localMatchRisk;
                    leastRiskStartIndex = i;
                    leastRiskEndIndex = i + localWindowSize - 1;
                }
                else
                {
                    leastRisk = localReverseMatchRisk;
                    leastRiskStartIndex = i + localWindowSize - 1;
                    leastRiskEndIndex = i;
                }
                matchSegments.Add(new StrokeSegment(inputStrokeIndex, leastRiskStartIndex, leastRiskEndIndex, leastRisk));
            }

            return matchSegments;
        }
    }
}
