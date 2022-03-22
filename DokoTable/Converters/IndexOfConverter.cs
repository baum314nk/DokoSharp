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

public class IndexOfConverter : IMultiValueConverter
{
    public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value[0] is not IList list) return -1;

        var item = value[1];

        return list.IndexOf(item);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}