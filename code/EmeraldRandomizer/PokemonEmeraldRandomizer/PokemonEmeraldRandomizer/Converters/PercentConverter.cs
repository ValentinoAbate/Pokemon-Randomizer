using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using PokemonEmeraldRandomizer.CultureUtils;

namespace PokemonEmeraldRandomizer.Converters
{
    public class PercentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                float fltValue = System.Convert.ToSingle(value);
                return string.Format("{0:P1}", fltValue);
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return float.Parse((value as string).RemovePercent()) / 100;
        }
    }
}
