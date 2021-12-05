using System;

namespace Server
{
    class Program
    {
        public static void Main(string[] args)
        {
            Console.CancelKeyPress += Console_CancelKeyPress;
            server.Start();
            Console.WriteLine("Server Closed");
            Console.ReadKey();
        }

        private static void Console_CancelKeyPress(object? sender, ConsoleCancelEventArgs e)
        {
            server.Shutdown();
            e.Cancel = true;
        }

        private static TcpServer server = new TcpServer(5000);
    }
}