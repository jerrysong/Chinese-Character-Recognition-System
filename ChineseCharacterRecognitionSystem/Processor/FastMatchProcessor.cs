using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChineseCharacterRecognitionSystem
{
    class FastMatchProcessor
    {
        private const double PruneCriterionBasedOnOverallMatch = 1.5;
        private Character inputCharacter;
        private int inputCharacterStrokeCount;
        private double[][] matchRiskMatrix;
        private MinimumRiskMatchProcessor minimumRiskMatchProcessor;

        public FastMatchProcessor(Character handWrittenInput)
        {
            this.inputCharacter = handWrittenInput;
            this.inputCharacterStrokeCount = handWrittenInput.StrokeCount();
            matchRiskMatrix = new double[inputCharacterStrokeCount][];
            minimumRiskMatchProcessor = new MinimumRiskMatchProcessor();
        }

        public double GetMatchScore(Character referenceCharacter, double currentBestScore)
        {
            for (int i = 0; i < inputCharacterStrokeCount; i++)
            {
                matchRiskMatrix[i] = new double[referenceCharacter.StrokeCount()];
            }

            bool prune = ComputeRiskMatrix(referenceCharacter, currentBestScore);
            if (prune)
            {
                return 0;
            }

            Dictionary<int, int> matchOrder = minimumRiskMatchProcessor.GetLeastRiskMatchOrder(referenceCharacter, matchRiskMatrix);
            double netScore = 0;
            foreach (KeyValuePair<int, int> item in matchOrder)
            {
                netScore += matchRiskMatrix[item.Key][item.Value];
            }

            return netScore;
        }

        private bool ComputeRiskMatrix(Character referenceCharacter, double currentBestScore)
        {
            double localCharacterBestScore = 0;
            for (int i = 0; i < inputCharacterStrokeCount; i++)
            {
                double singleStrokeBestScore = Algorithms.Infinity;
                for (int j = 0; j < referenceCharacter.StrokeCount(); j++)
                {
                    double localMatchStrokeRisk = DTWProcessor.DTWDistance(inputCharacter[i], referenceCharacter[j]);
                    double localMatchReverseStrokeRisk = DTWProcessor.DTWDistance(inputCharacter[i], referenceCharacter[j].Reverse());
                    matchRiskMatrix[i][j] = Math.Min(localMatchStrokeRisk, localMatchReverseStrokeRisk);

                    if (matchRiskMatrix[i][j] < singleStrokeBestScore)
                    {
                        singleStrokeBestScore = matchRiskMatrix[i][j];
                    }
                }

                localCharacterBestScore += singleStrokeBestScore;
                if (localCharacterBestScore > currentBestScore * PruneCriterionBasedOnOverallMatch)
                {
                    return true;
                }
            }
            return false;
        }       
    }
}
