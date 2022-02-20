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

namespace DokoTable.Controls;

public class ImageOfCardConverter : IMultiValueConverter
{
    public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
    {
        var cardImages = (IDictionary<CardBase, BitmapImage>)value[0];
        var card = (CardBase)value[1];

        return cardImages[card];
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}