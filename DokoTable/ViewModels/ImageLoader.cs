using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using System.Diagnostics;
using Serilog;
using System.Windows.Media.Imaging;
using System.Text.RegularExpressions;
using System.Reflection;
using DokoSharp.Lib;

namespace DokoTable.ViewModels;

/// <summary>
/// Contains information about an image set.
/// </summary>
public class ImageSetConfig
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("pattern")]
    public string? Pattern { get; set; }

    [JsonPropertyName("extension")]
    public string? Extension { get; set; }

    [JsonPropertyName("symbol_names")]
    public Dictionary<string, string>? SymbolMap { get; set; }

    [JsonPropertyName("color_names")]
    public Dictionary<string, string>? ColorMap { get; set; }

    [JsonIgnore]
    public string? Path { get; set; }
}

/// <summary>
/// Provides methods to load card images.
/// </summary>
public class ImageLoader
{
    protected static readonly string BASE_PATH = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/Assets/Cards/";

    #region Fields

    protected Dictionary<string, ImageSetConfig> _imageSetConfigs;

    #endregion

    #region Properties

    public ICollection<string> AvailableSets => _imageSetConfigs.Keys;

    #endregion

    /// <summary>
    /// Creates a new card image loader instance.
    /// </summary>
    public ImageLoader()
    {
        _imageSetConfigs = new();
    }

    /// <summary>
    /// Detects valid card sets in the Assets/Cards folder. 
    /// </summary>
    public async Task DetectImageSetsAsync()
    {
        List<ImageSetConfig> result = new();

        foreach (var dir in Directory.GetDirectories(BASE_PATH))
        {
            string configPath = $"{dir}/config.json";
            if (!File.Exists(configPath)) continue;

            using FileStream fs = File.OpenRead(configPath);
            try
            {
                ImageSetConfig config = (await JsonSerializer.DeserializeAsync<ImageSetConfig>(fs))!;
                config.Path = dir;
                result.Add(config!);

                Debug.WriteLine($"Detected card set \"{config.Name}\".");

            }
            catch (JsonException e)
            {
                Debug.WriteLine("JSON exception while trying to parse config file {}", e.Message);
            }
        }

        // Clear old configs and add new ones
        _imageSetConfigs.Clear();
        result.ForEach(c => _imageSetConfigs[c.Name] = c);
    }

    /// <summary>
    /// Detects valid card sets in the Assets/Cards folder. 
    /// </summary>
    public void DetectImageSets()
    {
        List<ImageSetConfig> result = new();

        foreach (var dir in Directory.GetDirectories(BASE_PATH))
        {
            string configPath = $"{dir}/config.json";
            if (!File.Exists(configPath)) continue;

            using FileStream fs = File.OpenRead(configPath);
            try
            {
                ImageSetConfig config = JsonSerializer.Deserialize<ImageSetConfig>(fs)!;
                config.Path = dir;
                result.Add(config!);

                Debug.WriteLine($"Detected card set \"{config.Name}\".");

            }
            catch (JsonException e)
            {
                Debug.WriteLine("JSON exception while trying to parse config file {}", e.Message);
            }
        }

        // Clear old configs and add new ones
        _imageSetConfigs.Clear();
        result.ForEach(c => _imageSetConfigs[c.Name!] = c);
    }

    /// <summary>
    /// Returns key-value-pairs of card identifiers of the form "{COLOR}{SYMBOL}" mapped to bitmap images. 
    /// </summary>
    /// <param name="setName"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public IEnumerable<KeyValuePair<CardBase, byte[]>> LoadImages(string setName)
    {
        if (!_imageSetConfigs.TryGetValue(setName, out ImageSetConfig? config))
        {
            throw new ArgumentException("An image set with the given name doesn't exist.", nameof(setName));
        }

        foreach (var filePath in Directory.GetFiles(config.Path!, $"*.{config.Extension}"))
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var matches = Regex.Matches(fileName, config.Pattern!);
            if (matches.Count != 1) continue;

            var match = matches[0];
            // Get color & symbol of card image by using the captured groups from the filename
            // and the name maps defined in the config file if given.
            var aliasColor = match.Groups["color"].Value;
            var color = (config.ColorMap != null && config.ColorMap.ContainsKey(aliasColor)) ?
                config.ColorMap![aliasColor] :
                aliasColor;

            var aliasSymbol = match.Groups["symbol"].Value;
            var symbol = (config.SymbolMap != null && config.SymbolMap.ContainsKey(aliasSymbol)) ?
                config.SymbolMap[aliasSymbol] :
                aliasSymbol;

            var card = CardBase.GetByIdentifier($"{color}_{symbol}");
            // Skip images for which no card exists
            if (card == null) continue;

            // Load image from file
            var imgRaw = File.ReadAllBytes(filePath);

            yield return new KeyValuePair<CardBase, byte[]>(card, imgRaw);
        }
    }
}
