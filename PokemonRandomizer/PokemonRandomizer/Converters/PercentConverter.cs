using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using PokemonRandomizer.CultureUtils;
using System.Globalization;

namespace PokemonRandomizer.Converters
{
    public class PercentConverter : IValueConverter
    {
        public static string ToPercentString(double value)
        {
            return string.Format("{0:P1}", value);
        }

        public static double FromPercentString(string value)
        {
            return double.Parse(value.RemovePercent()) / 100;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                float fltValue = System.Convert.ToSingle(value);
                return ToPercentString(fltValue);
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var str = (value as string).RemovePercent();
            try
            {
                return float.Parse(str) / 100;
            }
            catch
            {
                bool onePeriod = false;
                bool Filter(char c)
                {
                    if(c == '.' && !onePeriod)
                    {
                        onePeriod = true;
                        return true;
                    }
                    return char.IsDigit(c);
                }
                str = new string(str.Where(Filter).ToArray());
                try
                {
                    return float.Parse(str) / 100;
                }
                catch
                {
                    return float.NaN;
                }
            }
        }
    }
}
