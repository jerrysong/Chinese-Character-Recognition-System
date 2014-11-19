using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ChineseCharacterRecognitionSystem
{
    public static class Algorithms
    {
        public const int Infinity = 10000;

        public static double EuclidDistance(Point p1, Point p2)
        {
            double distance = Math.Pow(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2), 0.5);
            return distance;
        }

        public static int GetOffLinePointsCount(Point currentPoint, Point previousPoint)
        {
            double pixelPitch = 12;
            double width = currentPoint.X - previousPoint.X;
            double height = currentPoint.Y - previousPoint.Y;
            double distance = EuclidDistance(currentPoint, previousPoint);
            if (distance < 1.5 * pixelPitch)
            {
                return 0;
            }

            int interpolationPointsCount = (int)(distance / pixelPitch);
            return interpolationPointsCount;
        }

        public static Stroke GetOffLineStroke(Point currentPoint, Point previousPoint)
        {
            Stroke offLineStroke = new Stroke(StrokeType.Offline);
            double pixelPitch = 12;
            double width = currentPoint.X - previousPoint.X;
            double height = currentPoint.Y - previousPoint.Y;
            double distance = EuclidDistance(currentPoint, previousPoint);
            if (distance < 1.5 * pixelPitch)
            {
                return offLineStroke;
            }

            int interpolationPointsCount = (int)(distance / pixelPitch);
            double widthVariation = width / (double)interpolationPointsCount;
            double heightVariation = height / (double)interpolationPointsCount;

            for (int i = 1; i <= interpolationPointsCount; i++)
            {
                int x = previousPoint.X + (int)(i * widthVariation);
                int y = previousPoint.Y + (int)(i * heightVariation);

                offLineStroke.Add(new Point(x, y, StrokeType.Offline));
            }

            return offLineStroke;
        }

        public static double GetRadians(Point previousPoint, Point currentPoint)
        {
            double x = currentPoint.X - previousPoint.X;
            double y = currentPoint.Y - previousPoint.Y;
            return Math.Atan2(y, x);
        }

        public static Line GetLine(Point currentPoint, Point nextPoint, StrokeType strokeType)
        {
            Line line = new Line();
            if (strokeType == StrokeType.Online)
            {
                line.Stroke = SystemColors.WindowFrameBrush;
            }
            else if (strokeType == StrokeType.Offline)
            {
                line.Stroke = SystemColors.HighlightBrush;
            }
            line.X1 = currentPoint.X;
            line.Y1 = currentPoint.Y;
            line.X2 = nextPoint.X;
            line.Y2 = nextPoint.Y;
            return line;
        }

        public static Ellipse GetEllipse(StrokeType strokeType)
        {
            Ellipse elips = new Ellipse();
            if (strokeType == StrokeType.Online)
            {
                elips.Stroke = SystemColors.WindowTextBrush;
            }
            else if (strokeType == StrokeType.Offline)
            {
                elips.Stroke = SystemColors.HighlightBrush;
            }
            elips.Width = 5;
            elips.Height = 5;
            elips.Fill = new SolidColorBrush(Colors.Black);
            return elips;
        }

        public static string GetTimeDifference(DateTime dateBegin, DateTime dateEnd)
        {
            TimeSpan ts1 = new TimeSpan(dateBegin.Ticks);
            TimeSpan ts2 = new TimeSpan(dateEnd.Ticks);
            TimeSpan ts3 = ts1.Subtract(ts2).Duration();
            return ts3.TotalMilliseconds.ToString();
        }

        public static dynamic SortDictionaryDescendingByKey<T>(Dictionary<int, T> dictionary)
        {
            var sortedDict = from entry in dictionary orderby entry.Key descending select entry;
            return sortedDict;
        }

        public static dynamic SortDictionaryAscendingByKey<T>(Dictionary<int, T> dictionary)
        {
            var sortedDict = from entry in dictionary orderby entry.Key ascending select entry;
            return sortedDict;
        }

        public static dynamic SortDictionaryAscendingByValue<T>(Dictionary<T, int> dictionary)
        {
            var sortedDict = from entry in dictionary orderby entry.Value ascending select entry;
            return sortedDict;
        }

        public static int RadicalToIndex(Radical radical)
        {
            switch (radical)
            {
                case Radical.口:
                    return 0;
                case Radical.亻:
                    return 1;
                case Radical.氵:
                    return 2;
                case Radical.扌:
                    return 3;
                case Radical.女:
                    return 4;
                case Radical.木:
                    return 5;
                case Radical.土:
                    return 6;
                case Radical.日:
                    return 7;
                case Radical.亠:
                    return 8;
                case Radical.刂:
                    return 9;
                case Radical.忄:
                    return 10;
                case Radical.宀:
                    return 11;
                case Radical.人:
                    return 12;
                case Radical.十:
                    return 13;
                case Radical.山:
                    return 14;
                case Radical.力:
                    return 15;
                case Radical.月:
                    return 16;
                case Radical.尸:
                    return 17;
                case Radical.巾:
                    return 18;
                case Radical.心:
                    return 19;
                case Radical.阝:
                    return 20;
                case Radical.子:
                    return 21;
                case Radical.儿:
                    return 22;
                case Radical.又:
                    return 23;
                case Radical.广:
                    return 24;
                case Radical.勹:
                    return 25;
                case Radical.大:
                    return 26;
                case Radical.彳:
                    return 27;
                case Radical.弓:
                    return 28;
                case Radical.火:
                    return 29;
                case Radical.攵:
                    return 30;
                case Radical.辶:
                    return 31;
                case Radical.卩:
                    return 32;
                case Radical.王:
                    return 33;
                case Radical.艹:
                    return 34;
                case Radical.禾:
                    return 35;
                case Radical.水:
                    return 36;
                case Radical.工:
                    return 37;
                default:
                    return -1;
            }
        }

        public static Radical IndexToRadical(int index)
        {
            switch (index)
            {
                case 0:
                    return Radical.口;
                case 1:
                    return Radical.亻;
                case 2:
                    return Radical.氵;
                case 3:
                    return Radical.扌;
                case 4:
                    return Radical.女;
                case 5:
                    return Radical.木;
                case 6:
                    return Radical.土;
                case 7:
                    return Radical.日;
                case 8:
                    return Radical.亠;
                case 9:
                    return Radical.刂;
                case 10:
                    return Radical.忄;
                case 11:
                    return Radical.宀;
                case 12:
                    return Radical.人;
                case 13:
                    return Radical.十;
                case 14:
                    return Radical.山;
                case 15:
                    return Radical.力;
                case 16:
                    return Radical.月;
                case 17:
                    return Radical.尸;
                case 18:
                    return Radical.巾;
                case 19:
                    return Radical.心;
                case 20:
                    return Radical.阝;
                case 21:
                    return Radical.子;
                case 22:
                    return Radical.儿;
                case 23:
                    return Radical.又;
                case 24:
                    return Radical.广;
                case 25:
                    return Radical.勹;
                case 26:
                    return Radical.大;
                case 27:
                    return Radical.彳;
                case 28:
                    return Radical.弓;
                case 29:
                    return Radical.火;
                case 30:
                    return Radical.攵;
                case 31:
                    return Radical.辶;
                case 32:
                    return Radical.卩;
                case 33:
                    return Radical.王;
                case 34:
                    return Radical.艹;
                case 35:
                    return Radical.禾;
                case 36:
                    return Radical.水;
                case 37:
                    return Radical.工;
                default:
                    return Radical.None;
            }
        }

        public static Radical CharToRadical(char radical)
        {
            switch (radical)
            {
                case '口':
                    return Radical.口;
                case '亻':
                    return Radical.亻;
                case '氵':
                    return Radical.氵;
                case '扌':
                    return Radical.扌;
                case '女':
                    return Radical.女;
                case '木':
                    return Radical.木;
                case '土':
                    return Radical.土;
                case '日':
                    return Radical.日;
                case '亠':
                    return Radical.亠;
                case '刂':
                    return Radical.刂;
                case '忄':
                    return Radical.忄;
                case '宀':
                    return Radical.宀;
                case '人':
                    return Radical.人;
                case '十':
                    return Radical.十;
                case '山':
                    return Radical.山;
                case '力':
                    return Radical.力;
                case '月':
                    return Radical.月;
                case '尸':
                    return Radical.尸;
                case '巾':
                    return Radical.巾;
                case '心':
                    return Radical.心;
                case '阝':
                    return Radical.阝;
                case '子':
                    return Radical.子;
                case '儿':
                    return Radical.儿;
                case '又':
                    return Radical.又;
                case '广':
                    return Radical.广;
                case '勹':
                    return Radical.勹;
                case '大':
                    return Radical.大;
                case '彳':
                    return Radical.彳;
                case '弓':
                    return Radical.弓;
                case '火':
                    return Radical.火;
                case '攵':
                    return Radical.攵;
                case '辶':
                    return Radical.辶;
                case '卩':
                    return Radical.卩;
                case '王':
                    return Radical.王;
                case '艹':
                    return Radical.艹;
                case '禾':
                    return Radical.禾;
                case '水':
                    return Radical.水;
                case '工':
                    return Radical.工;
                default:
                    return Radical.None;
            }
        }

        public static Character GetCharacterPortion(Character inputCharacter, int start, int size)
        {
            Character characterPortion = new Character();
            for (int i = 0; i < size; i++)
            {
                Stroke addedStroke = Stroke.Clone(inputCharacter[start + i]);
                characterPortion.Add(addedStroke);
            }

            Preprocessor preprocessor = new Preprocessor(characterPortion);
            //preprocessor.RemoveIsolatedPoints();
            preprocessor.Normalize();
            Character normalizedCharacterPortion = preprocessor.GetFeaturedCharacter();

            return normalizedCharacterPortion;
        }
    }
}
