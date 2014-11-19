using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace ChineseCharacterRecognitionSystem
{
    [ValueConversion(typeof(TestSampleBatch), typeof(String))]
    public class SampleBatchConveter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            TestSampleBatch type = (TestSampleBatch)value;
            switch (type)
            {
                case TestSampleBatch.TidyBatch:
                    return "Tidy Batch";
                case TestSampleBatch.CursiveBatch:
                    return "Cursive Batch";               
                case TestSampleBatch.LigatureBatch:
                    return "Ligature Batch";
                default:
                    return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            string strType = value as string;
            switch (strType)
            {
                case "Tidy Batch":
                    return TestSampleBatch.TidyBatch;
                case "Cursive Batch":
                    return TestSampleBatch.CursiveBatch;
                case "Ligature Batch":
                    return TestSampleBatch.LigatureBatch;
                default:
                    return DependencyProperty.UnsetValue;
            }
        }
    }
}
