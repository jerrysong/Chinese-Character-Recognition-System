using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChineseCharacterRecognitionSystem
{
    class RadicalBasedMatchProcessor
    {
        public static List<Character> ReferenceRadicals;
        private Character inputCharacter;
        private int inputCharacterStrokeCount;
        private Dictionary<int, double> radicalMatchScores;

        public RadicalBasedMatchProcessor(Character handWrittenInput)
        {
            radicalMatchScores = new Dictionary<int, double>();
            inputCharacter = handWrittenInput;
            inputCharacterStrokeCount = handWrittenInput.StrokeCount();
        }

        public double GetRadicalMatchScore(Character referenceCharacter)
        {
            int radicalIndex = Algorithms.RadicalToIndex(referenceCharacter.Radical);
            if (!radicalMatchScores.ContainsKey(radicalIndex))
            {
                double score = ComputeMatchScoreForSingleRadical(radicalIndex);
                radicalMatchScores.Add(radicalIndex, score);
            }
            return radicalMatchScores[radicalIndex];
        }

        public Dictionary<int, double> ComputeRadicalMatchScoreDict()
        {
            for (int i = 0; i < ReferenceRadicals.Count; i++)
            {
                double score = ComputeMatchScoreForSingleRadical(i);
                radicalMatchScores.Add(i, score);
            }

            return radicalMatchScores;
        }

        private double ComputeMatchScoreForSingleRadical(int radicalIndex)
        {
            if (radicalIndex < 0)
            {
                return 0;
            }

            Character radical = ReferenceRadicals[radicalIndex];
            int radicalStrokeCount = radical.StrokeCount();
            double bestScore = Algorithms.Infinity;
            int shiftWindowSize = radicalStrokeCount;

            if (inputCharacterStrokeCount <= radicalStrokeCount)
            {
                UnconstraintMatchProcessor unconstraintMatchProcessor = new UnconstraintMatchProcessor(inputCharacter);
                bestScore = unconstraintMatchProcessor.GetMatchScoreAdvanced(radical, Algorithms.Infinity);

                bestScore /= Math.Pow(radical.PointsCount, 0.5);
                return bestScore;
            }

            for (int i = 0; i + shiftWindowSize - 1 < inputCharacterStrokeCount; i++)
            {
                Character characterPortion = Algorithms.GetCharacterPortion(inputCharacter, i, shiftWindowSize);
                UnconstraintMatchProcessor unconstraintMatchProcessor = new UnconstraintMatchProcessor(characterPortion);
                double currentScore = unconstraintMatchProcessor.GetMatchScoreAdvanced(radical, Algorithms.Infinity);
                if (currentScore < bestScore)
                {
                    bestScore = currentScore;
                }
            }

            bestScore /= Math.Pow(radical.PointsCount, 0.5);
            return bestScore;
        }        
    }
}
