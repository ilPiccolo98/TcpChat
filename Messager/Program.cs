using System;

namespace Messager
{
    class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Insert a name:");
            string name = Console.ReadLine();
            TcpMessager messager = new TcpMessager(name, "localhost", 5000);
            messager.Connect();
            messager.Run();
            Console.WriteLine("Messager Closed");
            Console.ReadKey();
        }

    }
}
