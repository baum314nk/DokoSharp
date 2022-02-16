// Setup logging
using DokoSharp.Lib.Client;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(Serilog.Events.LogEventLevel.Debug)
    .CreateLogger();

Client client = new Client();
client.Start();