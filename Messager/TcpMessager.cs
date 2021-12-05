using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Messager
{
    internal class TcpMessager
    {
        public TcpMessager(string name, string serverAddress, int port)
        {
            this.name = name;
            this.serverAddress = serverAddress; 
            this.port = port;
            server = new TcpClient();
            runMessager = false;
        }

        public void Connect()
        {
            try
            {
                Console.WriteLine("Connecting to the server");
                server.Connect(serverAddress, port);
                Console.WriteLine("Connected to the server");
                string data = $"messager:{name}";
                byte[] rawData = Encoding.UTF8.GetBytes(data, 0, data.Length);
                server.GetStream().Write(rawData, 0, rawData.Length);   
            }
            catch(SocketException e)
            {
                Console.WriteLine("Error! No server connection");
                CleanUp(server);
            }
        }

        public void Run()
        {
            if(!IsDisconnected(server) && server.Connected)
            {
                Console.WriteLine("Server running...");
                server.ReceiveBufferSize = bufferSize;
                server.SendBufferSize = bufferSize;
                runMessager = true;
                while(runMessager)
                {
                    if (IsDisconnected(server))
                    {
                        Console.WriteLine("Disconnected to the server");
                        runMessager = false;
                    }
                    else
                    {
                        Console.Write($"{name} > ");
                        string data = Console.ReadLine();
                        if (data?.Length > 0 && (data == "exit" || data == "quit"))
                            Shutdown();
                        else if(data?.Length > 0)
                        {
                            byte[] rawData = Encoding.UTF8.GetBytes(data, 0, data.Length);
                            server.GetStream().Write(rawData, 0, rawData.Length);
                        }
                    }
                }
                CleanUp(server);
            }
            else
            {
                Console.WriteLine("Disconnected to the server, impossible to run");
                CleanUp(server);
            }
        }

        public void Shutdown()
        {
            runMessager = false;
        }

        private void CleanUp(TcpClient client)
        {
            if (client.Connected)
            {
                client.GetStream().Close();
                client.Close();
            }
        }

        private static bool IsDisconnected(TcpClient client)
        {
            try
            {
                Socket s = client.Client;
                return s.Poll(10 * 1000, SelectMode.SelectRead) && (s.Available == 0);
            }
            catch (SocketException se)
            {
                return true;
            }
        }

        private string name;
        private string serverAddress;
        private int port;
        private TcpClient server;
        private bool runMessager;
        private const int bufferSize = 1024 * 2;
    }
}
