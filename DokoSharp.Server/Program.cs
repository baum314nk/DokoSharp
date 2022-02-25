// Setup logging
using DokoSharp.Server;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(Serilog.Events.LogEventLevel.Debug)
    .CreateLogger();

Console.Write("Enter IP (default 127.0.0.1): ");
string ipAddress = Console.ReadLine()!;
if (ipAddress == string.Empty) ipAddress = "127.0.0.1";
//string ipAddress = "127.0.0.1";
Console.Write("Enter port (default 1234): ");
string portStr = Console.ReadLine()!;
if (portStr == string.Empty) portStr = "1234";
int port = int.Parse(portStr);
//int port = 1234;

TcpServer server = new(ipAddress, port);
server.Start();

Console.ReadLine();