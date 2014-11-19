using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace ChineseCharacterRecognitionSystem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private static int testSampleIndex = 0;
        private static List<double> timeConsumptionRecord = new List<double>();
        private static List<int> correctResultRankRecord = new List<int>();

        private string DatabaseDirectory = Directory.GetCurrentDirectory() + "\\Database\\";       
        private RecognitionMode mode;
        private TestSampleBatch sampleBatch;
        private Point currentPoint;
        private Point previousPoint;
        private Stroke currentStroke;        
        private Character rawInputCharacter;
        private Character featuredInputCharacter;
        private List<Character> referenceCharacters;
        private List<Character> referenceRadicals;
        private List<Character> testSampleCharacters; 

        public ObservableCollection<string> ScorePanel
        {
            get;
            set;
        }

        public RecognitionMode Mode
        {
            get { return mode; }
            set 
            { 
                mode = value;
                RaisePropertyChanged("Mode");
            }
        }

        public TestSampleBatch SampleBatch
        {
            get { return sampleBatch; }
            set
            {
                sampleBatch = value;
                RaisePropertyChanged("SampleBatch");
                testSampleCharacters = DatabaseProcessor.LoadTestCharacters(DatabaseDirectory, sampleBatch);
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            Initialize();
            referenceCharacters = DatabaseProcessor.LoadReferenceCharacterDatabase(DatabaseDirectory);
            referenceRadicals = DatabaseProcessor.LoadRadicalDatabase(DatabaseDirectory);
            testSampleCharacters = DatabaseProcessor.LoadTestCharacters(DatabaseDirectory, SampleBatch);
            ScorePanel = new ObservableCollection<string>();
            DataContext = this;
            Mode = RecognitionMode.Unconstraint;
            RadicalBasedMatchProcessor.ReferenceRadicals = referenceRadicals;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        private void Initialize()
        {
            previousPoint = new Point(-1, -1, 0);
            rawInputCharacter = new Character();
        }
        
        #region Mouse Event Handler

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (IsOutsidePaintPanel(e))
            {
                return;
            }

            currentPoint = new Point((int)e.GetPosition(this).X, (int)e.GetPosition(this).Y, StrokeType.Online);
            currentStroke = new Stroke(StrokeType.Online);
            currentStroke.Add(currentPoint);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (IsOutsidePaintPanel(e))
            {
                return;
            }

            currentPoint = new Point((int)e.GetPosition(this).X, (int)e.GetPosition(this).Y, StrokeType.Online);
            currentStroke.Add(currentPoint);
            rawInputCharacter.Add(currentStroke);
            previousPoint = currentPoint;

            if (mode == RecognitionMode.Fast)
            {
                InputCharacterNormalizedPanel.Children.Clear();
                InputCharacterFeaturePointsPanel.Children.Clear();
                InputCharacterLinkedStrokesPanel.Children.Clear();
                Button_CharacterMatch_Click(this,new RoutedEventArgs());
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (!IsOutsidePaintPanel(e))
            {
                this.Cursor = System.Windows.Input.Cursors.Pen;
            }
            else
            {
                this.Cursor = System.Windows.Input.Cursors.Arrow;
            }

            // Draw the on line
            if (e.LeftButton == MouseButtonState.Pressed && this.PaintPanel.IsMouseOver)
            {
                Point nextPoint = new Point((int)e.GetPosition(this).X, (int)e.GetPosition(this).Y, StrokeType.Online);
                Line line = Algorithms.GetLine(currentPoint, nextPoint, StrokeType.Online);
                line.StrokeThickness = 3;
                PaintPanel.Children.Add(line);

                currentPoint = new Point(nextPoint.X, nextPoint.Y, StrokeType.Online);
                currentStroke.Add(currentPoint);
            }
        }

        #endregion

        #region Button Event Handler

        private void Button_SaveReferenceCharacter_Click(object sender, RoutedEventArgs e)
        {
            SaveReferenceCharacter();
        }

        private void Button_CharacterMatch_Click(object sender, RoutedEventArgs e)
        {
            Complete();
            if (featuredInputCharacter == null)
            {
                return;
            }

            int filteredCountByStrokeCountDisparity = 0;
            int filteredCountBySizeDisparity = 0;
            int filteredCountByPrune = 0;
            int filteredCountByRadical = 0;
            string timeInterval = null;
            Dictionary<double, List<int>> scoreDict = null;

            Thread matchingThread = new Thread(new ThreadStart(() =>
                {
                    switch (mode)
                    {
                        case RecognitionMode.Unconstraint:
                            {
                                UnconstraintMatch(out filteredCountByStrokeCountDisparity, out filteredCountBySizeDisparity, out filteredCountByPrune, out timeInterval, out scoreDict);
                                break;
                            }
                        case RecognitionMode.Fast:
                            {
                                FastMatch(out filteredCountByStrokeCountDisparity, out filteredCountBySizeDisparity, out filteredCountByPrune, out timeInterval, out scoreDict);
                                break;
                            }
                        case RecognitionMode.RadicalBased:
                            {
                                RadicalBasedMatch(out filteredCountByStrokeCountDisparity, out filteredCountBySizeDisparity, out filteredCountByPrune, out filteredCountByRadical, out timeInterval, out scoreDict);
                                break;
                            }
                    }
                   
                    Dispatcher.Invoke(new Action(() =>
                    {
                        ScorePanel.Clear();
                        ScorePanel.Add(String.Format("Running Time: {0}ms", timeInterval));
                        ScorePanel.Add(String.Format("Filtered Out by Stroke Count Disparity: {0}", filteredCountByStrokeCountDisparity));
                        ScorePanel.Add(String.Format("Filtered Out by Size Disparity: {0}", filteredCountBySizeDisparity));
                        ScorePanel.Add(String.Format("Filtered Out by Prune: {0}", filteredCountByPrune));
                        if (mode == RecognitionMode.RadicalBased)
                        {
                            ScorePanel.Add(String.Format("Filtered Out by Radical: {0}", filteredCountByRadical));
                        }

                        int rank = 1;
                        foreach (KeyValuePair<double, List<int>> pair in scoreDict)
                        {
                            foreach (int index in pair.Value)
                            {
                                ScorePanel.Add(String.Format("Rank:{0}, Score:{1}, Character: {2}, Index: {3} ", rank, pair.Key.ToString("F2"), referenceCharacters[index].Font, index));
                                rank++;
                                timeConsumptionRecord.Add(pair.Key);
                            }
                        }
                    }));
                }));
            matchingThread.Start();
        }

        private void Button_RadicalMatch_Click(object sender, RoutedEventArgs e)
        {
            Complete();
            Thread matchingThread = new Thread(new ThreadStart(() =>
            {
                DateTime startTime, endTime;
                startTime = DateTime.Now;
                RadicalBasedMatchProcessor radicalBasedMatchProcessor = new RadicalBasedMatchProcessor(featuredInputCharacter);
                Dictionary<int, double> radicalMatchScores = radicalBasedMatchProcessor.ComputeRadicalMatchScoreDict();
                endTime = DateTime.Now;

                string timeInterval = Algorithms.GetTimeDifference(startTime, endTime);
                var sortedScores = from pair in radicalMatchScores
                                   orderby pair.Value ascending
                                   select pair;

                Dispatcher.Invoke(new Action(() =>
                {
                    ScorePanel.Clear();
                    ScorePanel.Add(String.Format("Running Time: {0}ms", timeInterval));
                    foreach (KeyValuePair<int, double> pair in sortedScores)
                    {
                        ScorePanel.Add(String.Format("Score:{0}, Radical: {1}, Index: {2} ", pair.Value.ToString("F2"), referenceRadicals[pair.Key].Font, pair.Key));
                    }
                }));
            }));
            matchingThread.Start();
        }

        private void Button_SingleCharacterMatch_Click(object sender, RoutedEventArgs e)
        {
            Complete();
            string inputCharacter = Microsoft.VisualBasic.Interaction.InputBox("Input the character here", "Input Box", "三", -1, -1);
            int referenceCharacterIndex = -1;
            if (inputCharacter == "")
            {
                return;
            }
            for (int i = 0; i < referenceCharacters.Count(); i++)
            {
                if (inputCharacter[0] == referenceCharacters[i].Font)
                {
                    referenceCharacterIndex = i;
                    break;
                }
            }
            if (referenceCharacterIndex == -1)
            {
                MessageBox.Show("No This Character", "Warn");
                return;
            }
            
            ReferenceCharacterStrokesPanel.Children.Clear();
            ReferenceCharacterFeaturePointsPanel.Children.Clear();
            ReferenceCharacterLinkedStrokesPanel.Children.Clear();
            
            Character referenceCharacter = referenceCharacters[referenceCharacterIndex];

            for (int j = 0; j < referenceCharacter.StrokeCount(); j++)
            {
                for (int i = 0; i < referenceCharacter[j].Count() - 1; i++)
                {
                    Line line = Algorithms.GetLine(referenceCharacter[j][i], referenceCharacter[j][i + 1], StrokeType.Online);
                    ReferenceCharacterStrokesPanel.Children.Add(line);
                }
            }

            UnconstraintMatchProcessor unconstraintMatchProcessor = new UnconstraintMatchProcessor(featuredInputCharacter);
            unconstraintMatchProcessor.ComputeMatchRisksForAllStrokes(referenceCharacter, Algorithms.Infinity);
            List<Point> reorderPointsList = unconstraintMatchProcessor.GetReorderReferencePointsList(referenceCharacter);
            double score = unconstraintMatchProcessor.GetMatchScoreAdvanced(referenceCharacter, Algorithms.Infinity);

            for (int i = 0; i < reorderPointsList.Count() - 1; i++)
            {
                Line line = Algorithms.GetLine(reorderPointsList[i], reorderPointsList[i + 1], reorderPointsList[i+1].type);
                ReferenceCharacterLinkedStrokesPanel.Children.Add(line);

                Ellipse elips = Algorithms.GetEllipse(reorderPointsList[i].type);
                Canvas.SetLeft(elips, reorderPointsList[i].X);
                Canvas.SetTop(elips, reorderPointsList[i].Y);
                ReferenceCharacterFeaturePointsPanel.Children.Add(elips);
            }

            ScorePanel.Clear();
            ScorePanel.Add(String.Format("Score:{0}, Character: {1}, Index: {2} ", score.ToString("F2"), inputCharacter[0], referenceCharacterIndex));
        }

        private void Button_SingleRadicalMatch_Click(object sender, RoutedEventArgs e)
        {
            Complete();
            string inputCharacter = Microsoft.VisualBasic.Interaction.InputBox("Input the character index here", "Input Box", "0", -1, -1);
            if (inputCharacter == "" || !System.Text.RegularExpressions.Regex.IsMatch(inputCharacter, @"^-?\d+$"))
            {
                return;
            }
            int referenceRadicalIndex = Convert.ToInt32(inputCharacter);

            ReferenceCharacterStrokesPanel.Children.Clear();
            ReferenceCharacterFeaturePointsPanel.Children.Clear();
            ReferenceCharacterLinkedStrokesPanel.Children.Clear();

            Character referenceRadical = referenceRadicals[referenceRadicalIndex];
            Character bestMatchPortion = GetBestMatchPortion(referenceRadicalIndex);

            SketchStrokes(bestMatchPortion.Content());
            SketchFeaturePoints(bestMatchPortion);

            for (int j = 0; j < referenceRadical.StrokeCount(); j++)
            {
                for (int i = 0; i < referenceRadical[j].Count() - 1; i++)
                {
                    Line line = Algorithms.GetLine(referenceRadical[j][i], referenceRadical[j][i + 1], StrokeType.Online);
                    ReferenceCharacterStrokesPanel.Children.Add(line);
                }
            }

            UnconstraintMatchProcessor unconstraintMatchProcessor = new UnconstraintMatchProcessor(bestMatchPortion);
            unconstraintMatchProcessor.ComputeMatchRisksForAllStrokes(referenceRadical, Algorithms.Infinity);
            List<Point> reorderPointsList = unconstraintMatchProcessor.GetReorderReferencePointsList(referenceRadical);
            double score = unconstraintMatchProcessor.GetMatchScoreAdvanced(referenceRadical, Algorithms.Infinity);

            for (int i = 0; i < reorderPointsList.Count() - 1; i++)
            {
                Line line = Algorithms.GetLine(reorderPointsList[i], reorderPointsList[i + 1], reorderPointsList[i + 1].type);
                ReferenceCharacterLinkedStrokesPanel.Children.Add(line);

                Ellipse elips = Algorithms.GetEllipse(reorderPointsList[i].type);
                Canvas.SetLeft(elips, reorderPointsList[i].X);
                Canvas.SetTop(elips, reorderPointsList[i].Y);
                ReferenceCharacterFeaturePointsPanel.Children.Add(elips);
            }

            ScorePanel.Clear();
            ScorePanel.Add(String.Format("Score:{0}, Radical: {1}, Index: {2} ", score.ToString("F2"), referenceRadicals[referenceRadicalIndex].Font, referenceRadicalIndex));
        }        

        private void Button_Clear_Click(object sender, RoutedEventArgs e)
        {
            Initialize();
            PaintPanel.Children.Clear();
            InputCharacterNormalizedPanel.Children.Clear();
            InputCharacterFeaturePointsPanel.Children.Clear();
            InputCharacterLinkedStrokesPanel.Children.Clear();
            ReferenceCharacterStrokesPanel.Children.Clear();
            ReferenceCharacterFeaturePointsPanel.Children.Clear();
            ReferenceCharacterLinkedStrokesPanel.Children.Clear();
        }

        private void Button_SaveTestSample_Click(object sender, RoutedEventArgs e)
        {
            SaveTestSample();
        }

        private void Button_TestSingleSample_Click(object sender, RoutedEventArgs e)
        {
            //TestAllSamples();           
            TestSingleSample();
        }

        private void Button_TestAllSample_Click(object sender, RoutedEventArgs e)
        {
            TestAllSamples();
        }

        #endregion

        #region Match Functions

        private void Complete()
        {
            List<Stroke> normalizedStrokeList = new List<Stroke>();
            Preprocessor preprocess = new Preprocessor(rawInputCharacter);
            preprocess.RemoveIsolatedPoints();
            normalizedStrokeList = preprocess.Normalize();
            featuredInputCharacter = preprocess.GetFeaturedCharacter();

            SketchStrokes(normalizedStrokeList);
            SketchFeaturePoints(featuredInputCharacter);            
        }

        private void UnconstraintMatch(out int filteredCountByStrokeCountDisparity, out int filteredCountBySizeDisparity, out int filteredCountByPrune, out string interval, out Dictionary<double, List<int>> scoreDict)
        {       
            filteredCountByPrune = 0;
            scoreDict = new Dictionary<double, List<int>>();
            double currentBestScore = Algorithms.Infinity;                    
            UnconstraintMatchProcessor unconstraintMatchProcessor = new UnconstraintMatchProcessor(featuredInputCharacter);
            DateTime startTime, endTime;
            startTime = DateTime.Now;

            Dictionary<int, List<int>> candidateReferenceCharacterDict = ConstraintFreeElementaryFiliter(out filteredCountByStrokeCountDisparity, out filteredCountBySizeDisparity);
            BinarySort(ref candidateReferenceCharacterDict);
            foreach(KeyValuePair<int, List<int>> item in candidateReferenceCharacterDict)
            {
                foreach (int index in item.Value)
                {
                    Character referenceCharacter = referenceCharacters[index];
                    double score = unconstraintMatchProcessor.GetMatchScoreAdvanced(referenceCharacter, currentBestScore);

                    if (score == 0)
                    {
                        filteredCountByPrune++;
                        continue;
                    }
                    else if (score < currentBestScore)
                    {
                        currentBestScore = score;
                    }

                    if (scoreDict.ContainsKey(score))
                    {
                        scoreDict[score].Add(index);
                    }
                    else
                    {
                        List<int> value = new List<int>();
                        value.Add(index);
                        scoreDict.Add(score, value);
                    }
                }
            }
            endTime = DateTime.Now;
            interval = Algorithms.GetTimeDifference(startTime, endTime);
            AscendingSort(ref scoreDict, RecognitionMode.Unconstraint);
        }

        private void FastMatch(out int filteredCountByStrokeCountDisparity, out int filteredCountBySizeDisparity, out int filteredCountByPrune, out string interval, out Dictionary<double, List<int>> scoreDict)
        {           
            scoreDict = new Dictionary<double, List<int>>();
            double currentBestScore = Algorithms.Infinity;     
            FastMatchProcessor fastMatchProcessor = new FastMatchProcessor(featuredInputCharacter);
            CoarseClassificationProcessor coarseClassificationProcessor = new CoarseClassificationProcessor(featuredInputCharacter);
            DateTime startTime, endTime;
            filteredCountByStrokeCountDisparity = 0;
            filteredCountBySizeDisparity = 0;
            filteredCountByPrune = 0;
            startTime = DateTime.Now;

            for (int i = 0; i < referenceCharacters.Count; i++)
            {
                if (!coarseClassificationProcessor.IsStrokeCountDifferenceTolerable(referenceCharacters[i], RecognitionMode.Fast))
                {
                    filteredCountByStrokeCountDisparity++;
                    continue;
                }

                if (!coarseClassificationProcessor.IsSizeDifferenceTolerable(referenceCharacters[i], RecognitionMode.Fast))
                {
                    filteredCountBySizeDisparity++;
                    continue;
                }

                double score = fastMatchProcessor.GetMatchScore(referenceCharacters[i], currentBestScore);

                if (score == 0)
                {
                    filteredCountByPrune++;
                    continue;
                }
                else if (score < currentBestScore)
                {
                    currentBestScore = score;
                }

                if (scoreDict.ContainsKey(score))
                {
                    scoreDict[score].Add(i);
                }
                else
                {
                    List<int> value = new List<int>();
                    value.Add(i);
                    scoreDict.Add(score, value);
                }
            }

            endTime = DateTime.Now;
            interval = Algorithms.GetTimeDifference(startTime, endTime);
            AscendingSort(ref scoreDict, RecognitionMode.Fast);
        }

        private void RadicalBasedMatch(out int filteredCountByStrokeCountDisparity, out int filteredCountBySizeDisparity, out int filteredCountByPrune, out int filteredCountByRadical, out string interval, out Dictionary<double, List<int>> scoreDict)
        {
            filteredCountByPrune = 0;
            filteredCountByRadical = 0;
            filteredCountByRadical = 0;
            scoreDict = new Dictionary<double, List<int>>();
            double currentBestScore = Algorithms.Infinity;
            UnconstraintMatchProcessor unconstraintMatchProcessor = new UnconstraintMatchProcessor(featuredInputCharacter);
            DateTime startTime, endTime;
            startTime = DateTime.Now;

            Dictionary<double, List<int>> candidateReferenceCharacterDict = RadicalBasedElementaryFiliter(out filteredCountByStrokeCountDisparity, out filteredCountBySizeDisparity, out filteredCountByRadical);
            foreach (KeyValuePair<double, List<int>> item in candidateReferenceCharacterDict)
            {
                foreach (int index in item.Value)
                {
                    Character referenceCharacter = referenceCharacters[index];
                    double score = unconstraintMatchProcessor.GetMatchScoreAdvanced(referenceCharacter, currentBestScore);

                    if (score == 0)
                    {
                        filteredCountByPrune++;
                        continue;
                    }
                    else if (score < currentBestScore)
                    {
                        currentBestScore = score;
                    }

                    if (scoreDict.ContainsKey(score))
                    {
                        scoreDict[score].Add(index);
                    }
                    else
                    {
                        List<int> value = new List<int>();
                        value.Add(index);
                        scoreDict.Add(score, value);
                    }
                }
            }
            endTime = DateTime.Now;
            interval = Algorithms.GetTimeDifference(startTime, endTime);
            AscendingSort(ref scoreDict, RecognitionMode.Unconstraint);
        }

        #endregion

        #region Support Functions

        private Dictionary<int, List<int>> ConstraintFreeElementaryFiliter(out int filteredCountByStrokeCountDisparity, out int filteredCountBySizeDisparity)
        {
            CoarseClassificationProcessor coarseClassificationProcessor = new CoarseClassificationProcessor(featuredInputCharacter);
            Dictionary<int, List<int>> candidateReferenceCharacterDict = new Dictionary<int, List<int>>();
            filteredCountByStrokeCountDisparity = 0;
            filteredCountBySizeDisparity = 0;

            for (int i = 0; i < referenceCharacters.Count; i++)
            {
                Character referenceCharacter = referenceCharacters[i];

                if (!coarseClassificationProcessor.IsStrokeCountDifferenceTolerable(referenceCharacter, RecognitionMode.Unconstraint))
                {
                    filteredCountByStrokeCountDisparity++;
                    continue;
                }

                if (!coarseClassificationProcessor.IsSizeDifferenceTolerable(referenceCharacter, RecognitionMode.Unconstraint))
                {
                    filteredCountBySizeDisparity++;
                    continue;
                }

                if (candidateReferenceCharacterDict.ContainsKey(referenceCharacter.PointsCount))
                {
                    candidateReferenceCharacterDict[referenceCharacter.PointsCount].Add(i);
                }
                else
                {
                    List<int> value = new List<int>();
                    value.Add(i);
                    candidateReferenceCharacterDict.Add(referenceCharacter.PointsCount, value);
                }
            }

            return candidateReferenceCharacterDict;
        }

        private Dictionary<double, List<int>> RadicalBasedElementaryFiliter(out int filteredCountByStrokeCountDisparity, out int filteredCountBySizeDisparity, out int filteredCountByRadical)
        {
            int cutOutPosition = 15;
            CoarseClassificationProcessor coarseClassificationProcessor = new CoarseClassificationProcessor(featuredInputCharacter);
            List<int> candidateReferenceCharacterList = new List<int>();
            filteredCountByStrokeCountDisparity = 0;
            filteredCountBySizeDisparity = 0;
            filteredCountByRadical = 0;

            for (int i = 0; i < referenceCharacters.Count; i++)
            {
                Character referenceCharacter = referenceCharacters[i];
                if (!coarseClassificationProcessor.IsStrokeCountDifferenceTolerable(referenceCharacter, RecognitionMode.Unconstraint))
                {
                    filteredCountByStrokeCountDisparity++;
                    continue;
                }

                if (!coarseClassificationProcessor.IsSizeDifferenceTolerable(referenceCharacter, RecognitionMode.Unconstraint))
                {
                    filteredCountBySizeDisparity++;
                    continue;
                }
                candidateReferenceCharacterList.Add(i);
            }

            Dictionary<double, List<int>> candidateReferenceCharacterDict = new Dictionary<double, List<int>>();
            RadicalBasedMatchProcessor radicalBasedMatchProcessor = new RadicalBasedMatchProcessor(featuredInputCharacter);
            foreach (int characterIndex in candidateReferenceCharacterList)
            {
                Character referenceCharacter = referenceCharacters[characterIndex];                
                double radicalMatchScore = radicalBasedMatchProcessor.GetRadicalMatchScore(referenceCharacter);

                if (candidateReferenceCharacterDict.ContainsKey(radicalMatchScore))
                {
                    candidateReferenceCharacterDict[radicalMatchScore].Add(characterIndex);
                }
                else
                {
                    List<int> value = new List<int>();
                    value.Add(characterIndex);
                    candidateReferenceCharacterDict.Add(radicalMatchScore, value);
                }
            }
            AscendingSort(ref candidateReferenceCharacterDict, RecognitionMode.RadicalBased);

            while (candidateReferenceCharacterDict.Count > cutOutPosition)
            {
                filteredCountByRadical += candidateReferenceCharacterDict.ElementAt(cutOutPosition).Value.Count;
                candidateReferenceCharacterDict.Remove(candidateReferenceCharacterDict.ElementAt(cutOutPosition).Key);
            }

            return candidateReferenceCharacterDict;
        }

        private void BinarySort(ref Dictionary<int, List<int>> candidateReferenceCharacterDict)
        {
            var items = from pair in candidateReferenceCharacterDict
                        orderby pair.Key ascending
                        select pair;

            candidateReferenceCharacterDict = new Dictionary<int, List<int>>();
            int middlePosition = items.Count() / 2;
            int leftSideIndex = middlePosition - 1, rightSideIndex = middlePosition + 1;
            candidateReferenceCharacterDict.Add(items.ElementAt(middlePosition).Key, items.ElementAt(middlePosition).Value);

            while (true)
            {
                if (leftSideIndex < 0 && rightSideIndex >= items.Count())
                {
                    break;
                }

                if (leftSideIndex >= 0)
                {
                    candidateReferenceCharacterDict.Add(items.ElementAt(leftSideIndex).Key, items.ElementAt(leftSideIndex).Value);
                }
                if (rightSideIndex < items.Count())
                {
                    candidateReferenceCharacterDict.Add(items.ElementAt(rightSideIndex).Key, items.ElementAt(rightSideIndex).Value);
                }

                leftSideIndex--;
                rightSideIndex++;
            }
        }

        private void AscendingSort(ref Dictionary<double, List<int>> scoreDict, RecognitionMode mode)
        {
            var sortedScores = from pair in scoreDict
                               orderby pair.Key ascending
                               select pair;

            scoreDict = new Dictionary<double, List<int>>();
            foreach (KeyValuePair<double, List<int>> item in sortedScores)
            {
                scoreDict.Add(item.Key, item.Value);
            }

            switch (mode)
            {
                case RecognitionMode.Unconstraint:
                    {
                        return;
                    }
                case RecognitionMode.Fast:
                    {
                        int inputCharacterStrokeCount = featuredInputCharacter.StrokeCount();
                        int firstExactMatchX = 0;
                        int firstExactMatchY = 0;
                        for (int i = 0; i < scoreDict.Count; i++)
                        {
                            List<int> list = scoreDict.ElementAt(i).Value;
                            for (int j = 0; j < list.Count; j++)
                            {
                                int referenceCharacterIndex = list[j];
                                if (referenceCharacters[referenceCharacterIndex].StrokeCount() == inputCharacterStrokeCount)
                                {
                                    firstExactMatchX = i;
                                    firstExactMatchY = j;

                                    if (firstExactMatchX == 0 && firstExactMatchY == 0)
                                    {
                                        return;
                                    }
                                    else
                                    {
                                        double score = scoreDict.ElementAt(firstExactMatchX).Key;
                                        scoreDict.ElementAt(firstExactMatchX).Value.RemoveAt(firstExactMatchY);
                                        scoreDict.ElementAt(0).Value.Insert(0, referenceCharacterIndex);
                                        return;
                                    }
                                }
                            }
                        }
                        break;
                    }
                case RecognitionMode.RadicalBased:
                    {
                        return;
                    }
            }
        }

        private bool IsOutsidePaintPanel(MouseEventArgs e)
        {
            System.Windows.Point clickPosition = e.GetPosition(this.PaintPanel);
            if (clickPosition.X > 400 || clickPosition.Y > 400)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private Character GetBestMatchPortion(int referenceRadicalIndex)
        {
            Character radical = referenceRadicals[referenceRadicalIndex];           
            int radicalStrokeCount = radical.StrokeCount();
            int inputCharacterStrokeCount = featuredInputCharacter.StrokeCount();
            int shiftWindowSize = radicalStrokeCount;
            double bestScore = Algorithms.Infinity;           
            Character bestMatchPortion = new Character();

            if (inputCharacterStrokeCount <= radicalStrokeCount)
            {
                return featuredInputCharacter;
            }
            
            for (int i = 0; i + shiftWindowSize - 1 < inputCharacterStrokeCount; i++)
            {
                Character characterPortion = Algorithms.GetCharacterPortion(featuredInputCharacter, i, shiftWindowSize);
                UnconstraintMatchProcessor unconstraintMatchProcessor = new UnconstraintMatchProcessor(characterPortion);
                double currentScore = unconstraintMatchProcessor.GetMatchScoreAdvanced(radical, Algorithms.Infinity);
                if (currentScore < bestScore)
                {
                    bestScore = currentScore;
                    bestMatchPortion = characterPortion;
                }
            }

            return bestMatchPortion;
        }

        private void SaveReferenceCharacter()
        {
            int fileCount = Directory.GetFiles(DatabaseDirectory + @"Characters\", "*.*", SearchOption.TopDirectoryOnly).Length;
            string index = Microsoft.VisualBasic.Interaction.InputBox("Input the character index here", "Input Box", fileCount.ToString(), -1, -1);
            if (index.Length == 0)
            {
                return;
            }
            DatabaseProcessor.SaveToCharacterDatabase(index, featuredInputCharacter, DatabaseDirectory + @"Characters\");
        }

        private void SaveTestSample()
        {
            int fileCount = Directory.GetFiles(DatabaseDirectory + @"Samples\", "*.*", SearchOption.TopDirectoryOnly).Length;
            //string index = Microsoft.VisualBasic.Interaction.InputBox("Input the character index here", "Input Box", fileCount.ToString(), -1, -1);
            /*if (index.Length == 0)
            {
                return;
            }*/
            DatabaseProcessor.SaveToCharacterDatabase(fileCount.ToString(), rawInputCharacter, DatabaseDirectory + @"Samples\");
            Button_Clear_Click(this, new RoutedEventArgs());
        }

        private void TestSingleSample()
        {
            string index = Microsoft.VisualBasic.Interaction.InputBox("Input the character index here", "Input Box", "0", -1, -1);
            if (index.Length == 0)
            {
                return;
            }
            rawInputCharacter = testSampleCharacters[Convert.ToInt32(index)];
            Button_CharacterMatch_Click(this, new RoutedEventArgs());          
        }

        private void TestAllSamples()
        {
            while (testSampleIndex != 100)
            {
                rawInputCharacter = testSampleCharacters[testSampleIndex];

                Complete();
                if (featuredInputCharacter == null)
                {
                    return;
                }

                int filteredCountByStrokeCountDisparity = 0;
                int filteredCountBySizeDisparity = 0;
                int filteredCountByPrune = 0;
                int filteredCountByRadical = 0;
                string timeInterval = null;
                Dictionary<double, List<int>> scoreDict = null;

                switch (mode)
                {
                    case RecognitionMode.Unconstraint:
                        {
                            UnconstraintMatch(out filteredCountByStrokeCountDisparity, out filteredCountBySizeDisparity, out filteredCountByPrune, out timeInterval, out scoreDict);
                            break;
                        }
                    case RecognitionMode.Fast:
                        {
                            FastMatch(out filteredCountByStrokeCountDisparity, out filteredCountBySizeDisparity, out filteredCountByPrune, out timeInterval, out scoreDict);
                            break;
                        }
                    case RecognitionMode.RadicalBased:
                        {
                            RadicalBasedMatch(out filteredCountByStrokeCountDisparity, out filteredCountBySizeDisparity, out filteredCountByPrune, out filteredCountByRadical, out timeInterval, out scoreDict);
                            break;
                        }
                }

                int rank = 1;
                bool resultFound = false;
                foreach (KeyValuePair<double, List<int>> item in scoreDict)
                {
                    List<int> scoreList = item.Value;
                    if (!resultFound)
                    {
                        foreach (int characterIndex in scoreList)
                        {
                            if (referenceCharacters[characterIndex].Font == testSampleCharacters[testSampleIndex].Font)
                            {
                                correctResultRankRecord.Add(rank);
                                resultFound = true;
                                break;
                            }
                            rank++;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                if (!resultFound)
                {
                    correctResultRankRecord.Add(-1);
                }
                timeConsumptionRecord.Add(Convert.ToDouble(timeInterval));

                testSampleIndex++;
                Initialize();
                PaintPanel.Children.Clear();
                InputCharacterNormalizedPanel.Children.Clear();
                InputCharacterFeaturePointsPanel.Children.Clear();
                InputCharacterLinkedStrokesPanel.Children.Clear();
                ReferenceCharacterStrokesPanel.Children.Clear();
                ReferenceCharacterFeaturePointsPanel.Children.Clear();
                ReferenceCharacterLinkedStrokesPanel.Children.Clear();
            }

            using (CsvFileWriter writer = new CsvFileWriter("Record.csv"))
            {
                CsvRow row;
                for (int i = 0; i < 100; i++)
                {
                    row = new CsvRow();
                    if (correctResultRankRecord[i] == -1)
                    {
                        row.Add("no");
                    }
                    else
                    {
                        row.Add(correctResultRankRecord[i].ToString());
                    }
                    row.Add(timeConsumptionRecord[i].ToString());
                    writer.WriteRow(row);
                }
            }
        }

        #endregion

        #region Sketch Functions

        private void SketchStrokes(List<Stroke> normalizedStrokeList)
        {
            InputCharacterNormalizedPanel.Children.Clear();
            InputCharacterLinkedStrokesPanel.Children.Clear();
            for (int j = 0; j < normalizedStrokeList.Count(); j++)
            {
                if (j != 0)
                {
                    Stroke offlineStroke = Algorithms.GetOffLineStroke(normalizedStrokeList[j].First(), normalizedStrokeList[j - 1].Last());
                    if (offlineStroke.Count() != 0)
                    {
                        Line line = Algorithms.GetLine(offlineStroke.First(), normalizedStrokeList[j - 1].Last(), StrokeType.Offline);
                        InputCharacterLinkedStrokesPanel.Children.Add(line);

                        for (int i = 0; i < offlineStroke.Count() - 1; i++)
                        {
                            line = Algorithms.GetLine(offlineStroke[i + 1], offlineStroke[i], StrokeType.Offline);
                            InputCharacterLinkedStrokesPanel.Children.Add(line);
                        }

                        line = Algorithms.GetLine(normalizedStrokeList[j].First(), offlineStroke.Last(), StrokeType.Offline);
                        InputCharacterLinkedStrokesPanel.Children.Add(line);
                    }
                }
                for (int i = 0; i < normalizedStrokeList[j].Count() - 1; i++)
                {
                    Line line = Algorithms.GetLine(normalizedStrokeList[j][i], normalizedStrokeList[j][i + 1], StrokeType.Online);
                    InputCharacterNormalizedPanel.Children.Add(line);

                    Line line2 = Algorithms.GetLine(normalizedStrokeList[j][i], normalizedStrokeList[j][i + 1], StrokeType.Online);
                    InputCharacterLinkedStrokesPanel.Children.Add(line2);
                }
            }
        }

        private void SketchFeaturePoints(Character character)
        {
            InputCharacterFeaturePointsPanel.Children.Clear();            
            for (int j = 0; j < character.StrokeCount(); j++)
            {
                if (j != 0)
                {
                    Stroke offlineStroke = Algorithms.GetOffLineStroke(character[j].First(), character[j - 1].Last());
                    for (int i = 0; i < offlineStroke.Count(); i++)
                    {
                        Ellipse elips = Algorithms.GetEllipse(StrokeType.Offline);
                        Canvas.SetLeft(elips, offlineStroke[i].X);
                        Canvas.SetTop(elips, offlineStroke[i].Y);
                        //InputCharacterFeaturePointsPanel.Children.Add(elips);
                    }
                }
                for (int i = 0; i < character[j].Count(); i++)
                {
                    Ellipse elips = Algorithms.GetEllipse(StrokeType.Online);
                    Canvas.SetLeft(elips, character[j][i].X);
                    Canvas.SetTop(elips, character[j][i].Y);
                    InputCharacterFeaturePointsPanel.Children.Add(elips);
                }
            }
        }

        #endregion
    }
}
