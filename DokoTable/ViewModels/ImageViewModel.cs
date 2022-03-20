using DokoSharp.Lib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Serilog;
using DokoTable.ViewModels.Commands;

namespace DokoTable.ViewModels;

/// <summary>
/// A viewmodel that handles the loading of various images.
/// </summary>
public class ImageViewModel : BaseViewModel
{
    #region Fields

    private readonly ImageLoader _imageLoader;

    private string? _cardImageSetName;
    private IReadOnlyDictionary<CardBase, BitmapImage>? _cardImageSet;

    #endregion

    #region Properties

    /// <summary>
    /// The names of the available image sets.
    /// </summary>
    public ICollection<string> AvailableImageSets => _imageLoader.AvailableSets;

    /// <summary>
    /// The name of the currently loaded image set.
    /// </summary>
    public string? CardImageSetName
    {
        get => _cardImageSetName;
        set
        {
            if (_cardImageSetName == value) return;

            _cardImageSetName = value;
            RaisePropertyChanged(nameof(CardImageSetName));
        }
    }

    /// <summary>
    /// The currently loaded image set.
    /// </summary>
    public IReadOnlyDictionary<CardBase, BitmapImage>? CardImageSet
    {
        get => _cardImageSet;
        set
        {
            if (_cardImageSet == value) return;

            _cardImageSet = value;
            RaisePropertyChanged(nameof(CardImageSet));
        }
    }

    #endregion

    #region Commands

    public ICommand LoadDefaultImageSetCommand { get; init; }
    private async Task DoLoadDefaultImageSet()
    {
        await DoDetectImageSets();
        var setName = "Deutsche Kriegsspielkarten"; //_imageLoader.AvailableSets.First();
        await DoLoadImageSet(setName);
    }

    public ICommand LoadImageSetCommand { get; init; }
    private async Task DoLoadImageSet(string setName)
    {
        var rawImages = await Task.Run(() => _imageLoader.LoadImagesAsync(setName));

        var images = new Dictionary<CardBase, BitmapImage>();
        await foreach (var kv in rawImages)
        {
            var rawImg = kv.Value;

            // Create BitmapImages from bytes
            // Needs to be done on UI thread
            var img = new BitmapImage();
            using (var ms = new MemoryStream(rawImg))
            {
                img.BeginInit();
                img.CacheOption = BitmapCacheOption.OnLoad;
                img.StreamSource = ms;
                img.EndInit();
            }

            images[kv.Key] = img;
        }

        CardImageSet = images;
        CardImageSetName = setName;
        Log.Information("Loaded image set \"{setName}\"", setName);
    }

    public ICommand DetectImageSetsCommand { get; init; }
    private async Task DoDetectImageSets()
    {
        await Task.Run(_imageLoader.DetectImageSetsAsync);
        RaisePropertyChanged(nameof(AvailableImageSets));

        Log.Information("Detected image sets: {set}", _imageLoader.AvailableSets);
    }



    #endregion

    #region Constructor

    public ImageViewModel(Dispatcher dispatcher) : base(dispatcher)
    {
        _imageLoader = new();

        LoadDefaultImageSetCommand = new AsyncCommand(DoLoadDefaultImageSet);
        LoadImageSetCommand = new ParamCommand<string>(
            async (setName) => await DoLoadImageSet(setName!),
            (setName) => AvailableImageSets.Contains(setName ?? string.Empty)
        );
        DetectImageSetsCommand = new AsyncCommand(DoDetectImageSets);
    }

    #endregion
}
