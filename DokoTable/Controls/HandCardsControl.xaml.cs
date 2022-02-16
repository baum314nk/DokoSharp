using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DokoTable.Controls
{
    /// <summary>
    /// Interaction logic for HandCardsControl.xaml
    /// </summary>
    public partial class HandCardsControl : UserControl
    {
        public IEnumerable<BitmapImage>? HandCards
        {
            get => (IEnumerable<BitmapImage>?)GetValue(HandCardsProperty);
            set => SetValue(HandCardsProperty, value);
        }
        public static readonly DependencyProperty HandCardsProperty =
            DependencyProperty.Register(
                "HandCards",
                typeof(IEnumerable<BitmapImage>),
                typeof(HandCardsControl),
                new PropertyMetadata(null)
            );

        public HandCardsControl()
        {
            InitializeComponent();
        }
    }
}
