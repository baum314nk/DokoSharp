using DokoSharp.Lib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace DokoTable.Controls;

public class HandCardsPanel : Panel
{
    protected override Size MeasureOverride(Size availableSize)
    {
        Size size = default;
        foreach (UIElement child in Children)
        {
            child.Measure(availableSize);
            size.Width = Math.Max(size.Width, child.DesiredSize.Width);
            size.Height = Math.Max(size.Height, child.DesiredSize.Height);
        }

        size.Width = double.IsPositiveInfinity(availableSize.Width) ? size.Width : availableSize.Width;
        size.Height = double.IsPositiveInfinity(availableSize.Height) ? size.Height : availableSize.Height;

        return size;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (Children.Count == 0) return finalSize;

        if (Children[0].DesiredSize.Width * Children.Count < finalSize.Width)
        {
            return ArrangeSideBySide(finalSize);
        }
        else
        {
            return ArrangeOverlapped(finalSize);
        }
    }

    protected Size ArrangeOverlapped(Size finalSize)
    {
        double availableWidth = finalSize.Width - Children[0].DesiredSize.Width;
        double offsetPerElement = availableWidth / (Children.Count - 1);

        for (int i = 0; i < Children.Count; i++)
        {
            UIElement child = Children[i];
            child.Arrange(new Rect(offsetPerElement * i, 0, child.DesiredSize.Width, child.DesiredSize.Height));
        }

        return finalSize;
    }

    protected Size ArrangeSideBySide(Size finalSize)
    {
        double offsetPerElement = Children[0].DesiredSize.Width;

        for (int i = 0; i < Children.Count; i++)
        {
            UIElement child = Children[i];
            child.Arrange(new Rect(offsetPerElement * i, 0, child.DesiredSize.Width, child.DesiredSize.Height));
        }

        return finalSize;
    }
}

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
