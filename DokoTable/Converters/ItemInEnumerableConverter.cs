using System;
using System.Collections.Generic;
using System.Globalization;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using DokoSharp.Lib;
using System.Windows.Media.Imaging;

namespace DokoTable.Converters;

public class ItemInEnumerableConverter : IMultiValueConverter
{
    public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value[0] == null) return false;

        var enumerable = (IEnumerable)value[0];
        var mainItem = value[1];

        foreach (var item in enumerable)
        {
            if (item == mainItem) return true;
        }

        return false;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
