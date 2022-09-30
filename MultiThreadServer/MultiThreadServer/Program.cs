using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace MultiThreadServer
{
    class Program
    {
        const int port = 8888;
        static TcpListener listener;
        public static List<TcpClient> clients = new List<TcpClient>();
        public static List<string> history = new List<string>();
        static void Main(string[] args)
        {
            try
            {
                listener = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
                listener.Start();
                Console.WriteLine("Ожидание подключений...");

                while (true)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    ClientObject clientObject = new ClientObject(client);
                    Console.WriteLine("Новый пользователь подключён");
                    clients.Add(client);
                    if (clients.Count > 1 && history.Count > 0)
                    {
                        clientObject.SendHistory();
                    }
                    // новый поток для нового клиента
                    Thread clientThread = new Thread(new ThreadStart(clientObject.Process));
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (listener != null)
                    listener.Stop();
            }
        }
    }
}