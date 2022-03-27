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

public class LessThanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not IComparable comparable) return false;

        // Returns true if value < parameter
        return comparable.CompareTo(parameter) < 0;
    }

    public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}