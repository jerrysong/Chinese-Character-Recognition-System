using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChineseCharacterRecognitionSystem
{
    class CoarseClassificationProcessor
    {
        private Character inputCharacter;

        public CoarseClassificationProcessor(Character handWrittenInput)
        {
            this.inputCharacter = handWrittenInput;
        }

        public bool IsStrokeCountDifferenceTolerable(Character referenceCharacter, RecognitionMode mode)
        {
            switch (mode)
            {
                case RecognitionMode.Unconstraint:
                    {
                        if (referenceCharacter.StrokeCount() >= inputCharacter.StrokeCount())
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                case RecognitionMode.Fast:
                    {
                        if (referenceCharacter.StrokeCount() == inputCharacter.StrokeCount() || referenceCharacter.StrokeCount() == inputCharacter.StrokeCount() + 1)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                case RecognitionMode.RadicalBased:
                    {
                        if (referenceCharacter.StrokeCount() >= inputCharacter.StrokeCount())
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                default:
                    return false;
            }
        }

        public bool IsSizeDifferenceTolerable(Character referenceCharacter, RecognitionMode mode)
        {
            double differenceRatio = (double)referenceCharacter.PointsCount / (double)inputCharacter.PointsCount;
            double toleranceUpLimit, toleranceDownLimit;
            SetToleranceFactor(out toleranceUpLimit, out toleranceDownLimit, mode);

            if ((toleranceDownLimit < differenceRatio) && (differenceRatio < toleranceUpLimit))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void SetToleranceFactor(out double toleranceUpLimit, out double toleranceDownLimit, RecognitionMode mode)
        {
            toleranceUpLimit = 0;
            toleranceDownLimit = 0;
            switch (mode)
            {
                case RecognitionMode.Unconstraint:
                    {
                        if (inputCharacter.PointsCount < 30)
                        {
                            toleranceUpLimit = 1.5;
                            toleranceDownLimit = 0.5;
                        }
                        else if (inputCharacter.PointsCount < 60)
                        {
                            toleranceUpLimit = 1.4;
                            toleranceDownLimit = 0.6;
                        }
                        else
                        {
                            toleranceUpLimit = 1.4;
                            toleranceDownLimit = 0.6;
                        }
                        break;
                    }
                case RecognitionMode.Fast:
                    {
                        if (inputCharacter.PointsCount < 30)
                        {
                            toleranceUpLimit = 1.4;
                            toleranceDownLimit = 0.5;
                        }
                        else if (inputCharacter.PointsCount < 60)
                        {
                            toleranceUpLimit = 1.3;
                            toleranceDownLimit = 0.6;
                        }
                        else
                        {
                            toleranceUpLimit = 1.3;
                            toleranceDownLimit = 0.7;
                        }
                        break;
                    }
                case RecognitionMode.RadicalBased:
                    {
                        if (inputCharacter.PointsCount < 30)
                        {
                            toleranceUpLimit = 1.5;
                            toleranceDownLimit = 0.5;
                        }
                        else if (inputCharacter.PointsCount < 60)
                        {
                            toleranceUpLimit = 1.4;
                            toleranceDownLimit = 0.6;
                        }
                        else
                        {
                            toleranceUpLimit = 1.4;
                            toleranceDownLimit = 0.6;
                        }
                        break;
                    }
            }
        }
    }
}
