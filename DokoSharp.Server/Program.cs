// Setup logging
using DokoSharp.Server;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

Console.Write("Enter IP (default 127.0.0.1): ");
//string ipAddress = Console.ReadLine()!;
//if (ipAddress == string.Empty) ipAddress = "127.0.0.1";
string ipAddress = "127.0.0.1";
Console.Write("Enter port (default 1234): ");
//string portStr = Console.ReadLine()!;
//if (portStr == string.Empty) portStr = "1234";
//int port = int.Parse(portStr);
int port = 1234;

TcpServer server = new(ipAddress, port);
while(true)
{
    try
    {
        server.Start();
    } catch (Exception ex)
    {
        Log.Error(ex.Message);
    }
    finally
    {
        server.Stop();
        Log.Information("Restarting the server...");
    }
}
