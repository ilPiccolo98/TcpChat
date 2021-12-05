using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Viewer
{
    internal class TcpViewer
    {
        public TcpViewer(string serverAddress, int port)
        {
            this.serverAddress = serverAddress;
            this.port = port;
            server = new TcpClient();
            runViewer = false;
        }

        public void Connect()
        {
            try
            {
                Console.WriteLine("Connecting to the server");
                server.Connect(serverAddress, port);
                Console.WriteLine("Connected to the server");
                string data = "viewer";
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
                runViewer = true;
                while (runViewer)
                {
                    if (IsDisconnected(server))
                    {
                        Console.WriteLine("Disconnected to the server");
                        runViewer = false;
                    }
                    else if (server.Available > 0)
                    {
                        byte[] rawData = new byte[server.Available];
                        server.GetStream().Read(rawData, 0, rawData.Length);
                        string data = Encoding.UTF8.GetString(rawData, 0, rawData.Length);
                        Console.WriteLine(data);
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
            Console.WriteLine("Shuting down...");
            runViewer = false;
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

        private void CleanUp(TcpClient client)
        {
            if(client.Connected)
            {
                client.GetStream().Close();
                client.Close();
            }
        }

        private string serverAddress;
        private int port;
        private TcpClient server;
        private bool runViewer;
        private const int bufferSize = 1024 * 2;
    }
}
