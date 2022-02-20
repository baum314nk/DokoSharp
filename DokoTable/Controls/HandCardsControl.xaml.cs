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
    /// Interaction logic for HandCardsControl.xaml
    /// </summary>
    public partial class HandCardsControl : UserControl
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
                typeof(HandCardsControl),
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
                typeof(HandCardsControl),
                new PropertyMetadata(null)
            );

        public IEnumerable<CardBase>? SelectableCards
        {
            get => (IEnumerable<CardBase>?)GetValue(SelectableCardsProperty);
            set => SetValue(SelectableCardsProperty, value);
        }
        public static readonly DependencyProperty SelectableCardsProperty =
            DependencyProperty.Register(
                "SelectableCards",
                typeof(IEnumerable<CardBase>),
                typeof(HandCardsControl),
                new PropertyMetadata(null)
            );

        public CardBase? SelectedCard
        {
            get => (CardBase?)GetValue(SelectedCardProperty);
            set => SetValue(SelectedCardProperty, value);
        }
        public static readonly DependencyProperty SelectedCardProperty =
            DependencyProperty.Register(
                "SelectedCard",
                typeof(CardBase),
                typeof(HandCardsControl),
                new PropertyMetadata(null)
            );

        public HandCardsControl()
        {
            InitializeComponent();
        }
    }
}
