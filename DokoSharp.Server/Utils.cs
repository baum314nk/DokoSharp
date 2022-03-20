using DokoSharp.Server.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DokoSharp.Server;

public static class Utils
{
    public static readonly JsonSerializerOptions DefaultJsonOptions;
    public static readonly JsonSerializerOptions BeautifyJsonOptions;

    static Utils()
    {
        DefaultJsonOptions = new();
        DefaultJsonOptions.Converters.Add(new MessageJsonConverter());
        BeautifyJsonOptions = new(DefaultJsonOptions) { WriteIndented = true };
    }
}