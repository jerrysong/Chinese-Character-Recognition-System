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
    [ValueConversion(typeof(RecognitionMode), typeof(String))]
    public class ModeConveter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            RecognitionMode type = (RecognitionMode)value;
            switch (type)
            {
                case RecognitionMode.Unconstraint:
                    return "Unconstraint";
                case RecognitionMode.Fast:
                    return "Fast";
                case RecognitionMode.RadicalBased:
                    return "Radical Based";
                default:
                    return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            string strType = value as string;
            switch (strType)
            {
                case "Unconstraint":
                    return RecognitionMode.Unconstraint;
                case "Fast":
                    return RecognitionMode.Fast;
                case "Radical Based":
                    return RecognitionMode.RadicalBased;
                default:
                    return DependencyProperty.UnsetValue;
            }
        }
    }
}
