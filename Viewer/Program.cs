using System;

namespace Viewer
{
    class Program
    {
        public static void Main(string[] args)
        {
            Console.CancelKeyPress += Console_CancelKeyPress;
            viewer.Connect();
            viewer.Run();
            Console.WriteLine("Viewer Closed");
            Console.ReadKey();
        }

        private static void Console_CancelKeyPress(object? sender, ConsoleCancelEventArgs e)
        {
            viewer.Shutdown();
            e.Cancel = true;
        }

        private static TcpViewer viewer = new TcpViewer("localhost", 5000);
    }
}
