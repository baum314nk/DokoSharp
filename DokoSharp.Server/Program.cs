// Setup logging
using DokoSharp.Lib.Server;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(Serilog.Events.LogEventLevel.Debug)
    .CreateLogger();

Server server = new Server();
server.Start();