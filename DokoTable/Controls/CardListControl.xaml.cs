using DokoSharp.Lib;
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
    /// Interaction logic for CardListControl.xaml
    /// </summary>
    public partial class CardListControl : UserControl
    {
        public IDictionary<CardBase, BitmapImage>? CardImages
        {
            get => (IDictionary<CardBase, BitmapImage>?)GetValue(CardImagesProperty);
            set => SetValue(CardImagesProperty, value);
        }
        public static readonly DependencyProperty CardImagesProperty =
            DependencyProperty.Register(
                "CardImages",
                typeof(IDictionary<CardBase, BitmapImage>),
                typeof(CardListControl),
                new PropertyMetadata(null)
            );

        public IEnumerable<CardBase>? Cards
        {
            get => (IEnumerable<CardBase>?)GetValue(CardsProperty);
            set => SetValue(CardsProperty, value);
        }
        public static readonly DependencyProperty CardsProperty =
            DependencyProperty.Register(
                "Cards",
                typeof(IEnumerable<CardBase>),
                typeof(CardListControl),
                new PropertyMetadata(null)
            );

        public CardListControl()
        {
            InitializeComponent();
        }
    }
}
