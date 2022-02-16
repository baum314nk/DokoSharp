using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Threading;
using DokoSharp.Lib;

namespace DokoTable.ViewModels;

public class DokoViewModel : IViewModel, INotifyPropertyChanged
{
    #region Fields

    private ImageLoader _imageLoader;
    private Dictionary<string, BitmapImage>? _cardImages;
    private string _infoText = "";
    private int _roundsToPlay = 4;

    #endregion

    #region Properties

    /// <summary>
    /// The loader for card images.
    /// </summary>
    public ImageLoader ImageLoader
    {
        get => _imageLoader;
        protected set
        {
            if (value == _imageLoader) return;

            _imageLoader = value;
            RaisePropertyChanged(nameof(ImageLoader));
        }
    }

    /// <summary>
    /// A mapping from card identifiers to images.
    /// </summary>
    public Dictionary<string, BitmapImage>? CardImages
    {
        get => _cardImages;
        protected set
        {
            if (value == _cardImages) return;

            _cardImages = value;
            RaisePropertyChanged(nameof(CardImages));
        }
    }

    /// <summary>
    /// The information text which is displayed above the hand cards.
    /// </summary>
    public string InformationText
    {
        get => _infoText;
        protected set
        {
            if (value == _infoText) return;

            _infoText = value;
            RaisePropertyChanged(nameof(InformationText));
        }
    }

    /// <summary>
    /// The number of rounds of Doko that should be played.
    /// </summary>
    public int RoundsToPlay
    {
        get => _roundsToPlay;
        set
        {
            if (value == _roundsToPlay) return;

            _roundsToPlay = value;
            RaisePropertyChanged(nameof(RoundsToPlay));
        }
    }

    #endregion

    #region Commands

    public ICommand LoadDefaultImageSet { get; init; }
    private async Task DoLoadDefaultImageSet()
    {
        await Task.Run(async () =>
        {
            await ImageLoader.DetectImageSetsAsync();

            var setName = ImageLoader.AvailableSets.First();
            var images = ImageLoader.LoadImages(setName);
            BeginInvoke(() => CardImages = new(images));
        });
    }

    public ICommand LoadImageSet { get; init; }
    private async Task DoLoadImageSet()
    {
        await Task.Run(() =>
        {
            var images = new Dictionary<string, BitmapImage>(ImageLoader.LoadImages(ImageLoader.AvailableSets.First()));
            BeginInvoke(() => CardImages = images);
        });
    }

    public ICommand DetectImageSets { get; init; }
    private async Task DoDetectImageSets()
    {
        await Task.Run(ImageLoader.DetectImageSetsAsync);
    }

    #endregion

    public DokoViewModel()
    {
        _dispatcher = Dispatcher.CurrentDispatcher;

        // Load default image set
        _imageLoader = new();

        LoadDefaultImageSet = new AsyncCommand(DoLoadDefaultImageSet);
        LoadImageSet = new AsyncCommand(DoLoadImageSet, () => ImageLoader.AvailableSets.Count > 0);
        DetectImageSets = new AsyncCommand(DoDetectImageSets);
    }

    #region Overrides & Implementations

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void RaisePropertyChanged(string property)
    {
        Debug.WriteLine($"Property {property} of DokoViewModel changed.");
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
    }

    private readonly Dispatcher _dispatcher;
    public bool IsSynchronized => throw new NotImplementedException();

    public void Invoke(Action action)
    {
        this._dispatcher.Invoke(action);
    }

    public void BeginInvoke(Action action)
    {
        _dispatcher.BeginInvoke(action);
    }

    #endregion
}
