using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    internal class TcpServer
    {
        public TcpServer(int port)
        {
            runServer = false;
            listener = new TcpListener(IPAddress.Any, port);
            viewers = new List<TcpClient>();
            messagers = new List<TcpClient>();
            messages = new Queue<string>();
        }

        public void Start()
        {
            Console.WriteLine("Server is starting");
            runServer = true;
            listener.Start();
            Console.WriteLine("Server started");
            while(runServer)
            {
                if (listener.Pending())
                    HandleNewConnections();
                GetMessages();
                SendMessages();
                RemoveDisconnectedClients(viewers);
            }
            Console.WriteLine("Server is closing");
            CleanUpCollection(viewers);
            CleanUpCollection(messagers);
            listener.Stop();
        }

        public void Shutdown()
        {
            Console.WriteLine("Shuting down...");
            runServer = false;
        }

        private void HandleNewConnections()
        {
            TcpClient client = listener.AcceptTcpClient();
            client.ReceiveBufferSize = bufferSize;
            client.SendBufferSize = bufferSize;
            EndPoint endPoint = client.Client.RemoteEndPoint;
            Console.WriteLine($"{endPoint} is trying to connect");
            byte[] rawData = new byte[bufferSize];
            int bytesRead = client.GetStream().Read(rawData, 0, bufferSize);
            if(bytesRead > 0)
            {
                Console.WriteLine($"{endPoint} has sent something");
                string data = Encoding.UTF8.GetString(rawData, 0, bytesRead);
                if (data == "viewer")
                {
                    Console.WriteLine($"{endPoint} is a viewer");
                    viewers.Add(client);
                }
                else if(data.StartsWith("messager:"))
                {
                    string name = data.Substring(data.IndexOf(":") + 1);
                    if(name.Length > 0)
                    {
                        Console.WriteLine($"{endPoint} is a messager and it's called {name}");
                        messagers.Add(client);
                        messages.Enqueue($"{name} with endpoint {endPoint} has joined the chat");
                    }
                    else
                    {
                        Console.WriteLine($"{endPoint} not recognised");
                        CleanUp(client);
                    }
                }
                else
                {
                    Console.WriteLine($"{endPoint} not recognised");
                    CleanUp(client);
                }
            }
            else
            {
                Console.WriteLine($"{endPoint} has nothing to send");
                CleanUp(client);
            }
        }

        private void CleanUp(TcpClient client)
        {
            if (client.Connected)
            {
                client.GetStream().Close();
                client.Close();
            }
        }

        private void CleanUpCollection(ICollection<TcpClient> clients)
        {
            foreach (TcpClient client in clients.ToArray())
                CleanUp(client);
            clients.Clear();
        }

        private void RemoveDisconnectedClients(ICollection<TcpClient> clients)
        {
            foreach (TcpClient client in clients.ToArray())
                if (IsDisconnected(client))
                {
                    Console.WriteLine($"{client.Client.RemoteEndPoint} has left the room");
                    clients.Remove(client);
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

        private void GetMessages()
        {
           foreach(TcpClient client in messagers)
                if(!IsDisconnected(client) && client.Available > 0)
                {
                    byte[] rawData = new byte[client.Available];
                    client.GetStream().Read(rawData, 0, rawData.Length);
                    string data = Encoding.UTF8.GetString(rawData, 0, rawData.Length);
                    Console.WriteLine($"Getting message: {data}");
                    messages.Enqueue(data);
                    Console.WriteLine($"Queue count: {messages.Count}");
                }
        }

        private void SendMessages()
        {
            foreach(string message in messages)
            {
                Console.WriteLine($"Sending message {message}");
                foreach (TcpClient client in viewers)
                {
                    Console.WriteLine($"To: {client.Client.RemoteEndPoint}");
                    byte[] rawData = Encoding.UTF8.GetBytes(message);
                    client.GetStream().Write(rawData, 0, rawData.Length);
                }
            }
            messages.Clear();
        }

        private bool runServer;
        private TcpListener listener;
        private List<TcpClient> viewers;
        private List<TcpClient> messagers;
        private Queue<string> messages;
        private const int bufferSize = 1024 * 2;
    }
}
