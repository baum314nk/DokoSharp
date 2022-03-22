using DokoSharp.Lib;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

public class PlacedCardsPanel : Panel
{
    /// <summary>
    /// The angles of the children at the corresponding indices.
    /// </summary>
    private static readonly double[] _angles = new double[] { 0, 90, 180, 270 };
    /// <summary>
    /// The local angles of the children.
    /// </summary>
    private static readonly double[] _localAngles = new[] { 10, -4.5, -3.9, 7.3 };

    /// <summary>
    /// Factor for the image size.
    /// Is smaller than 1 if the images need to be resized in order to fit 
    /// the available space, otherwise 1.
    /// </summary>
    private double _imageScale;

    #region Dependecy Property

    /// <summary>
    /// The index of the angle to use for the first child.
    /// </summary>
    public int FirstAngleIndex
    {
        get => (int)GetValue(FirstAngleIndexProperty);
        set => SetValue(FirstAngleIndexProperty, value);
    }
    public static readonly DependencyProperty FirstAngleIndexProperty =
        DependencyProperty.Register(
            "FirstAngleIndex",
            typeof(int),
            typeof(PlacedCardsPanel),
            new PropertyMetadata(null)
        );

    /// <summary>
    /// The size of the images.
    /// </summary>
    public Size ImageSize
    {
        get => (Size)GetValue(ImageSizeProperty);
        set => SetValue(ImageSizeProperty, value);
    }
    public static readonly DependencyProperty ImageSizeProperty =
        DependencyProperty.Register(
            "ImageSize",
            typeof(Size),
            typeof(PlacedCardsPanel),
            new PropertyMetadata(null)
        );

    /// <summary>
    /// The offset of the images from the center.
    /// </summary>
    public double OffsetFactor
    {
        get => (double)GetValue(OffsetFactorProperty);
        set => SetValue(OffsetFactorProperty, value);
    }
    public static readonly DependencyProperty OffsetFactorProperty =
        DependencyProperty.Register(
            "OffsetFactor",
            typeof(double),
            typeof(PlacedCardsPanel),
            new PropertyMetadata(null)
        );

    #endregion

    protected override Size MeasureOverride(Size availableSize)
    {
        double length = (1 + 2 * OffsetFactor) * ImageSize.Height;
        double minAvailable = Math.Min(availableSize.Width, availableSize.Height);

        _imageScale = minAvailable / length;
        return new(minAvailable, minAvailable);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (Children.Count == 0) return finalSize;

        // Center of the panel
        Point center = new(finalSize.Width / 2, finalSize.Height / 2);
        // Scaled image size
        Size imgSize = new(_imageScale * ImageSize.Width, _imageScale * ImageSize.Height);

        for (int i = 0; i < Children.Count; i++)
        {
            UIElement child = Children[i];

            // Determine top left point
            Point topLeft = center - ((Vector)imgSize / 2) + new Vector(0, OffsetFactor * imgSize.Height);

            // Arrange
            child.Arrange(new Rect(topLeft, imgSize));

            // Rotate
            int angleIdx = (FirstAngleIndex + i) % 4;
            child.RenderTransform = new TransformGroup()
            {
                Children = new(new[]
                {
                    // Rotate around center of panel
                    new RotateTransform(_angles[angleIdx], 0.5*imgSize.Width, (0.5 - OffsetFactor) * imgSize.Height),
                    // Small rotation around center of card
                    new RotateTransform(_localAngles[angleIdx], 0.5*imgSize.Width, 0.5*imgSize.Height)
                })
            };
        }

        return finalSize;
    }
}

/// <summary>
/// Interaction logic for PlacedCardsControl.xaml
/// </summary>
public partial class PlacedCardsControl : UserControl, INotifyPropertyChanged
{
    public Size ImageSize
    {
        get
        {
            var img = CardImages?.Values.First();
            if (img is null) return default;

            return new Size(img.Width, img.Height);
        }
    }

    #region Dependency Properties

    public IDictionary<CardBase, BitmapImage>? CardImages
    {
        get => (IDictionary<CardBase, BitmapImage>?)GetValue(CardImagesProperty);
        set => SetValue(CardImagesProperty, value);
    }
    public static readonly DependencyProperty CardImagesProperty =
        DependencyProperty.Register(
            "CardImages",
            typeof(IDictionary<CardBase, BitmapImage>),
            typeof(PlacedCardsControl),
            new PropertyMetadata(OnCardImagesChangedCallback)
        );
    private static void OnCardImagesChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is PlacedCardsControl c)
        {
            c.PropertyChanged?.Invoke(c, new(nameof(ImageSize)));
        }
    }

    /// <summary>
    /// The offset of the images from the center.
    /// </summary>
    public double OffsetFactor
    {
        get => (double)GetValue(OffsetFactorProperty);
        set => SetValue(OffsetFactorProperty, value);
    }
    public static readonly DependencyProperty OffsetFactorProperty =
        DependencyProperty.Register(
            "OffsetFactor",
            typeof(double),
            typeof(PlacedCardsControl),
            new PropertyMetadata(0.25)
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
            typeof(PlacedCardsControl),
            new PropertyMetadata(null)
        );

    public IList<string>? PlayerOrder
    {
        get => (IList<string>?)GetValue(PlayerOrderProperty);
        set => SetValue(PlayerOrderProperty, value);
    }
    public static readonly DependencyProperty PlayerOrderProperty =
        DependencyProperty.Register(
            "PlayerOrder",
            typeof(IList<string>),
            typeof(PlacedCardsControl),
            new PropertyMetadata(null)
        );

    public string? PlayerStartingTrick
    {
        get => (string?)GetValue(PlayerStartingTrickProperty);
        set => SetValue(PlayerStartingTrickProperty, value);
    }
    public static readonly DependencyProperty PlayerStartingTrickProperty =
        DependencyProperty.Register(
            "PlayerStartingTrick",
            typeof(string),
            typeof(PlacedCardsControl),
            new PropertyMetadata(null)
        );

    #endregion

    public PlacedCardsControl()
    {
        InitializeComponent();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}

