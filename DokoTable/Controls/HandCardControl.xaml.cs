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

public class FittingPanel : Panel
{
    private UIElement? _hoveredChild;

    protected UIElement? HoveredChild
    {
        get => _hoveredChild;
        set
        {
            if (_hoveredChild == value) return;

            const string test = "null";
            Trace.WriteLine($"Hovered Child is {value?.ToString() ?? test}");
            _hoveredChild = value;
            InvalidateArrange();
        }
    }

    protected override void OnVisualChildrenChanged(DependencyObject? visualAdded, DependencyObject? visualRemoved)
    {
        if (visualAdded != null)
        {
            var visual = (UIElement)visualAdded;

            visual.MouseEnter += Child_MouseEnter;
        }
        if (visualRemoved != null)
        {
            var visual = (UIElement)visualRemoved;

            visual.MouseEnter -= Child_MouseEnter;
        }
    }

    private void Child_MouseEnter(object sender, MouseEventArgs e)
    {
        HoveredChild = (UIElement?)sender;
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        HoveredChild = null;

        base.OnMouseLeave(e);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        Size size = new Size(0, 0);

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

        if (HoveredChild != null)
        {
            return ArrangeHovered(finalSize);
        }
        else
        {
            return ArrangeNoHovered(finalSize);
        }
    }

    protected Size ArrangeHovered(Size finalSize)
    {
        int hoveredIndex = Children.IndexOf(HoveredChild);

        double childWidth = Children[0].DesiredSize.Width;
        double childHeight = Children[0].DesiredSize.Height;

        double availableWidth = finalSize.Width - childWidth;
        double offsetPerElement = availableWidth / (Children.Count - 1);

        for (int i = 0; i < hoveredIndex; i++)
        {
            UIElement child = Children[i];
            child.Arrange(new Rect(offsetPerElement * i, 0, childWidth, childHeight));
            SetZIndex(child, i);
        }

        // Arrange hovered child
        HoveredChild!.Arrange(new Rect(offsetPerElement * hoveredIndex, 0, childWidth, childHeight));
        SetZIndex(HoveredChild, hoveredIndex);

        // Arrange children behind hovered child
        int idxOffset = hoveredIndex + 1;

        if (idxOffset == Children.Count - 1)
        {
            UIElement child = Children[idxOffset];
            child.Arrange(new Rect(offsetPerElement * idxOffset, 0, childWidth, childHeight));
            SetZIndex(child, -1);
        }
        else
        {
            double startWidth = offsetPerElement * hoveredIndex + childWidth;
            availableWidth = finalSize.Width - startWidth - childWidth;
            offsetPerElement = availableWidth / (Children.Count - idxOffset - 1);

            for (int i = idxOffset; i < Children.Count; i++)
            {
                UIElement child = Children[i];
                child.Arrange(new Rect(startWidth + offsetPerElement * (i - idxOffset), 0, childWidth, childHeight));
                SetZIndex(child, i);
            }
        }

        return finalSize;
    }

    protected Size ArrangeNoHovered(Size finalSize)
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

}

/// <summary>
/// Interaction logic for HandCardsControl.xaml
/// </summary>
public partial class HandCardControl : UserControl
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
            typeof(HandCardControl),
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
            typeof(HandCardControl),
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
            typeof(HandCardControl),
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
            typeof(HandCardControl),
            new PropertyMetadata(null)
        );

    public HandCardControl()
    {
        InitializeComponent();
    }
}
